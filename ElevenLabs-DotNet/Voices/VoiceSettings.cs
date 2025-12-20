// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    public sealed class VoiceSettings
    {
        public VoiceSettings() { }

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
        public float Stability { get; set; } = .75f;

        [JsonPropertyName("similarity_boost")]
        public float SimilarityBoost { get; set; } = .75f;

        [JsonPropertyName("style")]
        public float Style { get; set; } = 0.45f;

        [JsonPropertyName("use_speaker_boost")]
        public bool SpeakerBoost { get; set; } = true;

        [JsonPropertyName("speed")]
        public float Speed { get; set; } = 1f;
    }
}
