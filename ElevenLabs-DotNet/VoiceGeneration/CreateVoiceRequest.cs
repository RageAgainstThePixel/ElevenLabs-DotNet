// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.VoiceGeneration
{
    public sealed class CreateVoiceRequest
    {
        public CreateVoiceRequest(string voiceName, string description, string generatedVoiceId = null)
        {
            VoiceName = voiceName;
            Description = description;
            GeneratedVoiceId = generatedVoiceId;
        }

        [JsonInclude]
        [JsonPropertyName("voice_name")]
        public string VoiceName { get; }

        [JsonPropertyName("voice_description")]
        public string Description { get; }

        [JsonInclude]
        [JsonPropertyName("generated_voice_id")]
        public string GeneratedVoiceId { get; }
    }
}
