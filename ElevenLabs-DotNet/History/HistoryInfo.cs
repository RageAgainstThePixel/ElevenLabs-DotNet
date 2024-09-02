// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElevenLabs.History
{
    public sealed class HistoryInfo<T>
    {
        [JsonInclude]
        [JsonPropertyName("history")]
        public IReadOnlyList<T> HistoryItems { get; private set; }

        [JsonInclude]
        [JsonPropertyName("last_history_item_id")]
        public string LastHistoryItemId { get; }

        [JsonInclude]
        [JsonPropertyName("has_more")]
        public bool HasMore { get; }
    }
}