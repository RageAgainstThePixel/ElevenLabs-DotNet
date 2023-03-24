// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ElevenLabs.Tests
{
    internal class Test_Fixture_000_Proxy : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_Health()
        {
            var response = await HttpClient.GetAsync("/health");
            var responseAsString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[{response.StatusCode}] {responseAsString}");
            Assert.IsTrue(HttpStatusCode.OK == response.StatusCode);
        }

        [Test]
        public async Task Test_02_Client_Authenticated()
        {
            var voices = await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync();
            Assert.IsNotNull(voices);
            Assert.IsNotEmpty(voices);

            foreach (var model in voices)
            {
                Console.WriteLine(model);
            }
        }

        [Test]
        public async Task Test_03_Client_Unauthenticated()
        {
            var webApplicationFactory = new TestProxyFactory();
            var httpClient = webApplicationFactory.CreateClient();
            var settings = new ElevenLabsClientSettings(domain: "localhost:7096");
            var auth = new ElevenLabsAuthentication("invalid-token");
            var elevenLabsClient = new ElevenLabsClient(auth, settings, httpClient);

            try
            {
                await elevenLabsClient.VoicesEndpoint.GetAllVoicesAsync();
            }
            catch (HttpRequestException httpRequestException)
            {
                // System.Net.Http.HttpRequestException : GetModelsAsync Failed! HTTP status code: Unauthorized | Response body: User is not authorized
                Assert.IsTrue(httpRequestException.StatusCode == HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}