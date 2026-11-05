// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.SpeechToText
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AudioFormat
    {
        [JsonPropertyName("pcm_8000")]
        Pcm_8000,
        [JsonPropertyName("pcm_16000")]
        Pcm_16000,
        [JsonPropertyName("pcm_22050")]
        Pcm_22050,
        [JsonPropertyName("pcm_24000")]
        Pcm_24000,
        [JsonPropertyName("pcm_44100")]
        Pcm_44100,
        [JsonPropertyName("pcm_48000")]
        Pcm_48000,
        [JsonPropertyName("ulaw_8000")]
        Ulaw_8000
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CommitStrategy
    {
        [JsonPropertyName("manual")]
        Manual,
        [JsonPropertyName("vad")]
        Vad
    }
}