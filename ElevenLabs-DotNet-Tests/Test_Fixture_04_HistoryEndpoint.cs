// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElevenLabs.Tests
{
    internal class Test_Fixture_04_HistoryEndpoint : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_GetHistory()
        {
            Assert.NotNull(ElevenLabsClient.HistoryEndpoint);
            var results = await ElevenLabsClient.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(results);
            Assert.IsNotEmpty(results);

            foreach (var item in results.OrderBy(item => item.Date))
            {
                Console.WriteLine($"{item.State} {item.Date} | {item.Id} | {item.Text.Length} | {item.Text}");
            }
        }

        [Test]
        public async Task Test_02_GetHistoryAudio()
        {
            Assert.NotNull(ElevenLabsClient.HistoryEndpoint);
            var historyItems = await ElevenLabsClient.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(historyItems);
            Assert.IsNotEmpty(historyItems);
            var downloadItem = historyItems.MaxBy(item => item.Date);
            Assert.NotNull(downloadItem);
            Console.WriteLine($"Downloading {downloadItem!.Id}...");
            var result = await ElevenLabsClient.HistoryEndpoint.DownloadHistoryAudioAsync(downloadItem);
            Assert.NotNull(result);
        }

        [Test]
        public async Task Test_03_DownloadAllHistoryItems()
        {
            Assert.NotNull(ElevenLabsClient.HistoryEndpoint);
            var historyItems = await ElevenLabsClient.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(historyItems);
            Assert.IsNotEmpty(historyItems);
            var singleItem = historyItems.FirstOrDefault();
            var singleItemResult = await ElevenLabsClient.HistoryEndpoint.DownloadHistoryItemsAsync(new List<string> { singleItem });
            Assert.NotNull(singleItemResult);
            Assert.IsNotEmpty(singleItemResult);
            var downloadItems = historyItems.Select(item => item.Id).ToList();
            var results = await ElevenLabsClient.HistoryEndpoint.DownloadHistoryItemsAsync(downloadItems);
            Assert.NotNull(results);
            Assert.IsNotEmpty(results);
        }

        [Test]
        public async Task Test_04_DeleteHistoryItem()
        {
            Assert.NotNull(ElevenLabsClient.HistoryEndpoint);
            var historyItems = await ElevenLabsClient.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(historyItems);
            Assert.IsNotEmpty(historyItems);
            var itemsToDelete = historyItems.Where(item => item.Text.Contains("The quick brown fox jumps over the lazy dog.")).ToList();
            Assert.NotNull(itemsToDelete);
            Assert.IsNotEmpty(itemsToDelete);

            foreach (var historyItem in itemsToDelete)
            {
                Console.WriteLine($"Deleting {historyItem.Id}...");
                var result = await ElevenLabsClient.HistoryEndpoint.DeleteHistoryItemAsync(historyItem);
                Assert.NotNull(result);
                Assert.IsTrue(result);
            }

            var updatedItems = await ElevenLabsClient.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(updatedItems);
            Assert.That(updatedItems, Has.None.EqualTo(itemsToDelete));

            foreach (var item in updatedItems.OrderBy(item => item.Date))
            {
                Console.WriteLine($"{item.State} {item.Date} | {item.Id} | {item.Text}");
            }
        }
    }
}
