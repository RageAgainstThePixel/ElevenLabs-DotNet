// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace ElevenLabs.Tests
{
    internal class TestFixture_02_Models : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_GetModels()
        {
            Assert.NotNull(ElevenLabsClient.ModelsEndpoint);
            var models = await ElevenLabsClient.ModelsEndpoint.GetModelsAsync();
            Assert.NotNull(models);
            Assert.IsNotEmpty(models);

            foreach (var model in models)
            {
                Console.WriteLine($"{model.Id} | {model.Name} | {model.Description}");

                foreach (var language in model.Languages)
                {
                    Console.WriteLine($"    {language.Id} | {language.Name}");
                }
            }
        }
    }
}