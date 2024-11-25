// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs
{
    /// <summary>
    /// Represents timing information for a single character in the transcript
    /// </summary>
    public class TimestampedTranscriptCharacter
    {
        public TimestampedTranscriptCharacter() { }

        internal TimestampedTranscriptCharacter(string character, double startTime, double endTime)
        {
            Character = character;
            StartTime = startTime;
            EndTime = endTime;
        }

        /// <summary>
        /// The character being spoken
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("character")]
        public string Character { get; private set; }

        /// <summary>
        /// The time in seconds when this character starts being spoken
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("character_start_times_seconds")]
        public double StartTime { get; private set; }

        /// <summary>
        /// The time in seconds when this character finishes being spoken
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("character_end_times_seconds")]
        public double EndTime { get; private set; }
    }
}
