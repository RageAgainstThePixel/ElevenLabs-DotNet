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
    public sealed class VoicesEndpoint : ElevenLabsBaseEndPoint
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

        public VoicesEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "voices";

        /// <summary>
        /// Gets a list of all available voices for a user, and downloads all their settings.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="IReadOnlyList{T}"/> of <see cref="Voice"/>s.</returns>
        public Task<IReadOnlyList<Voice>> GetAllVoicesAsync(CancellationToken cancellationToken = default)
            => GetAllVoicesAsync(true, cancellationToken);

        /// <summary>
        /// Gets a list of all available voices for a user.
        /// </summary>
        /// <param name="downloadSettings">Whether to download all settings for the voices.</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="IReadOnlyList{T}"/> of <see cref="Voice"/>s.</returns>
        public async Task<IReadOnlyList<Voice>> GetAllVoicesAsync(bool downloadSettings, CancellationToken cancellationToken = default)
        {
            var response = await client.Client.GetAsync(GetUrl(), cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var voices = JsonSerializer.Deserialize<VoiceList>(responseAsString, ElevenLabsClient.JsonSerializationOptions).Voices;

            if (downloadSettings)
            {
                var voiceSettingsTasks = new List<Task>();

                foreach (var voice in voices)
                {
                    voiceSettingsTasks.Add(Task.Run(LocalGetVoiceSettingsAsync, cancellationToken));

                    async Task LocalGetVoiceSettingsAsync()
                    {
                        voice.Settings = await GetVoiceSettingsAsync(voice, cancellationToken).ConfigureAwait(false);
                    }
                }

                await Task.WhenAll(voiceSettingsTasks).ConfigureAwait(false);
            }

            return voices.ToList();
        }

        /// <summary>
        /// Gets the default settings for voices.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceSettings"/>.</returns>
        public async Task<VoiceSettings> GetDefaultVoiceSettingsAsync(CancellationToken cancellationToken = default)
        {
            var response = await client.Client.GetAsync(GetUrl("/settings/default"), cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<VoiceSettings>(responseAsString, ElevenLabsClient.JsonSerializationOptions);
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

            var response = await client.Client.GetAsync(GetUrl($"/{voiceId}/settings"), cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<VoiceSettings>(responseAsString, ElevenLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Gets metadata about a specific voice.
        /// </summary>
        /// <param name="voiceId">The id of the <see cref="Voice"/> to get.</param>
        /// <param name="withSettings">Should the response include the <see cref="VoiceSettings"/>?</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Voice"/>.</returns>
        public async Task<Voice> GetVoiceAsync(string voiceId, bool withSettings = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(voiceId))
            {
                throw new ArgumentNullException(nameof(voiceId));
            }

            var response = await client.Client.GetAsync(GetUrl($"/{voiceId}?with_settings={withSettings}"), cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<Voice>(responseAsString, ElevenLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Edit your settings for a specific voice.
        /// </summary>
        /// <param name="voiceId">The id of the voice settings to edit.</param>
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
            var response = await client.Client.PostAsync(GetUrl($"/{voiceId}/settings/edit"), payload, cancellationToken).ConfigureAwait(false);
            await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
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
                var paths = samplePaths.Where(path => !string.IsNullOrWhiteSpace(path)).ToList();

                if (paths.Any())
                {
                    foreach (var sample in paths)
                    {
                        if (!File.Exists(sample))
                        {
                            Console.WriteLine($"No sample clip found at {sample}!");
                            continue;
                        }

                        try
                        {
                            var fileBytes = await File.ReadAllBytesAsync(sample, cancellationToken);
                            form.Add(new ByteArrayContent(fileBytes), "files", Path.GetFileName(sample));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }

            if (labels != null)
            {
                form.Add(new StringContent(JsonSerializer.Serialize(labels)), "labels");
            }

            var response = await client.Client.PostAsync(GetUrl("/add"), form, cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var voiceResponse = JsonSerializer.Deserialize<VoiceResponse>(responseAsString, ElevenLabsClient.JsonSerializationOptions);
            return await GetVoiceAsync(voiceResponse.VoiceId, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Add a new voice to your collection of voices in VoiceLab from a stream
        /// </summary>
        /// <param name="name">Name of the voice you want to add.</param>
        /// <param name="samples">Collection of samples as an array of bytes to be used for the new voice</param>
        /// <param name="labels">Optional, labels for the new voice.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public async Task<Voice> AddVoiceAsync(string name, IEnumerable<byte[]> samples, IReadOnlyDictionary<string, string> labels = null, CancellationToken cancellationToken = default)
        {
            var form = new MultipartFormDataContent();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (samples == null)
            {
                throw new ArgumentNullException(nameof(samples));
            }

            form.Add(new StringContent(name), "name");

            var fileItr = 0;

            foreach (var content in samples)
            {
                try
                {
                    form.Add(new ByteArrayContent(content), "files", $"file-{fileItr++}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (labels != null)
            {
                form.Add(new StringContent(JsonSerializer.Serialize(labels)), "labels");
            }

            var response = await client.Client.PostAsync(GetUrl("/add"), form, cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var voiceResponse = JsonSerializer.Deserialize<VoiceResponse>(responseAsString, ElevenLabsClient.JsonSerializationOptions);
            return await GetVoiceAsync(voiceResponse.VoiceId, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Add a new voice to your collection of voices in VoiceLab from a stream
        /// </summary>
        /// <param name="name">Name of the voice you want to add.</param>
        /// <param name="sampleStreams">Collection of samples as a stream to be used for the new voice</param>
        /// <param name="labels">Optional, labels for the new voice.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public async Task<Voice> AddVoiceAsync(string name, IEnumerable<Stream> sampleStreams, IReadOnlyDictionary<string, string> labels = null, CancellationToken cancellationToken = default)
        {
            var form = new MultipartFormDataContent();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (sampleStreams == null)
            {
                throw new ArgumentNullException(nameof(sampleStreams));
            }

            form.Add(new StringContent(name), "name");
            var fileItr = 0;

            foreach (var voiceStream in sampleStreams)
            {
                try
                {
                    form.Add(new StreamContent(voiceStream), "files", $"file-{fileItr++}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (labels != null)
            {
                form.Add(new StringContent(JsonSerializer.Serialize(labels)), "labels");
            }

            var response = await client.Client.PostAsync(GetUrl("/add"), form, cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var voiceResponse = JsonSerializer.Deserialize<VoiceResponse>(responseAsString, ElevenLabsClient.JsonSerializationOptions);
            return await GetVoiceAsync(voiceResponse.VoiceId, cancellationToken: cancellationToken).ConfigureAwait(false);
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
                var paths = samplePaths.Where(path => !string.IsNullOrWhiteSpace(path)).ToList();

                if (paths.Any())
                {
                    foreach (var sample in paths)
                    {
                        if (!File.Exists(sample))
                        {
                            Console.WriteLine($"No sample clip found at {sample}!");
                            continue;
                        }

                        try
                        {
                            var fileBytes = await File.ReadAllBytesAsync(sample, cancellationToken);
                            form.Add(new ByteArrayContent(fileBytes), "files", Path.GetFileName(sample));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }

            if (labels != null)
            {
                form.Add(new StringContent(JsonSerializer.Serialize(labels)), "labels");
            }

            var response = await client.Client.PostAsync(GetUrl($"/{voice.Id}/edit"), form, cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(cancellationToken).ConfigureAwait(false);
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

            var response = await client.Client.DeleteAsync(GetUrl($"/{voiceId}"), cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(cancellationToken).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        #region Samples

        /// <summary>
        /// Download the audio corresponding to a <see cref="Sample"/> attached to a <see cref="Voice"/>.
        /// </summary>
        /// <param name="voice">The <see cref="Voice"/> this <see cref="Sample"/> belongs to.</param>
        /// <param name="sample">The <see cref="Sample"/> id to download.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceClip"/>.</returns>
        public async Task<VoiceClip> DownloadVoiceSampleAudioAsync(Voice voice, Sample sample, CancellationToken cancellationToken = default)
        {
            if (voice == null ||
                string.IsNullOrWhiteSpace(voice.Id))
            {
                throw new ArgumentNullException(nameof(voice));
            }

            if (sample == null ||
                string.IsNullOrWhiteSpace(sample.Id))
            {
                throw new ArgumentNullException(nameof(sample));
            }

            var response = await client.Client.GetAsync(GetUrl($"/{voice.Id}/samples/{sample.Id}/audio"), cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(cancellationToken).ConfigureAwait(false);
            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            return new VoiceClip(sample.Id, string.Empty, voice, memoryStream.ToArray());
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

            var response = await client.Client.DeleteAsync(GetUrl($"/{voiceId}/samples/{sampleId}"), cancellationToken);
            await response.CheckResponseAsync(cancellationToken);
            return response.IsSuccessStatusCode;
        }

        #endregion Samples
    }
}
