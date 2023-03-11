// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.VoiceGeneration
{
    public sealed class GeneratedVoiceRequest
    {
        [JsonInclude]
        [JsonPropertyName("text")]
        public string Text { get; private set; }

        [JsonInclude]
        [JsonPropertyName("gender")]
        public string Gender { get; private set; }

        [JsonInclude]
        [JsonPropertyName("accent")]
        public string Accent { get; private set; }

        [JsonInclude]
        [JsonPropertyName("age")]
        public string Age { get; private set; }

        [JsonInclude]
        [JsonPropertyName("accent_strength")]
        public int AccentStrength { get; private set; }
    }
}
