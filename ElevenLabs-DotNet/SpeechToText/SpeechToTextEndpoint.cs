// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.SpeechToText
{
    /// <summary>
    /// Access to realtime speech-to-text transcription.
    /// </summary>
    public sealed class SpeechToTextEndpoint : ElevenLabsBaseEndPoint
    {
        public SpeechToTextEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "speech-to-text";

        /// <summary>
        /// Transcribe an audio file.
        /// </summary>
        /// <param name="request"><see cref="SpeechToTextRequest"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SpeechToTextResponse"/>.</returns>
        public async Task<SpeechToTextResponse> ConvertAsync(SpeechToTextRequest request, CancellationToken cancellationToken = default)
        {
            using var content = new MultipartFormDataContent();

            // Add Model ID
            content.Add(new StringContent(request.ModelId), "model_id");

            // Add File
            var fileContent = new StreamContent(request.AudioFile);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "file", "audio.mp3"); // Filename is required by many servers, even if arbitrary

            // Optional Params
            if (!string.IsNullOrWhiteSpace(request.LanguageCode))
            {
                content.Add(new StringContent(request.LanguageCode), "language_code");
            }

            content.Add(new StringContent(request.TagAudioEvents.ToString().ToLower()), "tag_audio_events");

            if (request.NumSpeakers.HasValue)
            {
                content.Add(new StringContent(request.NumSpeakers.Value.ToString()), "num_speakers");
            }

            content.Add(new StringContent(request.TimestampsGranularity), "timestamps_granularity");
            content.Add(new StringContent(request.Diarize.ToString().ToLower()), "diarize");

            using var postRequest = new HttpRequestMessage(HttpMethod.Post, GetUrl("speech-to-text"));
            postRequest.Content = content;

            using var response = await client.Client.SendAsync(postRequest, cancellationToken);

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(EnableDebug, cancellationToken).ConfigureAwait(false);

            return JsonSerializer.Deserialize<SpeechToTextResponse>(responseString, ElevenLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Creates a new Speech-to-Text WebSocket session.
        /// </summary>
        /// <param name="configuration"><see cref="SpeechToTextSessionConfiguration"/> options.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SpeechToTextSession"/>.</returns>
        public async Task<SpeechToTextSession> CreateSpeechToTextSessionAsync(SpeechToTextSessionConfiguration configuration, CancellationToken cancellationToken = default)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var endpoint = GetWebsocketUri("/realtime", configuration.ToQueryParams());
            var websocket = new WebSocket(endpoint, new Dictionary<string, string>
            {
                { "User-Agent", "ElevenLabs-DotNet" },
                { "xi-api-key", client.ElevenLabsAuthentication.ApiKey }
            });

            var session = new SpeechToTextSession(websocket, EnableDebug);
            var initializeSessionTcs = new TaskCompletionSource<bool>();

            try
            {
                session.OnEventReceived += OnEventReceived;
                session.OnError += OnError;

                await session.ConnectAsync(cancellationToken).ConfigureAwait(false);

                // Wait for the session to be established (receiving session_started)
                await initializeSessionTcs.Task.WithCancellation(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                session.OnError -= OnError;
                session.OnEventReceived -= OnEventReceived;
            }

            return session;

            void OnError(Exception e)
                => initializeSessionTcs.TrySetException(e);

            void OnEventReceived(IServerEvent @event)
            {
                try
                {
                    if (@event is SessionStarted)
                    {
                        initializeSessionTcs.TrySetResult(true);
                    }
                    else if (@event is SpeechToTextError error)
                    {
                        initializeSessionTcs.TrySetException(new Exception($"Server Error: {error.ErrorMessage}"));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    initializeSessionTcs.TrySetException(e);
                }
            }
        }
    }
}