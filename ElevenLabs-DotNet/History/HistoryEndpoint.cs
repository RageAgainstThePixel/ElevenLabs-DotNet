// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.History
{
    /// <summary>
    /// Access to your history. Your history is a list of all your created audio including its metadata.
    /// </summary>
    public sealed class HistoryEndpoint : BaseEndPoint
    {
        private class HistoryInfo
        {
            [JsonInclude]
            [JsonPropertyName("history")]
            public IReadOnlyList<HistoryItem> History { get; private set; }
        }

        public HistoryEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string GetEndpoint()
            => $"{Api.BaseUrl}history";

        /// <summary>
        /// Get metadata about all your generated audio.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of history items containing metadata about generated audio.</returns>
        public async Task<IReadOnlyList<HistoryItem>> GetHistoryAsync(CancellationToken cancellationToken = default)
        {
            var result = await Api.Client.GetAsync($"{GetEndpoint()}", cancellationToken);
            var resultAsString = await result.ReadAsStringAsync();
            return JsonSerializer.Deserialize<HistoryInfo>(resultAsString, Api.JsonSerializationOptions)?.History;
        }

        /// <summary>
        /// Get audio of a history item.
        /// </summary>
        /// <param name="historyItem"><see cref="HistoryItem.Id"/></param>
        /// <param name="saveDirectory">Optional, save directory for the downloaded audio file.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>The path to the downloaded audio file..</returns>
        public async Task<string> GetHistoryAudioAsync(HistoryItem historyItem, string saveDirectory = null, CancellationToken cancellationToken = default)
        {
            var rootDirectory = (saveDirectory ?? Directory.GetCurrentDirectory()).CreateNewDirectory(nameof(ElevenLabs));
            var downloadDirectory = rootDirectory.CreateNewDirectory(nameof(History));
            var voiceDirectory = downloadDirectory.CreateNewDirectory(historyItem.VoiceName);
            var filePath = Path.Combine(voiceDirectory, $"{historyItem.Id}.mp3");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var response = await Api.Client.GetAsync($"{GetEndpoint()}/{historyItem.Id}/audio", cancellationToken);
            await response.CheckResponseAsync(cancellationToken);

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
        /// Delete a history item by its id.
        /// </summary>
        /// <param name="historyId"><see cref="HistoryItem.Id"/></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if history item was successfully deleted.</returns>
        public async Task<bool> DeleteHistoryItemAsync(string historyId, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.DeleteAsync($"{GetEndpoint()}/{historyId}", cancellationToken);
            await response.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Download one or more history items.<br/>
        /// If no ids are specified, then all history items are downloaded.<br/>
        /// If one history item id is provided, we will return a single audio file.<br/>
        /// If more than one history item ids are provided multiple audio files will be downloaded.
        /// </summary>
        /// <param name="historyItemIds">One or more history item ids queued for download.</param>
        /// <param name="saveDirectory">Optional, The directory path to save the history in.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A path to the downloaded zip file, or audio file.</returns>
        public async Task<IReadOnlyList<string>> DownloadHistoryItemsAsync(List<string> historyItemIds = null, string saveDirectory = null, CancellationToken cancellationToken = default)
        {
            historyItemIds ??= (await GetHistoryAsync(cancellationToken)).Select(item => item.Id).ToList();

            var audioClips = new List<string>();

            if (historyItemIds.Count == 1)
            {
                var history = await GetHistoryAsync(cancellationToken);
                var historyItem = history.FirstOrDefault(item => item.Id == historyItemIds.FirstOrDefault());
                audioClips.Add(await GetHistoryAudioAsync(historyItem, saveDirectory, cancellationToken));
            }
            else
            {
                var jsonContent = $"{{\"history_item_ids\":[\"{string.Join("\",\"", historyItemIds)}\"]}}".ToJsonStringContent();
                var response = await Api.Client.PostAsync($"{GetEndpoint()}/download", jsonContent, cancellationToken);
                await response.CheckResponseAsync(cancellationToken);
                var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

                var rootDirectory = (saveDirectory ?? Directory.GetCurrentDirectory()).CreateNewDirectory(nameof(ElevenLabs));
                var downloadDirectory = rootDirectory.CreateNewDirectory(nameof(History));
                var zipFilePath = $"{downloadDirectory}/history.zip";

                try
                {
                    if (File.Exists(zipFilePath))
                    {
                        File.Delete(zipFilePath);
                    }

                    var fileStream = new FileStream(zipFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);

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

                try
                {
                    ZipFile.ExtractToDirectory(zipFilePath, downloadDirectory, true);
                    audioClips.AddRange(Directory.GetFiles(downloadDirectory, "*.mp3", SearchOption.AllDirectories));
                }
                finally
                {
                    File.Delete(zipFilePath);
                }
            }

            return audioClips.ToList();
        }
    }
}
