// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.History
{
    /// <summary>
    /// Access to your history. Your history is a list of all your created audio including its metadata.
    /// </summary>
    public sealed class HistoryEndpoint : ElevenLabsBaseEndPoint
    {
        public HistoryEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "history";

        /// <summary>
        /// Get metadata about all your generated audio.
        /// </summary>
        /// <param name="pageSize">
        /// Optional, number of items to return. Cannot exceed 1000.<br/>
        /// Default: 100
        /// </param>
        /// <param name="startAfterId">Optional, the id of the item to start after.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="HistoryInfo{HistoryItem}"/>.</returns>
        public async Task<HistoryInfo<HistoryItem>> GetHistoryAsync(int? pageSize = null, string startAfterId = null, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, string>();

            if (pageSize.HasValue)
            {
                parameters.Add("page_size", pageSize.ToString());
            }

            if (!string.IsNullOrWhiteSpace(startAfterId))
            {
                parameters.Add("start_after_history_item_id", startAfterId);
            }

            using var response = await GetAsync(GetUrl(queryParameters: parameters), cancellationToken);
            return await response.DeserializeAsync<HistoryInfo<HistoryItem>>(EnableDebug, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets a history item by id.
        /// </summary>
        /// <param name="id"><see cref="HistoryItem.Id"/> or <see cref="VoiceClip.Id"/></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="HistoryItem"/></returns>
        public async Task<HistoryItem> GetHistoryItemAsync(string id, CancellationToken cancellationToken = default)
        {
            using var response = await GetAsync(GetUrl($"/{id}"), cancellationToken);
            return await response.DeserializeAsync<HistoryItem>(EnableDebug, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Download audio of a history item.
        /// </summary>
        /// <param name="id"><see cref="HistoryItem.Id"/> or <see cref="VoiceClip.Id"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceClip"/>.</returns>
        public async Task<VoiceClip> DownloadHistoryAudioAsync(string id, CancellationToken cancellationToken = default)
        {
            var historyItem = await GetHistoryItemAsync(id, cancellationToken).ConfigureAwait(false);
            return await DownloadHistoryAudioAsync(historyItem, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Download audio of a history item.
        /// </summary>
        /// <param name="historyItem"><see cref="HistoryItem"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceClip"/>.</returns>
        public async Task<VoiceClip> DownloadHistoryAudioAsync(HistoryItem historyItem, CancellationToken cancellationToken = default)
        {
            var voice = await client.VoicesEndpoint.GetVoiceAsync(historyItem.VoiceId, cancellationToken: cancellationToken).ConfigureAwait(false);
            using var response = await GetAsync(GetUrl($"/{historyItem.Id}/audio"), cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(EnableDebug, cancellationToken);
            var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var memoryStream = new MemoryStream();
            byte[] clipData;

            try
            {
                await responseStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                clipData = memoryStream.ToArray();
            }
            finally
            {
                await responseStream.DisposeAsync().ConfigureAwait(false);
                await memoryStream.DisposeAsync().ConfigureAwait(false);
            }

            return new VoiceClip(historyItem.Id, historyItem.Text, voice, clipData);
        }

        /// <summary>
        /// Delete a history item by its id.
        /// </summary>
        /// <param name="id"><see cref="HistoryItem.Id"/> or <see cref="VoiceClip.Id"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if history item was successfully deleted.</returns>
        public async Task<bool> DeleteHistoryItemAsync(string id, CancellationToken cancellationToken = default)
        {
            using var response = await DeleteAsync(GetUrl($"/{id}"), cancellationToken);
            await response.ReadAsStringAsync(EnableDebug, cancellationToken: cancellationToken);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Download one or more history items.<br/>
        /// If no ids are specified, then the last 100 history items are downloaded.<br/>
        /// If one history item id is provided, we will return a single audio file.<br/>
        /// If more than one history item ids are provided multiple audio files will be downloaded.
        /// </summary>
        /// <param name="historyItemIds">Optional, One or more history item ids queued for download.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of voice clips downloaded by the request.</returns>
        public async Task<IReadOnlyList<VoiceClip>> DownloadHistoryItemsAsync(List<string> historyItemIds = null, CancellationToken cancellationToken = default)
        {
            historyItemIds ??= (await GetHistoryAsync(cancellationToken: cancellationToken)).HistoryItems.Select(item => item.Id).ToList();
            var clips = new ConcurrentBag<VoiceClip>();

            async Task DownloadItem(string id)
            {
                try
                {
                    var historyItem = await GetHistoryItemAsync(id, cancellationToken).ConfigureAwait(false);
                    var clip = await DownloadHistoryAudioAsync(historyItem, cancellationToken).ConfigureAwait(false);
                    clips.Add(clip);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            await Task.WhenAll(historyItemIds.Select(DownloadItem)).ConfigureAwait(false);
            return clips.ToList();
        }
    }
}
