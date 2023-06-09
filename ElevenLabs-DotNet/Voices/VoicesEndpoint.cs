// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.Voices
{
    /// <summary>
    /// Access to voices created either by you or us.
    /// </summary>
    public sealed class VoicesEndpoint : BaseEndPoint
    {
        private class VoiceList
        {
            [JsonInclude]
            [JsonPropertyName("voices")]
            public IReadOnlyList<Voice> Voices { get; private set; }
        }

        private class VoiceResponse
        {
            [JsonInclude]
            [JsonPropertyName("voice_id")]
            public string VoiceId { get; private set; }
        }

        public VoicesEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string Root => "voices";

        /// <summary>
        /// Gets a list of all available voices for a user.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="IReadOnlyList{T}"/> of <see cref="Voice"/>s.</returns>
        public async Task<IReadOnlyList<Voice>> GetAllVoicesAsync(CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync(GetUrl(), cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            var voices = JsonSerializer.Deserialize<VoiceList>(responseAsString, Api.JsonSerializationOptions).Voices;
            var voiceSettingsTasks = new List<Task>();

            foreach (var voice in voices)
            {
                voiceSettingsTasks.Add(Task.Run(LocalGetVoiceSettings, cancellationToken));

                async Task LocalGetVoiceSettings()
                {
                    voice.Settings = await GetVoiceSettingsAsync(voice, cancellationToken);
                }
            }

            await Task.WhenAll(voiceSettingsTasks);
            return voices.ToList();
        }

        /// <summary>
        /// Gets the default settings for voices.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceSettings"/>.</returns>
        public async Task<VoiceSettings> GetDefaultVoiceSettingsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync(GetUrl("/settings/default"), cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonSerializer.Deserialize<VoiceSettings>(responseAsString, Api.JsonSerializationOptions);
        }

        /// <summary>
        /// Gets the settings for a specific voice.
        /// </summary>
        /// <param name="voiceId">The id of the <see cref="Voice"/> to get <see cref="VoiceSettings"/> for.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceSettings"/>.</returns>
        public async Task<VoiceSettings> GetVoiceSettingsAsync(string voiceId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(voiceId))
            {
                throw new ArgumentNullException(nameof(voiceId));
            }

            var response = await Api.Client.GetAsync(GetUrl($"/{voiceId}/settings"), cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonSerializer.Deserialize<VoiceSettings>(responseAsString, Api.JsonSerializationOptions);
        }

        /// <summary>
        /// Gets metadata about a specific voice.
        /// </summary>
        /// <param name="voiceId">The id of the <see cref="Voice"/> to get.</param>
        /// <param name="withSettings">Should the response include the <see cref="VoiceSettings"/>?</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Voice"/>.</returns>
        public async Task<Voice> GetVoiceAsync(string voiceId, bool withSettings = true, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(voiceId))
            {
                throw new ArgumentNullException(nameof(voiceId));
            }

            var response = await Api.Client.GetAsync(GetUrl($"/{voiceId}?with_settings={withSettings}"), cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Voice>(responseAsString, Api.JsonSerializationOptions);
        }

        /// <summary>
        /// Edit your settings for a specific voice.
        /// </summary>
        /// <param name="voiceId">Id of the voice settings to edit.</param>
        /// <param name="voiceSettings"><see cref="VoiceSettings"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if voice settings was successfully edited.</returns>
        public async Task<bool> EditVoiceSettingsAsync(string voiceId, VoiceSettings voiceSettings, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(voiceId))
            {
                throw new ArgumentNullException(nameof(voiceId));
            }

            var payload = JsonSerializer.Serialize(voiceSettings).ToJsonStringContent();
            var response = await Api.Client.PostAsync(GetUrl($"/{voiceId}/settings/edit"), payload, cancellationToken);
            await response.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Add a new voice to your collection of voices in VoiceLab.
        /// </summary>
        /// <param name="name">Name of the voice you want to add.</param>
        /// <param name="samplePaths">Collection of file paths to use as samples for the new voice.</param>
        /// <param name="labels">Optional, labels for the new voice.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public async Task<Voice> AddVoiceAsync(string name, IEnumerable<string> samplePaths = null, IReadOnlyDictionary<string, string> labels = null, CancellationToken cancellationToken = default)
        {
            var form = new MultipartFormDataContent();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            form.Add(new StringContent(name), "name");

            if (samplePaths != null)
            {
                samplePaths = samplePaths.ToList();

                if (samplePaths.Any())
                {
                    foreach (var sample in samplePaths)
                    {
                        if (string.IsNullOrWhiteSpace(sample))
                        {
                            continue;
                        }

                        var fileStream = File.OpenRead(sample);
                        var stream = new MemoryStream();
                        await fileStream.CopyToAsync(stream, cancellationToken);
                        form.Add(new ByteArrayContent(stream.ToArray()), "files", Path.GetFileName(sample));
                        await fileStream.DisposeAsync();
                        await stream.DisposeAsync();
                    }
                }
            }

            if (labels != null)
            {
                form.Add(new StringContent(JsonSerializer.Serialize(labels)), "labels");
            }

            var response = await Api.Client.PostAsync(GetUrl("/add"), form, cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            var voiceResponse = JsonSerializer.Deserialize<VoiceResponse>(responseAsString, Api.JsonSerializationOptions);
            var voice = await GetVoiceAsync(voiceResponse.VoiceId, cancellationToken: cancellationToken);
            return voice;
        }

        /// <summary>
        /// Edit a voice created by you.
        /// </summary>
        /// <param name="voice">The <see cref="Voice"/> to edit.</param>
        /// <param name="samplePaths">The full string paths of the <see cref="Sample"/>s to upload.</param>
        /// <param name="labels">The labels to set on the <see cref="Voice"/> description.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if voice was successfully edited.</returns>
        public async Task<bool> EditVoiceAsync(Voice voice, IEnumerable<string> samplePaths = null, IReadOnlyDictionary<string, string> labels = null, CancellationToken cancellationToken = default)
        {
            var form = new MultipartFormDataContent();

            if (voice == null)
            {
                throw new ArgumentNullException(nameof(voice));
            }

            form.Add(new StringContent(voice.Name), "name");

            if (samplePaths != null)
            {
                samplePaths = samplePaths.ToList();

                if (samplePaths.Any())
                {
                    foreach (var sample in samplePaths)
                    {
                        if (string.IsNullOrWhiteSpace(sample))
                        {
                            continue;
                        }

                        var fileStream = File.OpenRead(sample);
                        var stream = new MemoryStream();
                        await fileStream.CopyToAsync(stream, cancellationToken);
                        form.Add(new ByteArrayContent(stream.ToArray()), "files", Path.GetFileName(sample));
                        await fileStream.DisposeAsync();
                        await stream.DisposeAsync();
                    }
                }
            }

            if (labels != null)
            {
                form.Add(new StringContent(JsonSerializer.Serialize(labels)), "labels");
            }

            var response = await Api.Client.PostAsync(GetUrl($"/{voice.Id}/edit"), form, cancellationToken);
            await response.CheckResponseAsync(cancellationToken);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Delete a voice by its <see cref="Voice.Id"/>.
        /// </summary>
        /// <param name="voiceId">The id of the <see cref="Voice"/> to delete.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if voice was successfully deleted.</returns>
        public async Task<bool> DeleteVoiceAsync(string voiceId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(voiceId))
            {
                throw new ArgumentNullException(nameof(voiceId));
            }

            var response = await Api.Client.DeleteAsync(GetUrl($"/{voiceId}"), cancellationToken);
            await response.CheckResponseAsync(cancellationToken);
            return response.IsSuccessStatusCode;
        }

        #region Samples

        /// <summary>
        /// Get the audio corresponding to a sample attached to a voice.
        /// </summary>
        /// <param name="voiceId">The <see cref="Voice"/> id this <see cref="Sample"/> belongs to.</param>
        /// <param name="sampleId">The <see cref="Sample"/> id to download.</param>
        /// <param name="saveDirectory">Optional, directory to save the <see cref="Sample"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public async Task<string> GetVoiceSampleAsync(string voiceId, string sampleId, string saveDirectory = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(voiceId))
            {
                throw new ArgumentNullException(nameof(voiceId));
            }

            if (string.IsNullOrWhiteSpace(sampleId))
            {
                throw new ArgumentNullException(nameof(sampleId));
            }

            var response = await Api.Client.GetAsync(GetUrl($"/{voiceId}/samples/{sampleId}/audio"), cancellationToken);
            await response.CheckResponseAsync(cancellationToken);

            var rootDirectory = (saveDirectory ?? Directory.GetCurrentDirectory()).CreateNewDirectory(nameof(ElevenLabs));
            var downloadDirectory = rootDirectory.CreateNewDirectory(voiceId);
            var filePath = Path.Combine(downloadDirectory, $"{sampleId}.mp3");

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

            return filePath;
        }

        /// <summary>
        /// Delete the audio corresponding to a sample attached to a voice.
        /// </summary>
        /// <param name="voiceId">The <see cref="Voice"/> id this <see cref="Sample"/> belongs to.</param>
        /// <param name="sampleId">The <see cref="Sample"/> id to delete.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if <see cref="Voice"/> <see cref="Sample"/> was successfully deleted.</returns>
        public async Task<bool> DeleteVoiceSampleAsync(string voiceId, string sampleId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(voiceId))
            {
                throw new ArgumentNullException(nameof(voiceId));
            }

            if (string.IsNullOrWhiteSpace(sampleId))
            {
                throw new ArgumentNullException(nameof(sampleId));
            }

            var response = await Api.Client.DeleteAsync(GetUrl($"/{voiceId}/samples/{sampleId}"), cancellationToken);
            await response.CheckResponseAsync(cancellationToken);
            return response.IsSuccessStatusCode;
        }

        #endregion Samples
    }
}
