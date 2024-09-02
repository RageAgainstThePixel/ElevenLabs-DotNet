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
        public async Task Test_01_Dubbing_File()
        {
            Assert.NotNull(ElevenLabsClient.DubbingEndpoint);
            var filePath = Path.GetFullPath("../../../Assets/test_sample_01.ogg");
            var request = new DubbingRequest(filePath, "es", "en", 1);
            var response = await ElevenLabsClient.DubbingEndpoint.StartDubbingAsync(request);
            Assert.IsFalse(string.IsNullOrEmpty(response.DubbingId));
            Assert.IsTrue(response.ExpectedDurationSeconds > 0);
            Console.WriteLine($"Expected Duration: {response.ExpectedDurationSeconds:0.00} seconds");
            Assert.IsTrue(await ElevenLabsClient.DubbingEndpoint.WaitForDubbingCompletionAsync(response.DubbingId, progress: new Progress<string>(Console.WriteLine)));

            var srcFile = new FileInfo(filePath);
            var dubbedPath = new FileInfo($"{srcFile.FullName}.dubbed.{request.TargetLanguage}{srcFile.Extension}");
            {
                await using var fs = File.Open(dubbedPath.FullName, FileMode.Create);
                await foreach (var chunk in ElevenLabsClient.DubbingEndpoint.GetDubbedFileAsync(response.DubbingId, request.TargetLanguage))
                {
                    await fs.WriteAsync(chunk);
                }
            }
            Assert.IsTrue(dubbedPath.Exists);
            Assert.IsTrue(dubbedPath.Length > 0);

            var transcriptPath = new FileInfo($"{srcFile.FullName}.dubbed.{request.TargetLanguage}.srt");
            {
                var transcriptFile = await ElevenLabsClient.DubbingEndpoint.GetTranscriptForDubAsync(response.DubbingId, request.TargetLanguage);
                await File.WriteAllTextAsync(transcriptPath.FullName, transcriptFile);
            }
            Assert.IsTrue(transcriptPath.Exists);
            Assert.IsTrue(transcriptPath.Length > 0);

            await ElevenLabsClient.DubbingEndpoint.DeleteDubbingProjectAsync(response.DubbingId);
        }

        [Test]
        public async Task Test_02_Dubbing_Url()
        {
            Assert.NotNull(ElevenLabsClient.DubbingEndpoint);

            var uri = new Uri("https://youtu.be/Zo5-rhYOlNk");
            var request = new DubbingRequest(uri, "ja", "en", 1, true);
            var response = await ElevenLabsClient.DubbingEndpoint.StartDubbingAsync(request);
            Assert.IsFalse(string.IsNullOrEmpty(response.DubbingId));
            Assert.IsTrue(response.ExpectedDurationSeconds > 0);
            Console.WriteLine($"Expected Duration: {response.ExpectedDurationSeconds:0.00} seconds");
            Assert.IsTrue(await ElevenLabsClient.DubbingEndpoint.WaitForDubbingCompletionAsync(response.DubbingId, progress: new Progress<string>(Console.WriteLine)));

            var assetsDir = Path.GetFullPath("../../../Assets");
            var dubbedPath = new FileInfo(Path.Combine(assetsDir, $"online.dubbed.{request.TargetLanguage}.mp4"));
            {
                await using var fs = File.Open(dubbedPath.FullName, FileMode.Create);
                await foreach (var chunk in ElevenLabsClient.DubbingEndpoint.GetDubbedFileAsync(response.DubbingId, request.TargetLanguage))
                {
                    await fs.WriteAsync(chunk);
                }
            }
            Assert.IsTrue(dubbedPath.Exists);
            Assert.IsTrue(dubbedPath.Length > 0);

            var transcriptPath = new FileInfo(Path.Combine(assetsDir, $"online.dubbed.{request.TargetLanguage}.srt"));
            {
                var transcriptFile = await ElevenLabsClient.DubbingEndpoint.GetTranscriptForDubAsync(response.DubbingId, request.TargetLanguage);
                await File.WriteAllTextAsync(transcriptPath.FullName, transcriptFile);
            }
            Assert.IsTrue(transcriptPath.Exists);
            Assert.IsTrue(transcriptPath.Length > 0);

            await ElevenLabsClient.DubbingEndpoint.DeleteDubbingProjectAsync(response.DubbingId);
        }
    }
}
