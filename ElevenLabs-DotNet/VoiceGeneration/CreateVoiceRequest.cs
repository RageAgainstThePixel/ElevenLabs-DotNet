// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.VoiceGeneration
{
    public sealed class CreateVoiceRequest
    {
        [JsonInclude]
        [JsonPropertyName("voice_name")]
        public string VoiceName { get; private set; }

        [JsonInclude]
        [JsonPropertyName("generated_voice_id")]
        public string GeneratedVoiceId { get; private set; }
    }
}
