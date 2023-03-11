// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs
{
    internal class AuthInfo
    {
        [JsonConstructor]
        public AuthInfo(string apiKey)
        {
            ApiKey = apiKey;
        }

        [JsonPropertyName("apiKey")]
        public string ApiKey { get; }
    }
}