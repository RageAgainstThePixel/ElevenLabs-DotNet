// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.Dubbing
{
    /// <summary>
    /// Access to dubbing an audio or video file into a given language.
    /// </summary>
    public sealed class DubbingEndpoint(ElevenLabsClient client) : ElevenLabsBaseEndPoint(client)
    {
        protected override string Root => "dubbing";

        /// <summary>
        /// Dubs provided audio or video file into given language.
        /// </summary>
        /// <param name="request">The <see cref="DubbingRequest"/> containing dubbing configuration and files.</param>
        /// <param name="progress"><see cref="IProgress{DubbingProjectMetadata}"/> progress callback.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <param name="maxRetries">Optional, number of retry attempts when polling.</param>
        /// <param name="pollingInterval">Optional, <see cref="TimeSpan"/> between making requests.</param>
        /// <returns><see cref="DubbingProjectMetadata"/>.</returns>
        public async Task<DubbingProjectMetadata> DubAsync(DubbingRequest request, int? maxRetries = null, TimeSpan? pollingInterval = null, IProgress<DubbingProjectMetadata> progress = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            using var payload = new MultipartFormDataContent();

            try
            {
                if (request.Files != null)
                {
                    foreach (var (fileName, mediaType, stream) in request.Files)
                    {
                        await payload.AppendFileToFormAsync("file", stream, fileName, new(mediaType), cancellationToken);
                    }
                }

                if (!string.IsNullOrEmpty(request.ProjectName))
                {
                    payload.Add(new StringContent(request.ProjectName), "name");
                }

                if (request.SourceUrl != null)
                {
                    payload.Add(new StringContent(request.SourceUrl.ToString()), "source_url");
                }

                if (!string.IsNullOrEmpty(request.SourceLanguage))
                {
                    payload.Add(new StringContent(request.SourceLanguage), "source_lang");
                }

                if (!string.IsNullOrEmpty(request.TargetLanguage))
                {
                    payload.Add(new StringContent(request.TargetLanguage), "target_lang");
                }

                if (request.NumberOfSpeakers.HasValue)
                {
                    payload.Add(new StringContent(request.NumberOfSpeakers.Value.ToString(CultureInfo.InvariantCulture)), "num_speakers");
                }

                if (request.Watermark.HasValue)
                {
                    payload.Add(new StringContent(request.Watermark.Value.ToString()), "watermark");
                }

                if (request.StartTime.HasValue)
                {
                    payload.Add(new StringContent(request.StartTime.Value.ToString(CultureInfo.InvariantCulture)), "start_time");
                }

                if (request.EndTime.HasValue)
                {
                    payload.Add(new StringContent(request.EndTime.Value.ToString(CultureInfo.InvariantCulture)), "end_time");
                }

                if (request.HighestResolution.HasValue)
                {
                    payload.Add(new StringContent(request.HighestResolution.Value.ToString()), "highest_resolution");
                }

                if (request.DropBackgroundAudio.HasValue)
                {
                    payload.Add(new StringContent(request.DropBackgroundAudio.ToString().ToLower()), "drop_background_audio");
                }

                if (request.UseProfanityFilter.HasValue)
                {
                    payload.Add(new StringContent(request.UseProfanityFilter.ToString().ToLower()), "use_profanity_filter");
                }
            }
            finally
            {
                request.Dispose();
            }

            using var response = await client.Client.PostAsync(GetUrl(), payload, cancellationToken).ConfigureAwait(false);
            var responseBody = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var dubResponse = JsonSerializer.Deserialize<DubbingResponse>(responseBody);
            return await WaitForDubbingCompletionAsync(dubResponse, maxRetries ?? 60, pollingInterval ?? TimeSpan.FromSeconds(dubResponse.ExpectedDurationSeconds), pollingInterval == null, progress, cancellationToken);
        }

        private async Task<DubbingProjectMetadata> WaitForDubbingCompletionAsync(DubbingResponse dubbingResponse, int maxRetries, TimeSpan pollingInterval, bool adjustInterval, IProgress<DubbingProjectMetadata> progress = null, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            for (var i = 1; i < maxRetries + 1; i++)
            {
                var metadata = await GetDubbingProjectMetadataAsync(dubbingResponse, cancellationToken).ConfigureAwait(false);
                metadata.ExpectedDurationSeconds = dubbingResponse.ExpectedDurationSeconds;

                if (metadata.Status.Equals("dubbed", StringComparison.Ordinal))
                {
                    stopwatch.Stop();
                    metadata.TimeCompleted = stopwatch.Elapsed;
                    progress?.Report(metadata);
                    return metadata;
                }

                progress?.Report(metadata);

                if (metadata.Status.Equals("dubbing", StringComparison.Ordinal))
                {
                    if (adjustInterval && pollingInterval.TotalSeconds > 0.5f)
                    {
                        pollingInterval = TimeSpan.FromSeconds(dubbingResponse.ExpectedDurationSeconds / Math.Pow(2, i));
                    }

                    if (EnableDebug)
                    {
                        Console.WriteLine($"Dubbing for {dubbingResponse.DubbingId} in progress... Will check status again in {pollingInterval.TotalSeconds} seconds.");
                    }

                    await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception($"Dubbing for {dubbingResponse.DubbingId} failed: {metadata.Error}");
                }
            }

            throw new TimeoutException($"Dubbing for {dubbingResponse.DubbingId} timed out or exceeded expected duration.");
        }

        /// <summary>
        /// Returns metadata about a dubbing project, including whether it’s still in progress or not.
        /// </summary>
        /// <param name="dubbingId">Dubbing project id.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="DubbingProjectMetadata"/>.</returns>
        public async Task<DubbingProjectMetadata> GetDubbingProjectMetadataAsync(string dubbingId, CancellationToken cancellationToken = default)
        {
            using var response = await client.Client.GetAsync(GetUrl($"/{dubbingId}"), cancellationToken).ConfigureAwait(false);
            var responseBody = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<DubbingProjectMetadata>(responseBody);
        }

        /// <summary>
        /// Returns transcript for the dub in the specified format (SRT or WebVTT).
        /// </summary>
        /// <param name="dubbingId">Dubbing project id.</param>
        /// <param name="languageCode">The language code of the transcript.</param>
        /// <param name="formatType">Optional. The format type of the transcript file, either <see cref="DubbingFormat.Srt"/> or <see cref="DubbingFormat.WebVtt"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// A string containing the transcript content in the specified format.
        /// </returns>
        public async Task<string> GetTranscriptForDubAsync(string dubbingId, string languageCode, DubbingFormat formatType = DubbingFormat.Srt, CancellationToken cancellationToken = default)
        {
            var @params = new Dictionary<string, string> { { "format_type", formatType.ToString().ToLower() } };
            using var response = await client.Client.GetAsync(GetUrl($"/{dubbingId}/transcript/{languageCode}", @params), cancellationToken).ConfigureAwait(false);
            return await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns dubbed file as a streamed file.
        /// </summary>
        /// <param name="dubbingId">Dubbing project id.</param>
        /// <param name="languageCode">The language code of the dubbed content.</param>
        /// <param name="bufferSize">The size of the buffer used to read data from the response stream. Default is 8192 bytes.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// An asynchronous enumerable of byte arrays representing the dubbed file content. Each byte array
        /// contains a chunk of the dubbed file data.
        /// </returns>
        /// <remarks>
        /// This method streams the dubbed file content in chunks to optimize memory usage and improve performance.
        /// Adjust the <paramref name="bufferSize"/> parameter based on your specific requirements to achieve optimal performance.
        /// </remarks>
        public async IAsyncEnumerable<byte[]> GetDubbedFileAsync(string dubbingId, string languageCode, int bufferSize = 8192, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var response = await client.Client.GetAsync(GetUrl($"/{dubbingId}/audio/{languageCode}"), HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = await responseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                var chunk = new byte[bytesRead];
                Array.Copy(buffer, chunk, bytesRead);
                yield return chunk;
            }
        }

        /// <summary>
        /// Deletes a dubbing project.
        /// </summary>
        /// <param name="dubbingId">Dubbing project id.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public async Task DeleteDubbingProjectAsync(string dubbingId, CancellationToken cancellationToken = default)
        {
            using var response = await client.Client.DeleteAsync(GetUrl($"/{dubbingId}"), cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
        }
    }
}
