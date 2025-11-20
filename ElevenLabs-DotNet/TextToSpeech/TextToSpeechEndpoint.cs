// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using ElevenLabs.Models;
using ElevenLabs.Voices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.TextToSpeech
{
    /// <summary>
    /// Access to convert text to synthesized speech.
    /// </summary>
    public sealed class TextToSpeechEndpoint : ElevenLabsBaseEndPoint
    {
        private const string HistoryItemId = "history-item-id";
        private const string OutputFormatParameter = "output_format";
        private const string OptimizeStreamingLatencyParameter = "optimize_streaming_latency";
        private const string EnableLoggingParameter = "enable_logging";

        public TextToSpeechEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "text-to-speech";

        [Obsolete("use overload with TextToSpeechRequest")]
        public async Task<VoiceClip> TextToSpeechAsync(string text, Voice voice, VoiceSettings voiceSettings = null, Model model = null, OutputFormat outputFormat = OutputFormat.MP3_44100_128, int? optimizeStreamingLatency = null, Func<VoiceClip, Task> partialClipCallback = null, CancellationToken cancellationToken = default)
        {
            var defaultVoiceSettings = voiceSettings ?? voice.Settings ?? await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
            return await TextToSpeechAsync(new TextToSpeechRequest(voice, text, Encoding.UTF8, defaultVoiceSettings, outputFormat, optimizeStreamingLatency, model), partialClipCallback, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio.
        /// </summary>
        /// <param name="request"><see cref="TextToSpeechRequest"/>.</param>
        /// <param name="partialClipCallback">
        /// Optional, Callback to enable streaming audio as it comes in.<br/>
        /// Returns partial <see cref="VoiceClip"/>.
        /// </param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceClip"/>.</returns>
        public async Task<VoiceClip> TextToSpeechAsync(TextToSpeechRequest request, Func<VoiceClip, Task> partialClipCallback = null, CancellationToken cancellationToken = default)
        {
            request.VoiceSettings ??= await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
            using var payload = JsonSerializer.Serialize(request, ElevenLabsClient.JsonSerializationOptions).ToJsonStringContent();
            var parameters = new Dictionary<string, string>
            {
                { OutputFormatParameter, request.OutputFormat.ToString().ToLower() }
            };

#pragma warning disable CS0618 // Type or member is obsolete
            if (request.OptimizeStreamingLatency.HasValue)
            {
                parameters.Add(OptimizeStreamingLatencyParameter, request.OptimizeStreamingLatency.Value.ToString());
            }
#pragma warning restore CS0618 // Type or member is obsolete
            
            if (request.EnableLogging.HasValue)
            {
                parameters.Add(EnableLoggingParameter, request.EnableLogging.GetValueOrDefault() ? "true" : "false");
            }

            var endpoint = $"/{request.Voice.Id}";

            if (partialClipCallback != null)
            {
                endpoint += "/stream";
            }

            if (request.WithTimestamps)
            {
                endpoint += "/with-timestamps";
            }

            using var postRequest = new HttpRequestMessage(HttpMethod.Post, GetUrl(endpoint, parameters));
            postRequest.Content = payload;
            var requestOption = partialClipCallback == null
                ? HttpCompletionOption.ResponseContentRead
                : HttpCompletionOption.ResponseHeadersRead;
            using var response = await client.Client.SendAsync(postRequest, requestOption, cancellationToken);
            await response.CheckResponseAsync(EnableDebug, payload, cancellationToken).ConfigureAwait(false);
            var clipId = response.Headers.GetValues(HistoryItemId).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(clipId))
            {
                throw new ArgumentException("Failed to parse clip id!");
            }

            return request.WithTimestamps
                ? await StreamWithTimeStampsAsync(response).ConfigureAwait(false)
                : await StreamAsync(response).ConfigureAwait(false);

            async Task<VoiceClip> StreamWithTimeStampsAsync(HttpResponseMessage messageResponse)
            {
                await using var audioDataStream = new MemoryStream();
                var accumulatedTranscriptData = new List<TimestampedTranscriptCharacter>();
                await using var stream = await messageResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using var reader = new StreamReader(stream);

                while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
                {
                    const string data = "data: ";
                    const string done = "[DONE]";

                    if (line.StartsWith(data)) { line = line[data.Length..]; }
                    if (line == done) { break; }
                    if (string.IsNullOrWhiteSpace(line)) { continue; }

                    var transcriptData = JsonSerializer.Deserialize<TranscriptionResponse>(line, ElevenLabsClient.JsonSerializationOptions);
                    var timestampedTranscriptCharacters = (TimestampedTranscriptCharacter[])transcriptData.Alignment ?? [];

                    if (partialClipCallback != null)
                    {
                        try
                        {
                            var partialClip = new VoiceClip(clipId, request.Text, request.Voice, new ReadOnlyMemory<byte>(transcriptData.AudioBytes), request.OutputFormat.GetSampleRate())
                            {
                                TimestampedTranscriptCharacters = timestampedTranscriptCharacters
                            };
                            await partialClipCallback(partialClip).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                    accumulatedTranscriptData.AddRange(timestampedTranscriptCharacters);
                    await audioDataStream.WriteAsync(transcriptData.AudioBytes, 0, transcriptData.AudioBytes.Length, cancellationToken).ConfigureAwait(false);
                }

                return new VoiceClip(clipId, request.Text, request.Voice, new ReadOnlyMemory<byte>(audioDataStream.GetBuffer(), 0, (int)audioDataStream.Length), request.OutputFormat.GetSampleRate())
                {
                    TimestampedTranscriptCharacters = accumulatedTranscriptData.ToArray()
                };
            }

            async Task<VoiceClip> StreamAsync(HttpResponseMessage messageResponse)
            {
                int bytesRead;
                var totalBytesRead = 0;
                var buffer = new byte[8192];
                await using var audioDataStream = new MemoryStream();
                await using var responseStream = await messageResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

                while ((bytesRead = await responseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await audioDataStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);

                    if (partialClipCallback != null)
                    {
                        try
                        {
                            var partialClip = new VoiceClip(clipId, request.Text, request.Voice, new ReadOnlyMemory<byte>(audioDataStream.GetBuffer(), totalBytesRead, bytesRead), request.OutputFormat.GetSampleRate());
                            await partialClipCallback(partialClip).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                    totalBytesRead += bytesRead;
                }

                return new VoiceClip(clipId, request.Text, request.Voice, new ReadOnlyMemory<byte>(audioDataStream.GetBuffer(), 0, totalBytesRead), request.OutputFormat.GetSampleRate());
            }
        }
    }
}
