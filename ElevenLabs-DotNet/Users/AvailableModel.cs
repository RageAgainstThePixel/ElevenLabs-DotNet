// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElevenLabs.User
{
    public sealed class AvailableModel
    {
        [JsonInclude]
        [JsonPropertyName("model_id")]
        public string ModelId { get; private set; }

        [JsonInclude]
        [JsonPropertyName("display_name")]
        public string DisplayName { get; private set; }

        [JsonInclude]
        [JsonPropertyName("supported_languages")]
        public IReadOnlyList<SupportedLanguage> SupportedLanguages { get; private set; }
    }
}
