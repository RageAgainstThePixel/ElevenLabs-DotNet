// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    /// <summary>
    /// Type of the voice to filter by.
    /// </summary>
    public enum VoiceTypes
    {
        /// <summary>
        /// Personal voice.
        /// </summary>
        [JsonStringEnumMemberName("personal")]
        Personal,
        /// <summary>
        /// Community voice.
        /// </summary>
        [JsonStringEnumMemberName("community")]
        Community,
        /// <summary>
        /// Default voice.
        /// </summary>
        [JsonStringEnumMemberName("default")]
        Default,
        /// <summary>
        /// Workspace voice.
        /// </summary>
        [JsonStringEnumMemberName("workspace")]
        Workspace,
        /// <summary>
        /// Non-default voice (all but 'default').
        /// </summary>
        [JsonStringEnumMemberName("non-default")]
        NonDefault
    }
}
