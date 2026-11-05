// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElevenLabs.SpeechToText
{
    public sealed record InputAudioChunk : IClientEvent
    {
        public InputAudioChunk(string audioBase64, bool commit = false, int? sampleRate = null)
        {
            AudioBase64 = audioBase64;
            Commit = commit;
            SampleRate = sampleRate;
        }

        [JsonPropertyName("message_type")]
        public string MessageType { get; } = "input_audio_chunk";

        [JsonPropertyName("audio_base_64")]
        public string AudioBase64 { get; }

        [JsonPropertyName("commit")]
        public bool Commit { get; }

        [JsonPropertyName("sample_rate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? SampleRate { get; }

        [JsonPropertyName("previous_text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PreviousText { get; init; }

        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this, ElevenLabsClient.JsonSerializationOptions);
        }
    }
}