// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text.Json.Serialization;

namespace ElevenLabs.TextToSpeech
{
    internal sealed class TranscriptionResponse
    {
        [JsonInclude]
        [JsonPropertyName("audio_base64")]
        public string AudioBase64 { get; private set; }

        [JsonIgnore]
        public ReadOnlyMemory<byte> AudioBytes => Convert.FromBase64String(AudioBase64);

        [JsonInclude]
        [JsonPropertyName("alignment")]
        public Alignment Alignment { get; private set; }
    }
}
