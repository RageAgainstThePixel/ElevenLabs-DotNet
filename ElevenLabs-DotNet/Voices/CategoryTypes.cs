// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    /// <summary>
    /// Category of the voice to filter by.
    /// </summary>
    public enum CategoryTypes
    {
        /// <summary>
        /// Premade voice.
        /// </summary>
        [JsonStringEnumMemberName("premade")]
        Premade,
        /// <summary>
        /// Cloned voice.
        /// </summary>
        [JsonStringEnumMemberName("cloned")]
        Cloned,
        /// <summary>
        /// Generated voice.
        /// </summary>
        [JsonStringEnumMemberName("generated")]
        Generated,
        /// <summary>
        /// Professional voice.
        /// </summary>
        [JsonStringEnumMemberName("professional")]
        Professional
    }
}
