// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.SoundGeneration
{
    public sealed class SoundGenerationEndpoint : ElevenLabsBaseEndPoint
    {
        public SoundGenerationEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "sound-generation";

        /// <summary>
        /// converts text into sounds & uses the most advanced AI audio model ever.
        /// Create sound effects for your videos, voice-overs or video games.
        /// </summary>
        /// <param name="request"><see cref="SoundGenerationRequest"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="GeneratedClip"/>.</returns>
        public async Task<GeneratedClip> GenerateSoundAsync(SoundGenerationRequest request, CancellationToken cancellationToken = default)
        {
            using var payload = JsonSerializer.Serialize(request, ElevenLabsClient.JsonSerializationOptions).ToJsonStringContent();
            using var response = await client.Client.PostAsync(GetUrl(), payload, cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(EnableDebug, payload, cancellationToken).ConfigureAwait(false);
            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            return new GeneratedClip(Guid.NewGuid().ToString(), request.Text, memoryStream.ToArray());
        }

        ///// <summary>
        ///// Get metadata about all your generated sounds.
        ///// </summary>
        ///// <param name="pageSize">
        ///// Optional, number of items to return. Cannot exceed 1000.<br/>
        ///// Default: 100
        ///// </param>
        ///// <param name="startAfterId">Optional, the id of the item to start after.</param>
        ///// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        ///// <returns><see cref="HistoryInfo{T}"/>.</returns>
        //public async Task<HistoryInfo<SoundHistoryItem>> GetHistoryAsync(int? pageSize = null, string startAfterId = null, CancellationToken cancellationToken = default)
        //{
        //    var parameters = new Dictionary<string, string>();

        //    if (pageSize.HasValue)
        //    {
        //        parameters.Add("page_size", pageSize.ToString());
        //    }

        //    if (!string.IsNullOrWhiteSpace(startAfterId))
        //    {
        //        parameters.Add("start_after_history_item_id", startAfterId);
        //    }

        //    var response = await client.Client.GetAsync(GetUrl("/history", queryParameters: parameters), cancellationToken).ConfigureAwait(false);
        //    var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken: cancellationToken).ConfigureAwait(false);
        //    return JsonSerializer.Deserialize<HistoryInfo<SoundHistoryItem>>(responseAsString, ElevenLabsClient.JsonSerializationOptions);
        //}

        ///// <summary>
        ///// Get a sound by history item id.
        ///// </summary>
        ///// <param name="id"><see cref="SoundHistoryItem.Id"/> or <see cref="GeneratedClip.Id"/>.</param>
        ///// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        ///// <returns><see cref="SoundHistoryItem"/>.</returns>
        //public async Task<SoundHistoryItem> GetSoundAsync(string id, CancellationToken cancellationToken = default)
        //{
        //    var response = await client.Client.GetAsync(GetUrl($"/history/{id}"), cancellationToken).ConfigureAwait(false);
        //    var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken: cancellationToken).ConfigureAwait(false);
        //    return JsonSerializer.Deserialize<SoundHistoryItem>(responseAsString, ElevenLabsClient.JsonSerializationOptions);
        //}

        ///// <summary>
        ///// Download audio of a sound history item.
        ///// </summary>
        ///// <param name="id"><see cref="SoundHistoryItem.Id"/> or <see cref="GeneratedClip.Id"/>.</param>
        ///// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        ///// <returns><see cref="GeneratedClip"/>.</returns>
        //public async Task<GeneratedClip> DownloadSoundAudioAsync(string id, CancellationToken cancellationToken = default)
        //{
        //    var historyItem = await GetSoundAsync(id, cancellationToken).ConfigureAwait(false);
        //    return await DownloadSoundAudioAsync(historyItem, cancellationToken).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// Download audio of a sound history item.
        ///// </summary>
        ///// <param name="historyItem"><see cref="SoundHistoryItem"/>.</param>
        ///// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        ///// <returns><see cref="GeneratedClip"/>.</returns>
        //public async Task<GeneratedClip> DownloadSoundAudioAsync(SoundHistoryItem historyItem, CancellationToken cancellationToken = default)
        //{
        //    using var response = await client.Client.GetAsync(GetUrl($"/history/{historyItem.Id}/audio"), cancellationToken).ConfigureAwait(false);
        //    await response.CheckResponseAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
        //    var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        //    var memoryStream = new MemoryStream();
        //    byte[] clipData;

        //    try
        //    {
        //        await responseStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
        //        clipData = memoryStream.ToArray();
        //    }
        //    finally
        //    {
        //        await responseStream.DisposeAsync().ConfigureAwait(false);
        //        await memoryStream.DisposeAsync().ConfigureAwait(false);
        //    }

        //    return new GeneratedClip(historyItem.Id, historyItem.Text, clipData);
        //}

        ///// <summary>
        ///// Delete a history item by its id.
        ///// </summary>
        ///// <param name="id"><see cref="SoundHistoryItem.Id"/> or <see cref="GeneratedClip.Id"/>.</param>
        ///// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        ///// <returns>True, if history item was successfully deleted.</returns>
        //public async Task<bool> DeleteHistoryItemAsync(string id, CancellationToken cancellationToken = default)
        //{
        //    using var response = await client.Client.DeleteAsync(GetUrl($"/history/{id}"), cancellationToken).ConfigureAwait(false);
        //    await response.CheckResponseAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
        //    return response.IsSuccessStatusCode;
        //}

        ///// <summary>
        ///// Download one or more history items.<br/>
        ///// If no ids are specified, then the last 100 history items are downloaded.<br/>
        ///// If one history item id is provided, we will return a single audio file.<br/>
        ///// If more than one history item ids are provided multiple audio files will be downloaded.
        ///// </summary>
        ///// <param name="historyItemIds">Optional, One or more history item ids queued for download.</param>
        ///// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        ///// <returns>A list of voice clips downloaded by the request.</returns>
        //public async Task<IReadOnlyList<GeneratedClip>> DownloadHistoryItemsAsync(List<string> historyItemIds = null, CancellationToken cancellationToken = default)
        //{
        //    historyItemIds ??= (await GetHistoryAsync(cancellationToken: cancellationToken)).HistoryItems.Select(item => item.Id).ToList();
        //var clips = new ConcurrentBag<GeneratedClip>();

        //async Task DownloadItem(string id)
        //{
        //    try
        //    {
        //        var historyItem = await GetSoundAsync(id, cancellationToken).ConfigureAwait(false);
        //        var clip = await DownloadSoundAudioAsync(historyItem, cancellationToken).ConfigureAwait(false);
        //        clips.Add(clip);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //}

        //await Task.WhenAll(historyItemIds.Select(DownloadItem)).ConfigureAwait(false);
        //    return clips.ToList();
        //}
    }
}