// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    /// <summary>
    /// Represents a container for a collection of voices with pagination metadata.
    /// </summary>
    public sealed class VoiceList
    {
        [JsonInclude]
        [JsonPropertyName("voices")]
        public IReadOnlyList<Voice> Voices { get; private set; }

        [JsonInclude]
        [JsonPropertyName("has_more")]
        public bool HasMore { get; private set; }

        [JsonInclude]
        [JsonPropertyName("total_count")]
        public int TotalCount { get; private set; }

        [JsonInclude]
        [JsonPropertyName("next_page_token")]
        public string NextPageToken { get; private set; }
    }
}
