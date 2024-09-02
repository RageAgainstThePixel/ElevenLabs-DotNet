// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.SoundGeneration;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ElevenLabs.Tests
{
    internal class TestFixture_06_SoundGeneration : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_GenerateSound()
        {
            Assert.NotNull(ElevenLabsClient.SoundGenerationEndpoint);
            var request = new SoundGenerationRequest("Star Wars Light Saber parry");
            var clip = await ElevenLabsClient.SoundGenerationEndpoint.GenerateSoundAsync(request);
            Assert.NotNull(clip);
            Assert.IsFalse(clip.ClipData.IsEmpty);
            Assert.IsFalse(string.IsNullOrWhiteSpace(clip.Text));
        }

        //[Test]
        //public async Task Test_02_01_GetSoundGenerationHistory()
        //{
        //    Assert.NotNull(ElevenLabsClient.SoundGenerationEndpoint);
        //    var historyInfo = await ElevenLabsClient.SoundGenerationEndpoint.GetHistoryAsync(pageSize: 20);
        //    Assert.NotNull(historyInfo);
        //    Assert.IsNotEmpty(historyInfo.HistoryItems);

        //    foreach (var item in historyInfo.HistoryItems)
        //    {
        //        Console.WriteLine($"{item.Id} | {item.Text} | {item.CreatedAt}");
        //    }
        //}

        //[Test]
        //public async Task Test_02_02_GetHistoryAudio()
        //{
        //    Assert.NotNull(ElevenLabsClient.SoundGenerationEndpoint);
        //    var historyInfo = await ElevenLabsClient.SoundGenerationEndpoint.GetHistoryAsync(pageSize: 20);
        //    Assert.NotNull(historyInfo);
        //    Assert.IsNotEmpty(historyInfo.HistoryItems);
        //    var downloadItem = historyInfo.HistoryItems.OrderByDescending(item => item.CreatedAt).FirstOrDefault();
        //    Assert.NotNull(downloadItem);
        //    Console.WriteLine($"Downloading {downloadItem!.Id}...");
        //    var soundClip = await ElevenLabsClient.SoundGenerationEndpoint.DownloadSoundAudioAsync(downloadItem);
        //    Assert.NotNull(soundClip);
        //    Assert.IsFalse(soundClip.ClipData.IsEmpty);
        //}

        //[Test]
        //public async Task Test_02_03_DownloadAllHistoryItems()
        //{
        //    Assert.NotNull(ElevenLabsClient.SoundGenerationEndpoint);
        //    var historyInfo = await ElevenLabsClient.SoundGenerationEndpoint.GetHistoryAsync(pageSize: 20);
        //    Assert.NotNull(historyInfo);
        //    Assert.IsNotEmpty(historyInfo.HistoryItems);
        //    var singleItem = historyInfo.HistoryItems.FirstOrDefault();
        //    var singleItemResult = await ElevenLabsClient.SoundGenerationEndpoint.DownloadHistoryItemsAsync(new List<string> { singleItem });
        //    Assert.NotNull(singleItemResult);
        //    Assert.IsNotEmpty(singleItemResult);
        //    var downloadItems = historyInfo.HistoryItems.Select(item => item.Id).ToList();
        //    var soundClips = await ElevenLabsClient.SoundGenerationEndpoint.DownloadHistoryItemsAsync(downloadItems);
        //    Assert.NotNull(soundClips);
        //    Assert.IsNotEmpty(soundClips);
        //}

        //[Test]
        //public async Task Test_02_04_DeleteHistoryItem()
        //{
        //    Assert.NotNull(ElevenLabsClient.SoundGenerationEndpoint);
        //    var historyInfo = await ElevenLabsClient.SoundGenerationEndpoint.GetHistoryAsync(pageSize: 20);
        //    Assert.NotNull(historyInfo);
        //    Assert.IsNotEmpty(historyInfo.HistoryItems);
        //    var itemsToDelete = historyInfo.HistoryItems.Where(item => item.Text.Contains("Star Wars Light Saber parry")).ToList();
        //    Assert.NotNull(itemsToDelete);
        //    Assert.IsNotEmpty(itemsToDelete);

        //    foreach (var historyItem in itemsToDelete)
        //    {
        //        Console.WriteLine($"Deleting {historyItem.Id}...");
        //        var result = await ElevenLabsClient.SoundGenerationEndpoint.DeleteHistoryItemAsync(historyItem.Id);
        //        Assert.IsTrue(result);
        //    }

        //    var updatedHistoryInfo = await ElevenLabsClient.SoundGenerationEndpoint.GetHistoryAsync();
        //    Assert.NotNull(updatedHistoryInfo);
        //    Assert.That(updatedHistoryInfo.HistoryItems, Has.None.EqualTo(itemsToDelete));

        //    foreach (var item in updatedHistoryInfo.HistoryItems.OrderBy(item => item.CreatedAt))
        //    {
        //        Console.WriteLine($"[{item.Id}] {item.CreatedAt} | {item.Text}");
        //    }
        //}
    }
}