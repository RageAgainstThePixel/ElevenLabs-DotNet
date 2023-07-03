// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    public sealed class VoiceSettings
    {
        [JsonConstructor]
        public VoiceSettings(float stability, float similarityBoost, Boolean speakerboost=true, float style=0.45f)
        {
            Stability = stability;
            SimilarityBoost = similarityBoost;
            Style = style;
            SpeakerBoost = speakerboost;
        }

        [JsonPropertyName("stability")]
        public float Stability { get; set; }

        [JsonPropertyName("similarity_boost")]
        public float SimilarityBoost { get; set; }
        [JsonPropertyName("style")]
        public float Style { get; set; }
        [JsonPropertyName("use_speaker_boost")]
        public Boolean SpeakerBoost { get; set; }
    }
}
