// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        public async Task<IReadOnlyList<Voice>> GetAllVoicesAsync(CancellationToken cancellationToken = default)
            => await GetAllVoicesAsync(true, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Gets a list of all available voices for a user.
        /// </summary>
        /// <param name="downloadSettings">Whether to download all settings for the voices.</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="IReadOnlyList{T}"/> of <see cref="Voice"/>s.</returns>
        public async Task<IReadOnlyList<Voice>> GetAllVoicesAsync(bool downloadSettings, CancellationToken cancellationToken = default)
        {
            using var response = await client.Client.GetAsync(GetUrl(), cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var voices = JsonSerializer.Deserialize<VoiceList>(responseAsString, ElevenLabsClient.JsonSerializationOptions).Voices;

            if (downloadSettings)
            {
                var voiceSettingsTasks = new List<Task>();

                foreach (var voice in voices)
                {
                    voiceSettingsTasks.Add(LocalGetVoiceSettingsAsync());

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
            using var response = await client.Client.GetAsync(GetUrl("/settings/default"), cancellationToken).ConfigureAwait(false);
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

            using var response = await client.Client.GetAsync(GetUrl($"/{voiceId}/settings"), cancellationToken).ConfigureAwait(false);
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

            using var response = await client.Client.GetAsync(GetUrl($"/{voiceId}?with_settings={withSettings.ToString().ToLower()}"), cancellationToken).ConfigureAwait(false);
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

            using var payload = JsonSerializer.Serialize(voiceSettings).ToJsonStringContent();
            using var response = await client.Client.PostAsync(GetUrl($"/{voiceId}/settings/edit"), payload, cancellationToken).ConfigureAwait(false);
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
        [Obsolete("Use new overload with VoiceRequest.")]
        public async Task<Voice> AddVoiceAsync(string name, IEnumerable<string> samplePaths, IReadOnlyDictionary<string, string> labels = null, CancellationToken cancellationToken = default)
            => await AddVoiceAsync(new VoiceRequest(name, samplePaths, labels), cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Add a new voice to your collection of voices in VoiceLab from a stream
        /// </summary>
        /// <param name="name">Name of the voice you want to add.</param>
        /// <param name="samples">Collection of samples as an array of bytes to be used for the new voice</param>
        /// <param name="labels">Optional, labels for the new voice.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        [Obsolete("Use new overload with VoiceRequest.")]
        public async Task<Voice> AddVoiceAsync(string name, IEnumerable<byte[]> samples, IReadOnlyDictionary<string, string> labels = null, CancellationToken cancellationToken = default)
            => await AddVoiceAsync(new VoiceRequest(name, samples, labels), cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Add a new voice to your collection of voices in VoiceLab from a stream
        /// </summary>
        /// <param name="name">Name of the voice you want to add.</param>
        /// <param name="sampleStreams">Collection of samples as a stream to be used for the new voice</param>
        /// <param name="labels">Optional, labels for the new voice.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        [Obsolete("Use new overload with VoiceRequest.")]
        public async Task<Voice> AddVoiceAsync(string name, IEnumerable<Stream> sampleStreams, IReadOnlyDictionary<string, string> labels = null, CancellationToken cancellationToken = default)
            => await AddVoiceAsync(new VoiceRequest(name, sampleStreams, labels), cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Add a new voice to your collection of voices in VoiceLab.
        /// </summary>
        /// <param name="request"><see cref="VoiceRequest"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Voice"/>.</returns>
        public async Task<Voice> AddVoiceAsync(VoiceRequest request, CancellationToken cancellationToken = default)
        {
            using var payload = new MultipartFormDataContent();

            try
            {
                payload.Add(new StringContent(request.Name), "name");

                foreach (var (fileName, sample) in request.Samples)
                {
                    using var audioData = new MemoryStream();
                    await sample.CopyToAsync(audioData, cancellationToken).ConfigureAwait(false);
                    var content = new ByteArrayContent(audioData.ToArray());
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "files",
                        FileName = fileName
                    };
                    payload.Add(content, "files", fileName);
                }

                if (request.Labels != null)
                {
                    payload.Add(new StringContent(JsonSerializer.Serialize(request.Labels)), "labels");
                }
            }
            finally
            {
                request.Dispose();
            }

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, GetUrl("/add"));
            httpRequest.Content = payload;
            httpRequest.Version = HttpVersion.Version10;
            httpRequest.Headers.ExpectContinue = true;
            httpRequest.Headers.ConnectionClose = false;
            using var response = await client.Client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, payload, cancellationToken).ConfigureAwait(false);
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
            using var payload = new MultipartFormDataContent();

            ArgumentNullException.ThrowIfNull(voice, nameof(voice));
            ArgumentNullException.ThrowIfNull(samplePaths, nameof(samplePaths));

            payload.Add(new StringContent(voice.Name), "name");

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
                        var content = new ByteArrayContent(fileBytes);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = "files",
                            FileName = Path.GetFileName(sample)
                        };
                        payload.Add(content, "files", Path.GetFileName(sample));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            if (labels != null)
            {
                payload.Add(new StringContent(JsonSerializer.Serialize(labels)), "labels");
            }

            using var response = await client.Client.PostAsync(GetUrl($"/{voice.Id}/edit"), payload, cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(EnableDebug, payload, cancellationToken).ConfigureAwait(false);
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

            using var response = await client.Client.DeleteAsync(GetUrl($"/{voiceId}"), cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
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

            using var response = await client.Client.GetAsync(GetUrl($"/{voice.Id}/samples/{sample.Id}/audio"), cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
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

            using var response = await client.Client.DeleteAsync(GetUrl($"/{voiceId}/samples/{sampleId}"), cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        #endregion Samples
    }
}
