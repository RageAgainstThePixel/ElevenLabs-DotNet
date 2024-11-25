// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.TextToSpeech
{
    internal sealed class Alignment
    {
        [JsonInclude]
        [JsonPropertyName("characters")]
        public string[] Characters { get; private set; }

        [JsonInclude]
        [JsonPropertyName("character_start_times_seconds")]
        public double[] StartTimes { get; private set; }

        [JsonInclude]
        [JsonPropertyName("character_end_times_seconds")]
        public double[] EndTimes { get; private set; }

        public static implicit operator TimestampedTranscriptCharacter[](Alignment alignment)
        {
            if (alignment == null) { return null; }
            var characters = alignment.Characters;
            var startTimes = alignment.StartTimes;
            var endTimes = alignment.EndTimes;
            var timestampedTranscriptCharacters = new TimestampedTranscriptCharacter[characters.Length];

            for (var i = 0; i < characters.Length; i++)
            {
                timestampedTranscriptCharacters[i] = new TimestampedTranscriptCharacter(characters[i], startTimes[i], endTimes[i]);
            }

            return timestampedTranscriptCharacters;
        }
    }
}
