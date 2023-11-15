// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ElevenLabs.Tests
{
    internal class Test_Fixture_03_TextToSpeechEndpoint : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_TextToSpeech()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = (await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
            Assert.NotNull(voice);
            var defaultVoiceSettings = await ElevenLabsClient.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var clipPath = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync("The quick brown fox jumps over the lazy dog.", voice, defaultVoiceSettings);
            Assert.NotNull(clipPath);
            Console.WriteLine(clipPath);
        }

        [Test]
        public async Task Test_02_TextToSpeechPartial()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = (await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
            Assert.NotNull(voice);
            bool callbackCalled = false;
            var defaultVoiceSettings = await ElevenLabsClient.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var clipPath = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync("The quick brown fox jumps over the lazy dog.", voice, defaultVoiceSettings,
            
             partialClipCallback:  (partialClip) =>
            {
                callbackCalled = true;
                return null;
            });
            
            Assert.NotNull(clipPath);
            Assert.IsTrue(callbackCalled);
            Console.WriteLine(clipPath);
        }
    }
}