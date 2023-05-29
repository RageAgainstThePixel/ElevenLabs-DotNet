// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    public sealed class VoiceSettings
    {
        [JsonConstructor]
        public VoiceSettings(float stability, float similarityBoost)
        {
            Stability = stability;
            SimilarityBoost = similarityBoost;
        }

        [JsonPropertyName("stability")]
        public float Stability { get; set; }

        [JsonPropertyName("similarity_boost")]
        public float SimilarityBoost { get; set; }
    }
}
