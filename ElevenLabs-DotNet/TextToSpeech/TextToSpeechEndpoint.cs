// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using ElevenLabs.Models;
using ElevenLabs.Voices;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.TextToSpeech
{
    /// <summary>
    /// Access to convert text to synthesized speech.
    /// </summary>
    public sealed class TextToSpeechEndpoint : BaseEndPoint
    {
        public TextToSpeechEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string Root => "text-to-speech";

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio.
        /// </summary>
        /// <param name="text">Text input to synthesize speech for. Maximum 5000 characters.</param>
        /// <param name="voice"><see cref="Voice"/> to use.</param>
        /// <param name="voiceSettings">Optional, <see cref="VoiceSettings"/> that will override the default settings in <see cref="Voice.Settings"/>.</param>
        /// <param name="model">Optional, <see cref="Model"/> to use. Defaults to <see cref="Model.MonoLingualV1"/>.</param>
        /// <param name="saveDirectory">Optional, The save directory to save the audio clip.</param>
        /// <param name="deleteCachedFile">Optional, deletes the cached file for this text string. Default is false.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>Downloaded clip path.</returns>
        public async Task<string> TextToSpeechAsync(string text, Voice voice, VoiceSettings voiceSettings = null, Model model = null, string saveDirectory = null, bool deleteCachedFile = false, CancellationToken cancellationToken = default)
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

            if (string.IsNullOrWhiteSpace(voice.Name))
            {
                Console.WriteLine("Voice details not found! To speed up this call, cache the voice details before making this request.");
                voice = await Api.VoicesEndpoint.GetVoiceAsync(voice, cancellationToken: cancellationToken);
            }

            var rootDirectory = (saveDirectory ?? Directory.GetCurrentDirectory()).CreateNewDirectory(nameof(ElevenLabs));
            var speechToTextDirectory = rootDirectory.CreateNewDirectory(nameof(TextToSpeech));
            var downloadDirectory = speechToTextDirectory.CreateNewDirectory(voice.Name);
            var clipGuid = $"{voice.Id}{text}".GenerateGuid().ToString();
            var fileName = $"{clipGuid}.mp3";
            var filePath = Path.Combine(downloadDirectory, fileName);

            if (File.Exists(filePath) && deleteCachedFile)
            {
                File.Delete(filePath);
            }

            if (!File.Exists(filePath))
            {
                var defaultVoiceSettings = voiceSettings ?? voice.Settings ?? await Api.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
                var payload = JsonSerializer.Serialize(new TextToSpeechRequest(text, model ?? Model.MonoLingualV1, defaultVoiceSettings)).ToJsonStringContent();
                var response = await Api.Client.PostAsync(GetUrl($"/{voice.Id}"), payload, cancellationToken);
                await response.CheckResponseAsync(cancellationToken);

#if NET48
                var responseStream = await response.Content.ReadAsStreamAsync();
#else
                var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
#endif


                try
                {
                    var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);

                    try
                    {

                        await responseStream.CopyToAsync(fileStream, 81920, cancellationToken);
                        await fileStream.FlushAsync(cancellationToken);
                    }
                    finally
                    {
                        fileStream.Close();
#if !NET48
                        await fileStream.DisposeAsync();
#endif

                    }
                }
                finally
                {
#if !NET48
                    await responseStream.DisposeAsync();
#endif
                }
            }

            return filePath;
        }
    }
}
