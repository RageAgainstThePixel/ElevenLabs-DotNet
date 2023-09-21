// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using ElevenLabs.Voices;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.VoiceGeneration
{
    public sealed class VoiceGenerationEndpoint : BaseEndPoint
    {
        public VoiceGenerationEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string Root => "voice-generation";

        /// <summary>
        /// Gets the available voice generation options.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="GeneratedVoiceOptions"/>.</returns>
        public async Task<GeneratedVoiceOptions> GetVoiceGenerationOptionsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync(GetUrl("/generate-voice/parameters"), cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GeneratedVoiceOptions>(responseAsString, Api.JsonSerializationOptions);
        }

        /// <summary>
        /// Generate a <see cref="Voice"/>.
        /// </summary>
        /// <param name="generatedVoiceRequest"><see cref="GeneratedVoiceRequest"/></param>
        /// <param name="saveDirectory">Optional, The save directory for downloaded audio file.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Tuple{VoiceId,FilePath}"/>.</returns>
        public async Task<Tuple<string, string>> GenerateVoiceAsync(GeneratedVoiceRequest generatedVoiceRequest, string saveDirectory = null, CancellationToken cancellationToken = default)
        {
            var payload = JsonSerializer.Serialize(generatedVoiceRequest, Api.JsonSerializationOptions).ToJsonStringContent();
            var response = await Api.Client.PostAsync(GetUrl("/generate-voice"), payload, cancellationToken);
            await response.CheckResponseAsync(cancellationToken);

            var generatedVoiceId = response.Headers.FirstOrDefault(pair => pair.Key == "generated_voice_id").Value.FirstOrDefault();
            var rootDirectory = (saveDirectory ?? Directory.GetCurrentDirectory()).CreateNewDirectory(nameof(ElevenLabs));
            var downloadDirectory = rootDirectory.CreateNewDirectory(nameof(VoiceGeneration));
            var filePath = Path.Combine(downloadDirectory, $"{generatedVoiceId}.mp3");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

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

            return new Tuple<string, string>(generatedVoiceId, filePath);
        }

        /// <summary>
        /// Clone a <see cref="Voice"/>.
        /// </summary>
        /// <param name="createVoiceRequest"><see cref="CreateVoiceRequest"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Voice"/>.</returns>
        public async Task<Voice> CreateVoiceAsync(CreateVoiceRequest createVoiceRequest, CancellationToken cancellationToken = default)
        {
            var payload = JsonSerializer.Serialize(createVoiceRequest).ToJsonStringContent();
            var response = await Api.Client.PostAsync(GetUrl("/create-voice"), payload, cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Voice>(responseAsString, Api.JsonSerializationOptions);
        }
    }
}
