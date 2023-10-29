﻿// Licensed under the MIT License. See LICENSE in the project root for license information.

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
            Assert.IsNotEmpty(results.HistoryItems);

            foreach (var item in results.HistoryItems.OrderBy(item => item.Date))
            {
                Console.WriteLine($"{item.State} {item.Date} | {item.Id} | {item.Text.Length} | {item.Text}");
            }
        }

        [Test]
        public async Task Test_02_GetHistoryAudio()
        {
            Assert.NotNull(ElevenLabsClient.HistoryEndpoint);
            var historyInfo = await ElevenLabsClient.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(historyInfo);
            Assert.IsNotEmpty(historyInfo.HistoryItems);
            var downloadItem = historyInfo.HistoryItems.MaxBy(item => item.Date);
            Assert.NotNull(downloadItem);
            Console.WriteLine($"Downloading {downloadItem!.Id}...");
            var voiceClip = await ElevenLabsClient.HistoryEndpoint.DownloadHistoryAudioAsync(downloadItem);
            Assert.NotNull(voiceClip);
        }

        [Test]
        public async Task Test_03_DownloadAllHistoryItems()
        {
            Assert.NotNull(ElevenLabsClient.HistoryEndpoint);
            var historyInfo = await ElevenLabsClient.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(historyInfo);
            Assert.IsNotEmpty(historyInfo.HistoryItems);
            var singleItem = historyInfo.HistoryItems.FirstOrDefault();
            var singleItemResult = await ElevenLabsClient.HistoryEndpoint.DownloadHistoryItemsAsync(new List<string> { singleItem });
            Assert.NotNull(singleItemResult);
            Assert.IsNotEmpty(singleItemResult);
            var downloadItems = historyInfo.HistoryItems.Select(item => item.Id).ToList();
            var voiceClips = await ElevenLabsClient.HistoryEndpoint.DownloadHistoryItemsAsync(downloadItems);
            Assert.NotNull(voiceClips);
            Assert.IsNotEmpty(voiceClips);
        }

        [Test]
        public async Task Test_04_DeleteHistoryItem()
        {
            Assert.NotNull(ElevenLabsClient.HistoryEndpoint);
            var historyInfo = await ElevenLabsClient.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(historyInfo);
            Assert.IsNotEmpty(historyInfo.HistoryItems);
            var itemsToDelete = historyInfo.HistoryItems.Where(item => item.Text.Contains("The quick brown fox jumps over the lazy dog.")).ToList();
            Assert.NotNull(itemsToDelete);
            Assert.IsNotEmpty(itemsToDelete);

            foreach (var historyItem in itemsToDelete)
            {
                Console.WriteLine($"Deleting {historyItem.Id}...");
                var result = await ElevenLabsClient.HistoryEndpoint.DeleteHistoryItemAsync(historyItem);
                Assert.NotNull(result);
                Assert.IsTrue(result);
            }

            var updatedHistoryInfo = await ElevenLabsClient.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(updatedHistoryInfo);
            Assert.That(updatedHistoryInfo.HistoryItems, Has.None.EqualTo(itemsToDelete));

            foreach (var item in updatedHistoryInfo.HistoryItems.OrderBy(item => item.Date))
            {
                Console.WriteLine($"{item.State} {item.Date} | {item.Id} | {item.Text}");
            }
        }
    }
}
