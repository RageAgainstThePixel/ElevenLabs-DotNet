// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElevenLabs.VoiceGeneration
{
    public sealed class GeneratedVoiceOptions
    {
        [JsonInclude]
        [JsonPropertyName("genders")]
        public IReadOnlyList<Gender> Genders { get; private set; }

        [JsonInclude]
        [JsonPropertyName("accents")]
        public IReadOnlyList<Accent> Accents { get; private set; }

        [JsonInclude]
        [JsonPropertyName("ages")]
        public IReadOnlyList<Age> Ages { get; private set; }

        [JsonInclude]
        [JsonPropertyName("minimum_characters")]
        public int MinimumCharacters { get; private set; }

        [JsonInclude]
        [JsonPropertyName("maximum_characters")]
        public int MaximumCharacters { get; private set; }

        [JsonInclude]
        [JsonPropertyName("minimum_accent_strength")]
        public double MinimumAccentStrength { get; private set; }

        [JsonInclude]
        [JsonPropertyName("maximum_accent_strength")]
        public double MaximumAccentStrength { get; private set; }
    }
}
