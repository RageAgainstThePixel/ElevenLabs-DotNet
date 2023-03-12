// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.VoiceGeneration;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ElevenLabs.Voice.Tests
{
    internal class Test_Fixture_05_VoiceGeneration
    {
        [Test]
        public async Task Test_01_GetVoiceGenerationOptions()
        {
            var api = new ElevenLabsClient(ElevenLabsAuthentication.LoadFromEnv());
            Assert.NotNull(api.VoiceGenerationEndpoint);
            var options = await api.VoiceGenerationEndpoint.GetVoiceGenerationOptionsAsync();
            Assert.NotNull(options);
            Console.WriteLine(JsonSerializer.Serialize(options));
        }

        [Test]
        public async Task Test_02_GenerateVoice()
        {
            var api = new ElevenLabsClient(ElevenLabsAuthentication.LoadFromEnv());
            Assert.NotNull(api.VoiceGenerationEndpoint);
            var options = await api.VoiceGenerationEndpoint.GetVoiceGenerationOptionsAsync();
            var generateRequest = new GeneratedVoiceRequest("First we thought the PC was a calculator. Then we found out how to turn numbers into letters and we thought it was a typewriter.", options.Genders.FirstOrDefault(), options.Accents.FirstOrDefault(), options.Ages.FirstOrDefault());
            var (generatedVoiceId, audioFilePath) = await api.VoiceGenerationEndpoint.GenerateVoiceAsync(generateRequest);
            Console.WriteLine(generatedVoiceId);
            Console.WriteLine(audioFilePath);
            var createVoiceRequest = new CreateVoiceRequest("Test Voice Lab Create Voice", generatedVoiceId);
            File.Delete(audioFilePath);
            Assert.NotNull(createVoiceRequest);
            var result = await api.VoiceGenerationEndpoint.CreateVoiceAsync(createVoiceRequest);
            Assert.NotNull(result);
            Console.WriteLine(result.Id);
            var deleteResult = await api.VoicesEndpoint.DeleteVoiceAsync(result.Id);
            Assert.NotNull(deleteResult);
            Assert.IsTrue(deleteResult);
        }
    }
}