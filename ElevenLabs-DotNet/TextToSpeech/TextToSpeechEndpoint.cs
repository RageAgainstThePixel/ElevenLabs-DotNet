// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using ElevenLabs.Models;
using ElevenLabs.Voices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        public TextToSpeechEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "text-to-speech";

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio.
        /// </summary>
        /// <param name="text">
        /// Text input to synthesize speech for. Maximum 5000 characters.
        /// </param>
        /// <param name="voice">
        /// <see cref="Voice"/> to use.
        /// </param>
        /// <param name="voiceSettings">
        /// Optional, <see cref="VoiceSettings"/> that will override the default settings in <see cref="Voice.Settings"/>.
        /// </param>
        /// <param name="model">
        /// Optional, <see cref="Model"/> to use. Defaults to <see cref="Model.MonoLingualV1"/>.
        /// </param>
        /// <param name="outputFormat">
        /// Output format of the generated audio.<br/>
        /// Defaults to <see cref="OutputFormat.MP3_44100_128"/>
        /// </param>
        /// <param name="optimizeStreamingLatency">
        /// Optional, You can turn on latency optimizations at some cost of quality.
        /// The best possible final latency varies by model.<br/>
        /// Possible values:<br/>
        /// 0 - default mode (no latency optimizations)<br/>
        /// 1 - normal latency optimizations (about 50% of possible latency improvement of option 3)<br/>
        /// 2 - strong latency optimizations (about 75% of possible latency improvement of option 3)<br/>
        /// 3 - max latency optimizations<br/>
        /// 4 - max latency optimizations, but also with text normalizer turned off for even more latency savings
        /// (best latency, but can mispronounce eg numbers and dates).
        /// </param>
        /// <param name="partialClipCallback">
        /// Optional, Callback to enable streaming audio as it comes in.<br/>
        /// Returns partial <see cref="VoiceClip"/>.
        /// </param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceClip"/>.</returns>
        public async Task<VoiceClip> TextToSpeechAsync(string text, Voice voice, VoiceSettings voiceSettings = null, Model model = null, OutputFormat outputFormat = OutputFormat.MP3_44100_128, int? optimizeStreamingLatency = null, Func<VoiceClip, Task> partialClipCallback = null, CancellationToken cancellationToken = default)
        {
            if (text.Length > 5000)
            {
                throw new ArgumentOutOfRangeException(nameof(text), $"{nameof(text)} cannot exceed 5000 characters");
            }

            if (voice == null ||
                string.IsNullOrWhiteSpace(voice.Id))
            {
                throw new ArgumentNullException(nameof(voice));
            }

            var defaultVoiceSettings = voiceSettings ?? voice.Settings ?? await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
            using var payload = JsonSerializer.Serialize(new TextToSpeechRequest(text, model, defaultVoiceSettings)).ToJsonStringContent();
            var parameters = new Dictionary<string, string>
            {
                { OutputFormatParameter, outputFormat.ToString().ToLower() }
            };

            if (optimizeStreamingLatency.HasValue)
            {
                parameters.Add(OptimizeStreamingLatencyParameter, optimizeStreamingLatency.ToString());
            }

            using var postRequest = new HttpRequestMessage(HttpMethod.Post, GetUrl($"/{voice.Id}{(partialClipCallback == null ? string.Empty : "/stream")}", parameters));
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

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var memoryStream = new MemoryStream();
            int bytesRead;
            var totalBytesRead = 0;
            var buffer = new byte[8192];

            while ((bytesRead = await responseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await memoryStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);

                if (partialClipCallback != null)
                {
                    try
                    {
                        await partialClipCallback(new VoiceClip(clipId, text, voice, new ReadOnlyMemory<byte>(memoryStream.GetBuffer(), totalBytesRead, bytesRead))).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                totalBytesRead += bytesRead;
            }

            return new VoiceClip(clipId, text, voice, new ReadOnlyMemory<byte>(memoryStream.GetBuffer(), 0, totalBytesRead));
        }
    }
}
