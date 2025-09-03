// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    /// <summary>
    /// State of the voice’s fine-tuning to filter by. Applicable only to professional voices clones.
    /// </summary>
    public enum FineTuningStateTypes
    {
        /// <summary>
        /// Draft state.
        /// </summary>
        [JsonStringEnumMemberName("draft")]
        Draft,
        /// <summary>
        /// Not verified state.
        /// </summary>
        [JsonStringEnumMemberName("not_verified")]
        NotVerified,
        /// <summary>
        /// Not started state.
        /// </summary>
        [JsonStringEnumMemberName("not_started")]
        NotStarted,
        /// <summary>
        /// Queued state.
        /// </summary>
        [JsonStringEnumMemberName("queued")]
        Queued,
        /// <summary>
        /// Fine-tuning in progress.
        /// </summary>
        [JsonStringEnumMemberName("fine_tuning")]
        FineTuning,
        /// <summary>
        /// Fine-tuned state.
        /// </summary>
        [JsonStringEnumMemberName("fine_tuned")]
        FineTuned,
        /// <summary>
        /// Failed state.
        /// </summary>
        [JsonStringEnumMemberName("failed")]
        Failed,
        /// <summary>
        /// Delayed state.
        /// </summary>
        [JsonStringEnumMemberName("delayed")]
        Delayed
    }
}