// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.Dubbing
{
    public enum DubbingFormat
    {
        [JsonStringEnumMemberName("srt")]
        Srt,
        [JsonStringEnumMemberName("webvtt")]
        WebVtt
    }
}
