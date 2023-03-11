// Licensed under the MIT License. See LICENSE in the project root for license information.

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

        protected override string GetEndpoint()
            => $"{Api.BaseUrl}voice-generation";

        /// <summary>
        /// Gets the available voice generation options.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="GeneratedVoiceOptions"/>.</returns>
        public async Task<GeneratedVoiceOptions> GetVoiceGenerationOptionsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync($"{GetEndpoint()}/generate-voice/parameters", cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GeneratedVoiceOptions>(responseAsString, Api.JsonSerializationOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="generatedVoiceRequest"><see cref="GeneratedVoiceRequest"/></param>
        /// <param name="saveDirectory">The save directory for downloaded audio file.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Tuple{VoiceId,FilePath}"/>.</returns>
        public async Task<Tuple<string, string>> GenerateVoiceAsync(GeneratedVoiceRequest generatedVoiceRequest, string saveDirectory, CancellationToken cancellationToken = default)
        {
            var payload = JsonSerializer.Serialize(generatedVoiceRequest, Api.JsonSerializationOptions).ToJsonStringContent();
            var response = await Api.Client.PostAsync($"{GetEndpoint()}/generate-voice", payload, cancellationToken);
            await response.CheckResponseAsync(cancellationToken);

            var generatedVoiceId = response.Headers.FirstOrDefault(pair => pair.Key == "generated_voice_id").Value.FirstOrDefault();
            var rootDirectory = saveDirectory.CreateNewDirectory(nameof(ElevenLabs));
            var downloadDirectory = rootDirectory.CreateNewDirectory(nameof(VoiceGeneration));
            var filePath = Path.Combine(downloadDirectory, $"{generatedVoiceId}.mp3");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

            try
            {
                var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);

                try
                {
                    await responseStream.CopyToAsync(fileStream, cancellationToken);
                    await fileStream.FlushAsync(cancellationToken);
                }
                finally
                {
                    fileStream.Close();
                    await fileStream.DisposeAsync();
                }
            }
            finally
            {
                await responseStream.DisposeAsync();
            }

            return new Tuple<string, string>(generatedVoiceId, filePath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createVoiceRequest"><see cref="CreateVoiceRequest"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Voice"/>.</returns>
        public async Task<Voice> CreateVoiceAsync(CreateVoiceRequest createVoiceRequest, CancellationToken cancellationToken = default)
        {
            var payload = JsonSerializer.Serialize(createVoiceRequest).ToJsonStringContent();
            var response = await Api.Client.PostAsync($"{GetEndpoint()}/create-voice", payload, cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Voice>(responseAsString, Api.JsonSerializationOptions);
        }
    }
}
