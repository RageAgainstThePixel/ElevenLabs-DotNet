// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElevenLabs.User
{
    public sealed class SubscriptionInfo
    {
        [JsonInclude]
        [JsonPropertyName("tier")]
        public string Tier { get; private set; }

        [JsonInclude]
        [JsonPropertyName("character_count")]
        public int CharacterCount { get; private set; }

        [JsonInclude]
        [JsonPropertyName("character_limit")]
        public int CharacterLimit { get; private set; }

        [JsonInclude]
        [JsonPropertyName("can_extend_character_limit")]
        public bool CanExtendCharacterLimit { get; private set; }

        [JsonInclude]
        [JsonPropertyName("allowed_to_extend_character_limit")]
        public bool AllowedToExtendCharacterLimit { get; private set; }

        [JsonInclude]
        [JsonPropertyName("next_character_count_reset_unix")]
        public int NextCharacterCountResetUnix { get; private set; }

        [JsonIgnore]
        public DateTime NextCharacterCountReset => DateTimeOffset.FromUnixTimeSeconds(NextCharacterCountResetUnix).DateTime;

        [JsonInclude]
        [JsonPropertyName("voice_limit")]
        public int VoiceLimit { get; private set; }

        [JsonInclude]
        [JsonPropertyName("can_extend_voice_limit")]
        public bool CanExtendVoiceLimit { get; private set; }

        [JsonInclude]
        [JsonPropertyName("can_use_instant_voice_cloning")]
        public bool CanUseInstantVoiceCloning { get; private set; }

        [JsonInclude]
        [JsonPropertyName("available_models")]
        public IReadOnlyList<AvailableModel> AvailableModels { get; private set; }

        [JsonInclude]
        [JsonPropertyName("status")]
        public string Status { get; private set; }

        [JsonInclude]
        [JsonPropertyName("next_invoice")]
        public NextInvoice NextInvoice { get; private set; }
    }
}
