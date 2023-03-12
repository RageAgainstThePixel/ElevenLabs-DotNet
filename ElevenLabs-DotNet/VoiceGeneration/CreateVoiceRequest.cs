// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.VoiceGeneration
{
    public sealed class CreateVoiceRequest
    {
        public CreateVoiceRequest(string voiceName, string generatedVoiceId = null)
        {
            VoiceName = voiceName;
            GeneratedVoiceId = generatedVoiceId;
        }

        [JsonInclude]
        [JsonPropertyName("voice_name")]
        public string VoiceName { get; }

        [JsonInclude]
        [JsonPropertyName("generated_voice_id")]
        public string GeneratedVoiceId { get; }
    }
}
