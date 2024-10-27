// Licensed under the MIT License. See LICENSE in the project root for license information.

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
            var defaultVoiceSettings = await ElevenLabsClient.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync("The quick brown fox jumps over the lazy dog.", voice, defaultVoiceSettings);
            Assert.NotNull(voiceClip);
            Console.WriteLine(voiceClip.Id);
        }

        [Test]
        public async Task Test_02_StreamTextToSpeech()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = (await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
            Assert.NotNull(voice);
            var defaultVoiceSettings = await ElevenLabsClient.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var partialClips = new Queue<VoiceClip>();
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync("The quick brown fox jumps over the lazy dog.", voice, defaultVoiceSettings,
            partialClipCallback: async partialClip =>
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
        public async Task Test_TurboV2_5_LanguageEnforced_TextToSpeech()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = Voices.Voice.Adam;
            Assert.NotNull(voice);
            var defaultVoiceSettings = await ElevenLabsClient.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(
                "Příliš žluťoučký kůň úpěl ďábelské ódy",
                voice, 
                defaultVoiceSettings,
                Models.Model.TurboV2_5,
                OutputFormat.MP3_44100_192,
                null,
                "cs");

            Assert.NotNull(voiceClip);
            Console.WriteLine(voiceClip.Id);
        }
    }
}