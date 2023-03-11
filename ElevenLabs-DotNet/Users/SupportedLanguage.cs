// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.User
{
    public sealed class SupportedLanguage
    {
        [JsonInclude]
        [JsonPropertyName("iso_code")]
        public string IsoCode { get; private set; }

        [JsonInclude]
        [JsonPropertyName("display_name")]
        public string DisplayName { get; private set; }
    }
}
