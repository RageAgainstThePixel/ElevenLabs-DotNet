// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System;
using System.Collections.Generic;
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
        private const string DubbingId = "dubbing_id";
        private const string ExpectedDurationSecs = "expected_duration_sec";

        /// <summary>
        /// Gets or sets the maximum number of retry attempts to wait for the dubbing completion status.
        /// </summary>
        public int DefaultMaxRetries { get; set; } = 30;

        /// <summary>
        /// Gets or sets the timeout interval for waiting between dubbing status checks.
        /// </summary>
        public TimeSpan DefaultTimeoutInterval { get; set; } = TimeSpan.FromSeconds(10);

        protected override string Root => "dubbing";

        /// <summary>
        /// Dubs provided audio or video file into given language.
        /// </summary>
        /// <param name="request">The <see cref="DubbingRequest"/> containing dubbing configuration and files.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns> <see cref="DubbingResponse"/>.</returns>
        public async Task<DubbingResponse> DubAsync(DubbingRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            using var payload = new MultipartFormDataContent();

            try
            {
                foreach (var (fileName, mediaType, stream) in request.Files)
                {
                    await payload.AppendFileToFormAsync("file", stream, fileName, new(mediaType), cancellationToken);
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
            }
            finally
            {
                request.Dispose();
            }

            using var response = await client.Client.PostAsync(GetUrl(), payload, cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(EnableDebug, payload, cancellationToken).ConfigureAwait(false);
            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync<DubbingResponse>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits asynchronously for a dubbing operation to complete. This method polls the dubbing status at regular intervals,
        /// reporting progress updates if a progress reporter is provided.
        /// </summary>
        /// <param name="dubbingId">The ID of the dubbing project.</param>
        /// <param name="maxRetries">The maximum number of retries for checking the dubbing completion status. If not specified, a default value is used.</param>
        /// <param name="timeoutInterval">The time to wait between each status check. If not specified, a default interval is used.</param>
        /// <param name="progress">An optional <see cref="IProgress{T}"/> implementation to report progress updates, such as status messages and errors.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the waiting operation.</param>
        /// <returns>
        /// A task that represents the asynchronous wait operation. The task result is <see langword="true"/>
        /// if the dubbing completes successfully within the specified number of retries and timeout interval; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method checks the dubbing status by sending requests to the dubbing service at intervals defined by the <paramref name="timeoutInterval"/> parameter.
        /// If the dubbing status is "dubbed", the method returns <see langword="true"/>. If the dubbing fails or the specified number of <paramref name="maxRetries"/> is reached without successful completion, the method returns <see langword="false"/>.
        /// </remarks>
        public async Task<bool> WaitForDubbingCompletionAsync(string dubbingId, int? maxRetries = null, TimeSpan? timeoutInterval = null, IProgress<string> progress = null, CancellationToken cancellationToken = default)
        {
            maxRetries ??= DefaultMaxRetries;
            timeoutInterval ??= DefaultTimeoutInterval;

            for (var i = 0; i < maxRetries; i++)
            {
                var metadata = await GetDubbingProjectMetadataAsync(dubbingId, cancellationToken).ConfigureAwait(false);

                if (metadata.Status.Equals("dubbed", StringComparison.Ordinal)) { return true; }

                if (metadata.Status.Equals("dubbing", StringComparison.Ordinal))
                {
                    progress?.Report($"Dubbing for {dubbingId} in progress... Will check status again in {timeoutInterval.Value.TotalSeconds} seconds.");
                    await Task.Delay(timeoutInterval.Value, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    progress?.Report($"Dubbing for {dubbingId} failed: {metadata.Error}");
                    return false;
                }
            }

            progress?.Report($"Dubbing for {dubbingId} timed out or exceeded expected duration.");
            return false;
        }

        /// <summary>
        /// Returns metadata about a dubbing project, including whether it’s still in progress or not.
        /// </summary>
        /// <param name="dubbingId"></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="DubbingProjectMetadata"/>.</returns>
        public async Task<DubbingProjectMetadata> GetDubbingProjectMetadataAsync(string dubbingId, CancellationToken cancellationToken = default)
        {
            var response = await client.Client.GetAsync(GetUrl($"/{dubbingId}"), cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<DubbingProjectMetadata>(responseBody);
        }

        /// <summary>
        /// Returns transcript for the dub in the specified format (SRT or WebVTT).
        /// </summary>
        /// <param name="dubbingId">The ID of the dubbing project.</param>
        /// <param name="languageCode">The language code of the transcript.</param>
        /// <param name="formatType">Optional. The format type of the transcript file, either 'srt' or 'webvtt'.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task completes with the transcript content
        /// as a string in the specified format.
        /// </returns>
        /// <remarks>
        /// If <paramref name="formatType"/> is not specified, the method retrieves the transcript in its default format.
        /// </remarks>
        public async Task<string> GetTranscriptForDubAsync(string dubbingId, string languageCode, DubbingFormat formatType = DubbingFormat.Srt, CancellationToken cancellationToken = default)
        {
            var @params = new Dictionary<string, string> { { "format_type", formatType.ToString().ToLower() } };
            using var response = await client.Client.GetAsync(GetUrl($"/{dubbingId}/transcript/{languageCode}", @params), cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns dubbed file as a streamed file.
        /// </summary>
        /// <param name="dubbingId">The ID of the dubbing project.</param>
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
        /// <param name="dubbingId">The ID of the dubbing project.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public async Task DeleteDubbingProjectAsync(string dubbingId, CancellationToken cancellationToken = default)
        {
            using var response = await client.Client.DeleteAsync(GetUrl($"/{dubbingId}"), cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
        }
    }
}
