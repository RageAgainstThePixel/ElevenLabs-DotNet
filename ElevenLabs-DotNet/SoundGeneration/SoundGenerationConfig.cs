// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.SoundGeneration
{
    public sealed class SoundGenerationConfig
    {
        [JsonInclude]
        [JsonPropertyName("number_of_generations")]
        public int NumberOfGenerations { get; private set; }

        [JsonInclude]
        [JsonPropertyName("generation_settings")]
        public GenerationSettings GenerationSettings { get; private set; }

        [JsonInclude]
        [JsonPropertyName("text")]
        public string Text { get; private set; }
    }
}