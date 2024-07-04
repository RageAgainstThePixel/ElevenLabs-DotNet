﻿// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace ElevenLabs.Tests;

using ElevenLabs.Dubbing;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

internal class Test_Fixture_07_DubbingEndpoint : AbstractTestFixture
{
	[Test]
	public async Task Test_01_Dubbing()
	{
		Assert.NotNull(ElevenLabsClient.DubbingEndpoint);
		(string FilePath, string MediaType) audio = (Path.GetFullPath("../../../Assets/test_sample_01.ogg"), "audio/mpeg");

		FileInfo srcFile = new(audio.FilePath);

		DubbingRequest request = new()
		{
			File = audio,
			SourceLanguage = "en",
			TargetLanguage = "es",
			NumSpeakers = 1,
			Watermark = false,
		};

		string dubbingId = await ElevenLabsClient.DubbingEndpoint.DubbingAsync(request, new Progress<string>(msg => Debug.WriteLine(msg)));
		Assert.IsFalse(string.IsNullOrEmpty(dubbingId));

		FileInfo dubbedPath = new($"{srcFile.FullName}.dubbed.{request.TargetLanguage}{srcFile.Extension}");
		{
			await using FileStream fs = File.Open(dubbedPath.FullName, FileMode.Create);
			await foreach (byte[] chunk in ElevenLabsClient.DubbingEndpoint.GetDubbedFileAsync(dubbingId, request.TargetLanguage))
			{
				await fs.WriteAsync(chunk);
			}
		}
		Assert.IsTrue(dubbedPath.Exists);

		FileInfo transcriptPath = new($"{srcFile.FullName}.dubbed.{request.TargetLanguage}.srt");
		{
			string transcriptFile = await ElevenLabsClient.DubbingEndpoint.GetTranscriptForDubAsync(dubbingId, request.TargetLanguage, "srt");
			await File.WriteAllTextAsync(transcriptPath.FullName, transcriptFile);
		}
		Assert.IsTrue(transcriptPath.Exists);
	}
}
