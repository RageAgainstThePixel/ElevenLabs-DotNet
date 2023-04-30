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
            var clipPath = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync("The quick brown fox jumps over the lazy dog.", voice, defaultVoiceSettings, deleteCachedFile: true);
            Assert.NotNull(clipPath);
            Console.WriteLine(clipPath);
        }
    }
}