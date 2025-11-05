// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs
{
    public sealed class PronunciationDictionaryLocator
    {
        public PronunciationDictionaryLocator(string id, string version)
        {
            Id = id;
            Version = version;
        }

        [JsonInclude]
        [JsonPropertyName("pronunciation_dictionary_id")]
        public string Id { get; private set; }

        [JsonInclude]
        [JsonPropertyName("version_id")]
        public string Version { get; private set; }

        public static implicit operator PronunciationDictionaryLocator(PronunciationDictionary dict)
            => new(dict.Id, dict.LatestVersionId);
    }
}
