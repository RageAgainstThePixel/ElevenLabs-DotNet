// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.SpeechToText
{
    public sealed record SpeechToTextResponse
    {
        [JsonPropertyName("text")]
        public string Text { get; init; }

        [JsonPropertyName("language_code")]
        public string LanguageCode { get; init; }

        [JsonPropertyName("language_probability")]
        public double LanguageProbability { get; init; }

        [JsonPropertyName("words")]
        public TranscriptionWord[] Words { get; init; }

        [JsonPropertyName("transcription_id")]
        public string TranscriptionId { get; init; }
    }
}