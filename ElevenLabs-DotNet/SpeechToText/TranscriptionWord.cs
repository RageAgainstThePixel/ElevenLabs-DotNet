// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.SpeechToText
{
    public sealed record TranscriptionWord
    {
        [JsonPropertyName("text")]
        public string Text { get; init; }

        [JsonPropertyName("start")]
        public double Start { get; init; }

        [JsonPropertyName("end")]
        public double End { get; init; }

        [JsonPropertyName("type")]
        public string Type { get; init; }

        [JsonPropertyName("speaker_id")]
        public string SpeakerId { get; init; }

        [JsonPropertyName("logprob")]
        public double LogProb { get; init; }
    }
}