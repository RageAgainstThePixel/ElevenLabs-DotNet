// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;

namespace ElevenLabs.Tests
{
    internal abstract class AbstractTestFixture
    {
        protected class TestProxyFactory : WebApplicationFactory<Proxy.Program>
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseEnvironment("Development");
                base.ConfigureWebHost(builder);
            }
        }

        internal const string TestUserToken = "aAbBcCdDeE123456789";

        protected readonly HttpClient HttpClient;

        protected readonly ElevenLabsClient ElevenLabsClient;

        protected AbstractTestFixture()
        {
            var webApplicationFactory = new TestProxyFactory();
            HttpClient = webApplicationFactory.CreateClient();
            var domain = $"{HttpClient.BaseAddress?.Authority}:{HttpClient.BaseAddress?.Port}";
            var settings = new ElevenLabsClientSettings(domain: domain);
            var auth = new ElevenLabsAuthentication(TestUserToken);
            ElevenLabsClient = new ElevenLabsClient(auth, settings, HttpClient);
            ElevenLabsClient.EnableDebug = true;
        }
    }
}