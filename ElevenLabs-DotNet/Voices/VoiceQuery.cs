// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections.Generic;

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
        Personal,
        /// <summary>
        /// Community voice.
        /// </summary>
        Community,
        /// <summary>
        /// Default voice.
        /// </summary>
        Default,
        /// <summary>
        /// Workspace voice.
        /// </summary>
        Workspace,
        /// <summary>
        /// Non-default voice (all but 'default').
        /// </summary>
        NonDefault
    }

    /// <summary>
    /// Category of the voice to filter by.
    /// </summary>
    public enum CategoryTypes
    {
        /// <summary>
        /// Premade voice.
        /// </summary>
        Premade,
        /// <summary>
        /// Cloned voice.
        /// </summary>
        Cloned,
        /// <summary>
        /// Generated voice.
        /// </summary>
        Generated,
        /// <summary>
        /// Professional voice.
        /// </summary>
        Professional
    }

    /// <summary>
    /// State of the voice’s fine tuning to filter by. Applicable only to professional voices clones.
    /// </summary>
    public enum FineTuningStateTypes
    {
        /// <summary>
        /// Draft state.
        /// </summary>
        Draft,
        /// <summary>
        /// Not verified state.
        /// </summary>
        NotVerified,
        /// <summary>
        /// Not started state.
        /// </summary>
        NotStarted,
        /// <summary>
        /// Queued state.
        /// </summary>
        Queued,
        /// <summary>
        /// Fine tuning in progress.
        /// </summary>
        FineTuning,
        /// <summary>
        /// Fine tuned state.
        /// </summary>
        FineTuned,
        /// <summary>
        /// Failed state.
        /// </summary>
        Failed,
        /// <summary>
        /// Delayed state.
        /// </summary>
        Delayed
    }

    public sealed class VoiceQuery
    {
        public string NextPageToken { get; set; } = null;
        public int? PageSize { get; set; } = null;
        public string Search { get; set; } = null;
        public string Sort { get; set; } = null;
        public string SortDirection { get; set; } = null;
        public VoiceTypes? VoiceType { get; set; } = null;
        public CategoryTypes? Category { get; set; } = null;
        public FineTuningStateTypes? FineTuningState { get; set; } = null;
        public string CollectionId { get; set; } = null;
        public bool? IncludeTotalCount { get; set; } = null;
        public List<string> VoiceIds { get; set; } = null;

        public Dictionary<string, string> ToQueryParams()
        {
            var parameters = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(NextPageToken))
            {
                parameters.Add("next_page_token", NextPageToken);
            }

            if (PageSize.HasValue)
            {
                parameters.Add("page_size", PageSize.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(Search))
            {
                parameters.Add("search", Search);
            }

            if (!string.IsNullOrWhiteSpace(Sort))
            {
                parameters.Add("sort", Sort);
            }

            if (!string.IsNullOrWhiteSpace(SortDirection))
            {
                parameters.Add("sort_direction", SortDirection);
            }

            if (VoiceType.HasValue)
            {
                string voiceTypeString = VoiceType.Value switch
                {
                    VoiceTypes.Personal => "personal",
                    VoiceTypes.Community => "community",
                    VoiceTypes.Default => "default",
                    VoiceTypes.Workspace => "workspace",
                    VoiceTypes.NonDefault => "non-default",
                    _ => null
                };
                if (!string.IsNullOrWhiteSpace(voiceTypeString))
                {
                    parameters.Add("voice_type", voiceTypeString);
                }
            }

            if (Category.HasValue)
            {
                string categoryString = Category.Value switch
                {
                    CategoryTypes.Premade => "premade",
                    CategoryTypes.Cloned => "cloned",
                    CategoryTypes.Generated => "generated",
                    CategoryTypes.Professional => "professional",
                    _ => null
                };
                if (!string.IsNullOrWhiteSpace(categoryString))
                {
                    parameters.Add("category", categoryString);
                }
            }

            if (FineTuningState.HasValue)
            {
                string fineTuningStateString = FineTuningState.Value switch
                {
                    FineTuningStateTypes.Draft => "draft",
                    FineTuningStateTypes.NotVerified => "not_verified",
                    FineTuningStateTypes.NotStarted => "not_started",
                    FineTuningStateTypes.Queued => "queued",
                    FineTuningStateTypes.FineTuning => "fine_tuning",
                    FineTuningStateTypes.FineTuned => "fine_tuned",
                    FineTuningStateTypes.Failed => "failed",
                    FineTuningStateTypes.Delayed => "delayed",
                    _ => null
                };
                if (!string.IsNullOrWhiteSpace(fineTuningStateString))
                {
                    parameters.Add("fine_tuning_state", fineTuningStateString);
                }
            }

            if (!string.IsNullOrWhiteSpace(CollectionId))
            {
                parameters.Add("collection_id", CollectionId);
            }

            if (IncludeTotalCount.HasValue)
            {
                parameters.Add("include_total_count", IncludeTotalCount.Value.ToString().ToLower());
            }

            if (VoiceIds is { Count: > 0 })
            {
                parameters.Add("voice_ids", string.Join(",", VoiceIds));
            }

            return parameters;
        }
    }
}