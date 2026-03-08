// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.TextToSpeech;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.Tests
{
    internal class TestFixture_09_TextToSpeechEndpoint_QueryParameters
    {
        [TestCase(true, "false")]
        [TestCase(false, "true")]
        public async Task Test_DisableRetention_MapsToEnableLogging(bool disableRetention, string expectedEnableLogging)
        {
            var requestHandler = new CaptureRequestHandler();
            using var httpClient = new HttpClient(requestHandler);
            var auth = new ElevenLabsAuthentication("test-api-key");
            var settings = new ElevenLabsClientSettings("api.elevenlabs.io");
            using var client = new ElevenLabsClient(auth, settings, httpClient);

            client.TextToSpeechEndpoint.DisableRetention = disableRetention;
            var request = new TextToSpeechRequest(
                voice: Voices.Voice.Adam,
                text: "test",
                voiceSettings: new Voices.VoiceSettings());

            _ = await client.TextToSpeechEndpoint.TextToSpeechAsync(request);

            Assert.NotNull(requestHandler.LastRequestUri);
            Assert.AreEqual(expectedEnableLogging, GetQueryParameterValue(requestHandler.LastRequestUri, "enable_logging"));
        }

        private static string GetQueryParameterValue(Uri requestUri, string key)
        {
            foreach (var parameter in requestUri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var keyValuePair = parameter.Split('=', 2);

                if (keyValuePair[0] != key)
                {
                    continue;
                }

                return keyValuePair.Length > 1
                    ? Uri.UnescapeDataString(keyValuePair[1])
                    : string.Empty;
            }

            return null;
        }

        private sealed class CaptureRequestHandler : HttpMessageHandler
        {
            internal Uri LastRequestUri { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequestUri = request.RequestUri;
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(new byte[] { 0x01 })
                };
                response.Headers.Add("history-item-id", "test-history-item-id");
                return Task.FromResult(response);
            }
        }
    }
}
