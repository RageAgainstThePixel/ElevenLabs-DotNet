// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.SpeechToText
{
    public sealed class SpeechToTextSession : IDisposable
    {
        /// <summary>
        /// Enable or disable logging.
        /// </summary>
        public bool EnableDebug { get; set; }

        /// <summary>
        /// The timeout in seconds to wait for a response from the server.
        /// </summary>
        public int EventTimeout { get; set; } = 30;

        #region Internal

        internal event Action<IServerEvent> OnEventReceived;

        internal event Action<Exception> OnError;

        private readonly object eventLock = new();
        private readonly WebSocket websocketClient;
        private readonly ConcurrentQueue<IServerEvent> events = new();

        private bool isCollectingEvents;

        internal SpeechToTextSession(WebSocket webSocket, bool enableDebug)
        {
            websocketClient = webSocket;
            websocketClient.OnMessage += OnMessage;
            EnableDebug = enableDebug;
        }

        private void OnMessage(DataFrame dataFrame)
        {
            if (dataFrame.Type == OpCode.Text)
            {
                if (EnableDebug)
                {
                    Console.WriteLine(dataFrame.Text);
                }

                try
                {
                    IServerEvent serverEvent = null;
                    using var doc = JsonDocument.Parse(dataFrame.Text);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("message_type", out var typeElement))
                    {
                        var messageType = typeElement.GetString();
                        serverEvent = messageType switch
                        {
                            "session_started" => JsonSerializer.Deserialize<SessionStarted>(dataFrame.Text, ElevenLabsClient.JsonSerializationOptions),
                            "partial_transcript" => JsonSerializer.Deserialize<PartialTranscript>(dataFrame.Text, ElevenLabsClient.JsonSerializationOptions),
                            "committed_transcript" => JsonSerializer.Deserialize<CommittedTranscript>(dataFrame.Text, ElevenLabsClient.JsonSerializationOptions),
                            "committed_transcript_with_timestamps" => JsonSerializer.Deserialize<CommittedTranscriptWithTimestamps>(dataFrame.Text, ElevenLabsClient.JsonSerializationOptions),
                            // Map all error types to the generic SpeechToTextError
                            var t when t != null && (t.EndsWith("error") || t == "quota_exceeded" || t == "commit_throttled" || t == "unaccepted_terms" || t == "rate_limited" || t == "queue_overflow" || t == "resource_exhausted" || t == "session_time_limit_exceeded" || t == "input_error" || t == "chunk_size_exceeded" || t == "insufficient_audio_activity")
                                => JsonSerializer.Deserialize<SpeechToTextError>(dataFrame.Text, ElevenLabsClient.JsonSerializationOptions),
                            _ => null
                        };
                    }

                    if (serverEvent != null)
                    {
                        lock (eventLock)
                        {
                            events.Enqueue(serverEvent);
                        }

                        OnEventReceived?.Invoke(serverEvent);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    OnError?.Invoke(e);
                }
            }
        }

        ~SpeechToTextSession() => Dispose(false);

        internal async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            var connectTcs = new TaskCompletionSource<State>();
            websocketClient.OnOpen += OnWebsocketClientOnOpen;
            websocketClient.OnError += OnWebsocketClientOnError;

            try
            {
                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                websocketClient.Connect();
                await connectTcs.Task.WithCancellation(cancellationToken).ConfigureAwait(false);

                if (websocketClient.State != State.Open)
                {
                    throw new Exception($"Failed to start new session! {websocketClient.State}");
                }
            }
            finally
            {
                websocketClient.OnOpen -= OnWebsocketClientOnOpen;
                websocketClient.OnError -= OnWebsocketClientOnError;
            }

            return;

            void OnWebsocketClientOnError(Exception e)
                => connectTcs.TrySetException(e);

            void OnWebsocketClientOnOpen()
                => connectTcs.TrySetResult(websocketClient.State);
        }

        #region IDisposable

        private bool isDisposed;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed && disposing)
            {
                websocketClient.OnMessage -= OnMessage;
                websocketClient.Dispose();
                isDisposed = true;
            }
        }

        #endregion IDisposable

        #endregion Internal

        /// <summary>
        /// Receive callback updates from the server.
        /// </summary>
        /// <typeparam name="T"><see cref="ISpeechToTextServerEvent"/> to subscribe for updates to.</typeparam>
        /// <param name="sessionEvent">The event to receive updates for.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Task"/>.</returns>
        public async Task ReceiveUpdatesAsync<T>(Action<T> sessionEvent, CancellationToken cancellationToken) where T : IServerEvent
        {
            try
            {
                lock (eventLock)
                {
                    if (isCollectingEvents)
                    {
                        throw new Exception($"{nameof(ReceiveUpdatesAsync)} is already running!");
                    }

                    isCollectingEvents = true;
                }

                do
                {
                    try
                    {
                        T @event = default;

                        lock (eventLock)
                        {
                            if (events.TryDequeue(out var dequeuedEvent) &&
                                dequeuedEvent is T typedEvent)
                            {
                                @event = typedEvent;
                            }
                        }

                        if (@event != null)
                        {
                            sessionEvent(@event);
                        }

                        await Task.Yield();
                    }
                    catch (Exception e)
                    {
                        switch (e)
                        {
                            case TaskCanceledException:
                            case OperationCanceledException:
                                break;
                            default:
                                Console.WriteLine(e);
                                break;
                        }
                    }
                } while (!cancellationToken.IsCancellationRequested && websocketClient.State == State.Open);
            }
            finally
            {
                lock (eventLock)
                {
                    isCollectingEvents = false;
                }
            }
        }

        /// <summary>
        /// Send an audio chunk to the server.
        /// </summary>
        /// <param name="audioChunk">The audio chunk to send.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Task"/>.</returns>
        public async Task SendAudioChunkAsync(InputAudioChunk audioChunk, CancellationToken cancellationToken = default)
        {
            if (websocketClient.State != State.Open)
            {
                throw new Exception($"Websocket connection is not open! {websocketClient.State}");
            }

            var payload = audioChunk.ToJsonString();

            if (EnableDebug)
            {
                Console.WriteLine(payload);
            }

            await websocketClient.SendAsync(payload, cancellationToken).ConfigureAwait(false);
        }
    }
}