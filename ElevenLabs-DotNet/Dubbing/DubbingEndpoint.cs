namespace ElevenLabs.Dubbing;

using ElevenLabs.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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
    /// Initiates a dubbing operation asynchronously based on the provided <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The <see cref="DubbingRequest"/> containing dubbing configuration and files.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous dubbing operation. The task completes with the dubbing ID and expected duration 
    /// in seconds if the operation succeeds.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    public async Task<DubbingResponse> StartDubbingAsync(DubbingRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using MultipartFormDataContent content = [];

        if (!string.IsNullOrEmpty(request.Mode))
        {
            content.Add(new StringContent(request.Mode), "mode");
        }

        if (request.File.HasValue)
        {
            AppendFileToForm(content, "file", new(request.File.Value.FilePath), MediaTypeHeaderValue.Parse(request.File.Value.MediaType));
        }

        if (!string.IsNullOrEmpty(request.CsvFilePath))
        {
            AppendFileToForm(content, "csv_file", new(request.CsvFilePath), new("text/csv"));
        }

        if (!string.IsNullOrEmpty(request.ForegroundAudioFilePath))
        {
            AppendFileToForm(content, "foreground_audio_file", new(request.ForegroundAudioFilePath), new("audio/mpeg"));
        }

        if (!string.IsNullOrEmpty(request.BackgroundAudioFilePath))
        {
            AppendFileToForm(content, "background_audio_file", new(request.BackgroundAudioFilePath), new("audio/mpeg"));
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            content.Add(new StringContent(request.Name), "name");
        }

        if (!string.IsNullOrEmpty(request.SourceUrl))
        {
            content.Add(new StringContent(request.SourceUrl), "source_url");
        }

        if (!string.IsNullOrEmpty(request.SourceLanguage))
        {
            content.Add(new StringContent(request.SourceLanguage), "source_lang");
        }

        if (!string.IsNullOrEmpty(request.TargetLanguage))
        {
            content.Add(new StringContent(request.TargetLanguage), "target_lang");
        }

        if (request.NumSpeakers.HasValue)
        {
            content.Add(new StringContent(request.NumSpeakers.Value.ToString(CultureInfo.InvariantCulture)), "num_speakers");
        }

        if (request.Watermark.HasValue)
        {
            content.Add(new StringContent(request.Watermark.Value.ToString()), "watermark");
        }

        if (request.StartTime.HasValue)
        {
            content.Add(new StringContent(request.StartTime.Value.ToString(CultureInfo.InvariantCulture)), "start_time");
        }

        if (request.EndTime.HasValue)
        {
            content.Add(new StringContent(request.EndTime.Value.ToString(CultureInfo.InvariantCulture)), "end_time");
        }

        if (request.HighestResolution.HasValue)
        {
            content.Add(new StringContent(request.HighestResolution.Value.ToString()), "highest_resolution");
        }

        if (request.DubbingStudio.HasValue)
        {
            content.Add(new StringContent(request.DubbingStudio.Value.ToString()), "dubbing_studio");
        }

        using HttpResponseMessage response = await client.Client.PostAsync(GetUrl(), content, cancellationToken).ConfigureAwait(false);
        await response.CheckResponseAsync(cancellationToken).ConfigureAwait(false);

        using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<DubbingResponse>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private static void AppendFileToForm(MultipartFormDataContent content, string name, FileInfo fileInfo, MediaTypeHeaderValue mediaType)
    {
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException($"File not found: {fileInfo.FullName}");
        }

        FileStream fileStream = fileInfo.OpenRead();
        StreamContent fileContent = new(fileStream);
        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = name,
            FileName = fileInfo.Name,
        };
        fileContent.Headers.ContentType = mediaType;
        content.Add(fileContent);
    }

    /// <summary>
    /// Waits asynchronously for a dubbing operation to complete. This method polls the dubbing status at regular intervals,
    /// reporting progress updates if a progress reporter is provided.
    /// </summary>
    /// <param name="dubbingId">The ID of the dubbing project.</param>
    /// <param name="maxRetries">The maximum number of retries for checking the dubbing completion status. If not specified, a default value is used.</param>
    /// <param name="timeoutInterval">The time to wait between each status check. If not specified, a default interval is used.</param>
    /// <param name="progress">An optional <see cref="IProgress{string}"/> implementation to report progress updates, such as status messages and errors.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the waiting operation.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation. The task result is <see langword="true"/> if the dubbing completes successfully within the specified number of retries and timeout interval; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method checks the dubbing status by sending requests to the dubbing service at intervals defined by the <paramref name="timeoutInterval"/> parameter.
    /// If the dubbing status is "dubbed", the method returns <see langword="true"/>. If the dubbing fails or the specified number of <paramref name="maxRetries"/> is reached without successful completion, the method returns <see langword="false"/>.
    /// </remarks>
    public async Task<bool> WaitForDubbingCompletionAsync(string dubbingId, int? maxRetries = null, TimeSpan? timeoutInterval = null, IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        maxRetries ??= DefaultMaxRetries;
        timeoutInterval ??= DefaultTimeoutInterval;
        for (int i = 0; i < maxRetries; i++)
        {
            DubbingProjectMetadata metadata = await GetDubbingProjectMetadataAsync(dubbingId, cancellationToken).ConfigureAwait(false);
            if (metadata.Status.Equals("dubbed", StringComparison.Ordinal))
            {
                return true;
            }
            else if (metadata.Status.Equals("dubbing", StringComparison.Ordinal))
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

    private async Task<DubbingProjectMetadata> GetDubbingProjectMetadataAsync(string dubbingId, CancellationToken cancellationToken = default)
    {
        string url = $"{GetUrl()}/{dubbingId}";
        HttpResponseMessage response = await client.Client.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await response.CheckResponseAsync(cancellationToken).ConfigureAwait(false);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<DubbingProjectMetadata>(responseBody)
            ?? throw new JsonException("Could not deserialize the dubbing project metadata!");
    }

    /// <summary>
    /// Retrieves the dubbed file asynchronously as a sequence of byte arrays.
    /// </summary>
    /// <param name="dubbingId">The ID of the dubbing project.</param>
    /// <param name="languageCode">The language code of the dubbed content.</param>
    /// <param name="bufferSize">The size of the buffer used to read data from the response stream. Default is 8192 bytes.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
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
        string url = $"{GetUrl()}/{dubbingId}/audio/{languageCode}";
        using HttpResponseMessage response = await client.Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        await response.CheckResponseAsync(cancellationToken).ConfigureAwait(false);

        using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        byte[] buffer = new byte[bufferSize];
        int bytesRead;
        while ((bytesRead = await responseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            byte[] chunk = new byte[bytesRead];
            Array.Copy(buffer, chunk, bytesRead);
            yield return chunk;
        }
    }

    /// <summary>
    /// Retrieves the transcript for the dub asynchronously in the specified format (SRT or WebVTT).
    /// </summary>
    /// <param name="dubbingId">The ID of the dubbing project.</param>
    /// <param name="languageCode">The language code of the transcript.</param>
    /// <param name="formatType">Optional. The format type of the transcript file, either 'srt' or 'webvtt'.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task completes with the transcript content
    /// as a string in the specified format.
    /// </returns>
    /// <remarks>
    /// If <paramref name="formatType"/> is not specified, the method retrieves the transcript in its default format.
    /// </remarks>
    public async Task<string> GetTranscriptForDubAsync(string dubbingId, string languageCode, string formatType = null, CancellationToken cancellationToken = default)
    {
        string url = $"{GetUrl()}/{dubbingId}/transcript/{languageCode}";
        if (!string.IsNullOrEmpty(formatType))
        {
            url += $"?format_type={formatType}";
        }
        using HttpResponseMessage response = await client.Client.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await response.CheckResponseAsync(cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }
}
