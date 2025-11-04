// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Dubbing;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ElevenLabs.Tests
{
    internal class TestFixture_08_DubbingEndpoint : AbstractTestFixture
    {
        [Test]
        [Timeout(90000)]
        public async Task Test_01_Dubbing_File()
        {
            Assert.NotNull(ElevenLabsClient.DubbingEndpoint);
            var filePath = Path.GetFullPath("../../../Assets/test_sample_01.ogg");
            var request = new DubbingRequest(filePath, "es", "en", 1);
            var metadata = await ElevenLabsClient.DubbingEndpoint.DubAsync(request, progress: new Progress<DubbingProjectMetadata>(metadata =>
            {
                switch (metadata.Status)
                {
                    case "dubbing":
                        Console.WriteLine($"Dubbing for {metadata.DubbingId} in progress... Expected Duration: {metadata.ExpectedDurationSeconds:0.00} seconds");
                        break;
                    case "dubbed":
                        Console.WriteLine($"Dubbing for {metadata.DubbingId} complete in {metadata.TimeCompleted.TotalSeconds:0.00} seconds!");
                        break;
                    default:
                        Console.WriteLine($"Status: {metadata.Status}");
                        break;
                }
            }));
            Assert.IsFalse(string.IsNullOrEmpty(metadata.DubbingId));
            Assert.IsTrue(metadata.ExpectedDurationSeconds > 0);

            var srcFile = new FileInfo(filePath);
            var dubbedPath = new FileInfo($"{srcFile.FullName}.dubbed.{request.TargetLanguage}{srcFile.Extension}");
            {
                await using var fs = File.Open(dubbedPath.FullName, FileMode.Create);
                await foreach (var chunk in ElevenLabsClient.DubbingEndpoint.GetDubbedFileAsync(metadata.DubbingId, request.TargetLanguage))
                {
                    await fs.WriteAsync(chunk);
                }
            }
            Assert.IsTrue(dubbedPath.Exists);
            Assert.IsTrue(dubbedPath.Length > 0);

            var transcriptPath = new FileInfo($"{srcFile.FullName}.dubbed.{request.TargetLanguage}.srt");
            {
                var transcriptFile = await ElevenLabsClient.DubbingEndpoint.GetTranscriptForDubAsync(metadata.DubbingId, request.TargetLanguage);
                await File.WriteAllTextAsync(transcriptPath.FullName, transcriptFile);
            }
            Assert.IsTrue(transcriptPath.Exists);
            Assert.IsTrue(transcriptPath.Length > 0);

            await ElevenLabsClient.DubbingEndpoint.DeleteDubbingProjectAsync(metadata.DubbingId);
        }

        [Test]
        [Timeout(90000)]
        public async Task Test_02_Dubbing_Url()
        {
            Assert.NotNull(ElevenLabsClient.DubbingEndpoint);

            var request = new DubbingRequest(new Uri("https://youtu.be/Zo5-rhYOlNk"), "ja", "en", 1, watermark: true, dropBackgroundAudio: true);
            var metadata = await ElevenLabsClient.DubbingEndpoint.DubAsync(request, progress: new Progress<DubbingProjectMetadata>(metadata =>
            {
                switch (metadata.Status)
                {
                    case "dubbing":
                        Console.WriteLine($"Dubbing for {metadata.DubbingId} in progress... Expected Duration: {metadata.ExpectedDurationSeconds:0.00} seconds");
                        break;
                    case "dubbed":
                        Console.WriteLine($"Dubbing for {metadata.DubbingId} complete in {metadata.TimeCompleted.TotalSeconds:0.00} seconds!");
                        break;
                    default:
                        Console.WriteLine($"Status: {metadata.Status}");
                        break;
                }
            }));
            Assert.IsFalse(string.IsNullOrEmpty(metadata.DubbingId));
            Assert.IsTrue(metadata.ExpectedDurationSeconds > 0);

            var assetsDir = Path.GetFullPath("../../../Assets");
            var dubbedPath = new FileInfo(Path.Combine(assetsDir, $"online.dubbed.{request.TargetLanguage}.mp4"));
            {
                await using var fs = File.Open(dubbedPath.FullName, FileMode.Create);
                await foreach (var chunk in ElevenLabsClient.DubbingEndpoint.GetDubbedFileAsync(metadata.DubbingId, request.TargetLanguage))
                {
                    await fs.WriteAsync(chunk);
                }
            }
            Assert.IsTrue(dubbedPath.Exists);
            Assert.IsTrue(dubbedPath.Length > 0);

            var transcriptPath = new FileInfo(Path.Combine(assetsDir, $"online.dubbed.{request.TargetLanguage}.srt"));
            {
                var transcriptFile = await ElevenLabsClient.DubbingEndpoint.GetTranscriptForDubAsync(metadata.DubbingId, request.TargetLanguage);
                await File.WriteAllTextAsync(transcriptPath.FullName, transcriptFile);
            }
            Assert.IsTrue(transcriptPath.Exists);
            Assert.IsTrue(transcriptPath.Length > 0);

            await ElevenLabsClient.DubbingEndpoint.DeleteDubbingProjectAsync(metadata.DubbingId);
        }
    }
}
