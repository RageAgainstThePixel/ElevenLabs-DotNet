// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElevenLabs.SpeechToText
{
    public sealed record SessionStarted : IServerEvent
    {
        [JsonPropertyName("message_type")]
        public string MessageType { get; init; }

        [JsonPropertyName("session_id")]
        public string SessionId { get; init; }

        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this, ElevenLabsClient.JsonSerializationOptions);
        }
    }

    public sealed record PartialTranscript : IServerEvent
    {
        [JsonPropertyName("message_type")]
        public string MessageType { get; init; }

        [JsonPropertyName("text")]
        public string Text { get; init; }

        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this, ElevenLabsClient.JsonSerializationOptions);
        }
    }

    public sealed record CommittedTranscript : IServerEvent
    {
        [JsonPropertyName("message_type")]
        public string MessageType { get; init; }

        [JsonPropertyName("text")]
        public string Text { get; init; }

        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this, ElevenLabsClient.JsonSerializationOptions);
        }
    }

    public sealed record CommittedTranscriptWithTimestamps : IServerEvent
    {
        [JsonPropertyName("message_type")]
        public string MessageType { get; init; }

        [JsonPropertyName("text")]
        public string Text { get; init; }

        [JsonPropertyName("language_code")]
        public string LanguageCode { get; init; }

        [JsonPropertyName("words")]
        public TranscriptionWord[] Words { get; init; }

        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this, ElevenLabsClient.JsonSerializationOptions);
        }
    }

    public sealed record SpeechToTextError : IServerEvent
    {
        [JsonPropertyName("message_type")]
        public string MessageType { get; init; }

        [JsonPropertyName("error")]
        public string ErrorMessage { get; init; }

        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this, ElevenLabsClient.JsonSerializationOptions);
        }
    }
}