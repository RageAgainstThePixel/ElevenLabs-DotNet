// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    public sealed class Voice
    {
        public Voice(string id)
        {
            Id = id;
        }

        public static implicit operator string(Voice voice) => voice.Id;

        [JsonInclude]
        [JsonPropertyName("voice_id")]
        public string Id { get; private set; }

        [JsonInclude]
        [JsonPropertyName("name")]
        public string Name { get; private set; }

        [JsonInclude]
        [JsonPropertyName("samples")]
        public IReadOnlyList<Sample> Samples { get; private set; }

        [JsonInclude]
        [JsonPropertyName("category")]
        public string Category { get; private set; }

        [JsonInclude]
        [JsonPropertyName("labels")]
        public IReadOnlyDictionary<string, string> Labels { get; private set; }

        [JsonInclude]
        [JsonPropertyName("preview_url")]
        public string PreviewUrl { get; private set; }

        [JsonInclude]
        [JsonPropertyName("available_for_tiers")]
        public IReadOnlyList<string> AvailableForTiers { get; private set; }

        [JsonInclude]
        [JsonPropertyName("settings")]
        public VoiceSettings Settings { get; internal set; }
    }
}
