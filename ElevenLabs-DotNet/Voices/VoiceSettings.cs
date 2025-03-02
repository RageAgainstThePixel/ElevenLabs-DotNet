// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    public sealed class VoiceSettings
    {
        [Obsolete("use new .ctr overload")]
        public VoiceSettings(
            float stability = .75f,
            float similarityBoost = .75f,
            bool speakerBoost = true,
            float style = 0.45f)
            : this(stability, similarityBoost, style, speakerBoost)
        {
        }

        [JsonConstructor]
        public VoiceSettings(
            float stability = .75f,
            float similarityBoost = .75f,
            float style = 0.45f,
            bool speakerBoost = true,
            float speed = 1f)
        {
            Stability = stability;
            SimilarityBoost = similarityBoost;
            Style = style;
            SpeakerBoost = speakerBoost;
            Speed = speed;
        }

        [JsonPropertyName("stability")]
        public float Stability { get; set; }

        [JsonPropertyName("similarity_boost")]
        public float SimilarityBoost { get; set; }

        [JsonPropertyName("style")]
        public float Style { get; set; }

        [JsonPropertyName("use_speaker_boost")]
        public bool SpeakerBoost { get; set; }

        [JsonPropertyName("speed")]
        public float Speed { get; set; }
    }
}
