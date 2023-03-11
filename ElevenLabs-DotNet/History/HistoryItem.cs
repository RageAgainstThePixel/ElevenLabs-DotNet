// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text.Json.Serialization;

namespace ElevenLabs.History
{
    public sealed class HistoryItem
    {
        public static implicit operator string(HistoryItem historyItem) => historyItem.Id;

        [JsonInclude]
        [JsonPropertyName("history_item_id")]
        public string Id { get; private set; }

        [JsonInclude]
        [JsonPropertyName("voice_id")]
        public string VoiceId { get; private set; }

        [JsonInclude]
        [JsonPropertyName("voice_name")]
        public string VoiceName { get; private set; }

        [JsonInclude]
        [JsonPropertyName("text")]
        public string Text { get; private set; }

        [JsonInclude]
        [JsonPropertyName("date_unix")]
        public int DateUnix { get; private set; }

        [JsonIgnore]
        public DateTime Date => DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime;

        [JsonInclude]
        [JsonPropertyName("character_count_change_from")]
        public int CharacterCountChangeFrom { get; private set; }

        [JsonInclude]
        [JsonPropertyName("character_count_change_to")]
        public int CharacterCountChangeTo { get; private set; }

        [JsonInclude]
        [JsonPropertyName("content_type")]
        public string ContentType { get; private set; }

        [JsonInclude]
        [JsonPropertyName("state")]
        public string State { get; private set; }
    }
}
