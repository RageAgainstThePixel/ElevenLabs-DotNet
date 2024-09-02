// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.SoundGeneration
{
    public sealed class GenerationSettings
    {
        [JsonInclude]
        [JsonPropertyName("duration_seconds")]
        public float? Duration { get; private set; }

        [JsonInclude]
        [JsonPropertyName("prompt_influence")]
        public float PromptInfluence { get; private set; }
    }
}