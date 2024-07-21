// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    public sealed class SharedVoiceList
    {
        [JsonInclude]
        [JsonPropertyName("voices")]
        public IReadOnlyList<SharedVoiceInfo> Voices { get; private set; }

        [JsonInclude]
        [JsonPropertyName("has_more")]
        public bool HasMore { get; private set; }

        [JsonInclude]
        [JsonPropertyName("last_sort_id")]
        public string LastId { get; private set; }
    }
}