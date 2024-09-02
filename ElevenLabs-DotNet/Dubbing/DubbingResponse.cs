// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.Dubbing
{
    public sealed class DubbingResponse
    {
        [JsonInclude]
        [JsonPropertyName("dubbing_id")]
        public string DubbingId { get; private set; }

        [JsonInclude]
        [JsonPropertyName("expected_duration_sec")]
        public float ExpectedDurationSeconds { get; private set; }

        public static implicit operator string(DubbingResponse response) => response?.DubbingId;
    }
}