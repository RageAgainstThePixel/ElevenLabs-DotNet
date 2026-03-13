// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.SpeechToText
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AudioFormat
    {
        [JsonStringEnumMemberName("pcm_8000")]
        Pcm8000,
        [JsonStringEnumMemberName("pcm_16000")]
        Pcm16000,
        [JsonStringEnumMemberName("pcm_22050")]
        Pcm22050,
        [JsonStringEnumMemberName("pcm_24000")]
        Pcm24000,
        [JsonStringEnumMemberName("pcm_44100")]
        Pcm44100,
        [JsonStringEnumMemberName("pcm_48000")]
        Pcm48000,
        [JsonStringEnumMemberName("ulaw_8000")]
        Ulaw8000
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