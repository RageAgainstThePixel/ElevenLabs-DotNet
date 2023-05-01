// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.Models
{
    public sealed class Language
    {
        [JsonInclude]
        [JsonPropertyName("language_id")]
        public string Id { get; private set; }

        [JsonInclude]
        [JsonPropertyName("name")]
        public string Name { get; private set; }
    }
}