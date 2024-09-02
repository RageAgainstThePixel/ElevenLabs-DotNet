// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElevenLabs.Dubbing
{
    public sealed class DubbingProjectMetadata
    {
        [JsonInclude]
        [JsonPropertyName("dubbing_id")]
        public string DubbingId { get; private set; }

        [JsonInclude]
        [JsonPropertyName("name")]
        public string Name { get; private set; }

        [JsonInclude]
        [JsonPropertyName("status")]
        public string Status { get; private set; }

        [JsonInclude]
        [JsonPropertyName("target_languages")]
        public List<string> TargetLanguages { get; private set; }

        [JsonInclude]
        [JsonPropertyName("error")]
        public string Error { get; private set; }

        [JsonIgnore]
        public float ExpectedDurationSeconds { get; internal set; }

        [JsonIgnore]
        public TimeSpan TimeCompleted { get; internal set; }
    }
}
