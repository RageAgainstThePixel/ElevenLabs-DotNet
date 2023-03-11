// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    public sealed class VoiceSettings
    {
        [JsonInclude]
        [JsonPropertyName("stability")]
        public float Stability { get; private set; }

        [JsonInclude]
        [JsonPropertyName("similarity_boost")]
        public float SimilarityBoost { get; private set; }
    }
}
