// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.TextToSpeech;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElevenLabs.Tests
{
    internal class TestFixture_04_TextToSpeechEndpoint : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_TextToSpeech()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = Voices.Voice.Adam;
            Assert.NotNull(voice);
            var request = new TextToSpeechRequest(voice, "The quick brown fox jumps over the lazy dog.");
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request);
            Assert.NotNull(voiceClip);
            Console.WriteLine(voiceClip.Id);
        }

        [Test]
        public async Task Test_02_StreamTextToSpeech()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = (await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
            Assert.NotNull(voice);
            var partialClips = new Queue<VoiceClip>();
            var request = new TextToSpeechRequest(voice, "The quick brown fox jumps over the lazy dog.", outputFormat: OutputFormat.PCM_24000);
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request, async partialClip =>
            {
                Assert.IsNotNull(partialClip);
                partialClips.Enqueue(partialClip);
                await Task.CompletedTask;
            });
            Assert.NotNull(partialClips);
            Assert.IsNotEmpty(partialClips);
            Assert.NotNull(voiceClip);
            Console.WriteLine(voiceClip.Id);
        }

        [Test]
        public async Task Test_03_TextToSpeech_Transcription()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = Voices.Voice.Adam;
            Assert.NotNull(voice);
            var request = new TextToSpeechRequest(voice, "The quick brown fox jumps over the lazy dog.", withTimestamps: true);
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request);
            Assert.NotNull(voiceClip);
            Console.WriteLine(voiceClip.Id);
            Assert.NotNull(voiceClip.TimestampedTranscriptCharacters);
            Assert.IsNotEmpty(voiceClip.TimestampedTranscriptCharacters);
            Console.WriteLine("| Character | Start Time | End Time |");
            Console.WriteLine("| --------- | ---------- | -------- |");
            foreach (var character in voiceClip.TimestampedTranscriptCharacters)
            {
                Console.WriteLine($"| {character.Character} | {character.StartTime} | {character.EndTime} |");
            }
        }

        [Test]
        public async Task Test_04_StreamTextToSpeech_Transcription()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = Voices.Voice.Adam;
            Assert.NotNull(voice);
            voice.Settings ??= await ElevenLabsClient.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var partialClips = new Queue<VoiceClip>();
            var characters = new Queue<TimestampedTranscriptCharacter>();
            Console.WriteLine("| Character | Start Time | End Time |");
            Console.WriteLine("| --------- | ---------- | -------- |");
            var request = new TextToSpeechRequest(voice, "The quick brown fox jumps over the lazy dog.", outputFormat: OutputFormat.PCM_24000, withTimestamps: true);
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request, async partialClip =>
            {
                Assert.IsNotNull(partialClip);
                partialClips.Enqueue(partialClip);
                await Task.CompletedTask;
                foreach (var character in partialClip.TimestampedTranscriptCharacters)
                {
                    characters.Enqueue(character);
                    Console.WriteLine($"| {character.Character} | {character.StartTime} | {character.EndTime} |");
                }
            });
            Assert.NotNull(partialClips);
            Assert.NotNull(partialClips);
            Assert.IsNotEmpty(partialClips);
            Assert.NotNull(voiceClip);
            Console.WriteLine(voiceClip.Id);
            Assert.AreEqual(characters.ToArray(), voiceClip.TimestampedTranscriptCharacters);
        }

        [Test]
        public async Task Test_05_01_LanguageEnforced_TextToSpeech()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = Voices.Voice.Adam;
            Assert.NotNull(voice);
            var partialClips = new Queue<VoiceClip>();
            var characters = new Queue<TimestampedTranscriptCharacter>();
            Console.WriteLine("| Character | Start Time | End Time |");
            Console.WriteLine("| --------- | ---------- | -------- |");
            var request = new TextToSpeechRequest(voice, "The quick brown fox jumps over the lazy dog.", outputFormat: OutputFormat.PCM_24000, withTimestamps: true);
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request, async partialClip =>
            {
                await Task.CompletedTask;
                partialClips.Enqueue(partialClip);
                foreach (var character in partialClip.TimestampedTranscriptCharacters)
                {
                    characters.Enqueue(character);
                    Console.WriteLine($"| {character.Character} | {character.StartTime} | {character.EndTime} |");
                }
            });
            Assert.NotNull(partialClips);
            Assert.NotNull(partialClips);
            Assert.IsNotEmpty(partialClips);
            Assert.NotNull(voiceClip);
            Console.WriteLine(voiceClip.Id);
            Assert.AreEqual(characters.ToArray(), voiceClip.TimestampedTranscriptCharacters);
        }

        [Test]
        public async Task Test_05_02_TurboV2_5_LanguageEnforced_TextToSpeech()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = Voices.Voice.Adam;
            Assert.NotNull(voice);
            var defaultVoiceSettings = await ElevenLabsClient.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var request = new TextToSpeechRequest(
                voice: voice,
                text: "Příliš žluťoučký kůň úpěl ďábelské ódy",
                voiceSettings: defaultVoiceSettings,
                model: Models.Model.TurboV2_5,
                outputFormat: OutputFormat.MP3_44100_192,
                languageCode: "cs");
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request);
            Assert.NotNull(voiceClip);
            Console.WriteLine(voiceClip.Id);
        }
    }
}