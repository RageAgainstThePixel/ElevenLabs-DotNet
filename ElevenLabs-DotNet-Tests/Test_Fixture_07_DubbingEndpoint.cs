// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace ElevenLabs.Tests;

using ElevenLabs.Dubbing;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

internal class Test_Fixture_07_DubbingEndpoint : AbstractTestFixture
{
    [Test]
    public async Task Test_01_Dubbing_File()
    {
        Assert.NotNull(ElevenLabsClient.DubbingEndpoint);

        (string FilePath, string MediaType) audio = (Path.GetFullPath("../../../Assets/test_sample_01.ogg"), "audio/mpeg");
        DubbingRequest request = new()
        {
            File = audio,
            SourceLanguage = "en",
            TargetLanguage = "es",
            NumSpeakers = 1,
            Watermark = false,
        };

        (string dubbingId, float expectedDurationSecs) = await ElevenLabsClient.DubbingEndpoint.StartDubbingAsync(request);
        Assert.IsFalse(string.IsNullOrEmpty(dubbingId));
        Assert.IsTrue(expectedDurationSecs > 0);
        Console.WriteLine($"Expected Duration: {expectedDurationSecs:0.00} seconds");

        Assert.IsTrue(await ElevenLabsClient.DubbingEndpoint.WaitForDubbingCompletionAsync(dubbingId, progress: new Progress<string>(msg => Console.WriteLine(msg))));

        FileInfo srcFile = new(audio.FilePath);
        FileInfo dubbedPath = new($"{srcFile.FullName}.dubbed.{request.TargetLanguage}{srcFile.Extension}");
        {
            await using FileStream fs = File.Open(dubbedPath.FullName, FileMode.Create);
            await foreach (byte[] chunk in ElevenLabsClient.DubbingEndpoint.GetDubbedFileAsync(dubbingId, request.TargetLanguage))
            {
                await fs.WriteAsync(chunk);
            }
        }
        Assert.IsTrue(dubbedPath.Exists);
        Assert.IsTrue(dubbedPath.Length > 0);

        FileInfo transcriptPath = new($"{srcFile.FullName}.dubbed.{request.TargetLanguage}.srt");
        {
            string transcriptFile = await ElevenLabsClient.DubbingEndpoint.GetTranscriptForDubAsync(dubbingId, request.TargetLanguage, "srt");
            await File.WriteAllTextAsync(transcriptPath.FullName, transcriptFile);
        }
        Assert.IsTrue(transcriptPath.Exists);
        Assert.IsTrue(transcriptPath.Length > 0);
    }

    [Test]
    public async Task Test_02_Dubbing_Url()
    {
        Assert.NotNull(ElevenLabsClient.DubbingEndpoint);

        Uri uri = new("https://youtu.be/Zo5-rhYOlNk");
        DubbingRequest request = new()
        {
            SourceUrl = uri.AbsoluteUri,
            SourceLanguage = "en",
            TargetLanguage = "ja",
            NumSpeakers = 1,
            Watermark = true,
        };

        (string dubbingId, float expectedDurationSecs) = await ElevenLabsClient.DubbingEndpoint.StartDubbingAsync(request);
        Assert.IsFalse(string.IsNullOrEmpty(dubbingId));
        Assert.IsTrue(expectedDurationSecs > 0);
        Console.WriteLine($"Expected Duration: {expectedDurationSecs:0.00} seconds");

        Assert.IsTrue(await ElevenLabsClient.DubbingEndpoint.WaitForDubbingCompletionAsync(dubbingId, progress: new Progress<string>(msg => Console.WriteLine(msg))));

        string assetsDir = Path.GetFullPath("../../../Assets");
        FileInfo dubbedPath = new(Path.Combine(assetsDir, $"online.dubbed.{request.TargetLanguage}.mp4"));
        {
            await using FileStream fs = File.Open(dubbedPath.FullName, FileMode.Create);
            await foreach (byte[] chunk in ElevenLabsClient.DubbingEndpoint.GetDubbedFileAsync(dubbingId, request.TargetLanguage))
            {
                await fs.WriteAsync(chunk);
            }
        }
        Assert.IsTrue(dubbedPath.Exists);
        Assert.IsTrue(dubbedPath.Length > 0);

        FileInfo transcriptPath = new(Path.Combine(assetsDir, $"online.dubbed.{request.TargetLanguage}.srt"));
        {
            string transcriptFile = await ElevenLabsClient.DubbingEndpoint.GetTranscriptForDubAsync(dubbingId, request.TargetLanguage, "srt");
            await File.WriteAllTextAsync(transcriptPath.FullName, transcriptFile);
        }
        Assert.IsTrue(transcriptPath.Exists);
        Assert.IsTrue(transcriptPath.Length > 0);
    }
}
