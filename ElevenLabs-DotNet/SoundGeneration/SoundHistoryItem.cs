// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text.Json.Serialization;

namespace ElevenLabs.SoundGeneration
{
    public sealed class SoundHistoryItem
    {
        [JsonInclude]
        [JsonPropertyName("sound_generation_history_item_id")]
        public string Id { get; private set; }

        [JsonInclude]
        [JsonPropertyName("text")]
        public string Text { get; private set; }

        [JsonInclude]
        [JsonPropertyName("created_at_unix")]
        public int CreatedAtUnixTimeSeconds { get; private set; }

        [JsonIgnore]
        public DateTime CreatedAt => DateTimeOffset.FromUnixTimeSeconds(CreatedAtUnixTimeSeconds).DateTime;

        [JsonInclude]
        [JsonPropertyName("content_type")]
        public string ContentType { get; private set; }

        [JsonInclude]
        [JsonPropertyName("generation_config")]
        public SoundGenerationConfig SoundGenerationConfig { get; private set; }

        public static implicit operator string(SoundHistoryItem item) => item?.Id;
    }
}