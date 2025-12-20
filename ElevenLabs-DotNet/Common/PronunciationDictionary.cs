// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text.Json.Serialization;

namespace ElevenLabs
{
    public sealed record PronunciationDictionary
    {
        [JsonInclude]
        [JsonPropertyName("id")]
        public string Id { get; private set; }

        [JsonInclude]
        [JsonPropertyName("latest_version_id")]
        public string LatestVersionId { get; private set; }

        [JsonInclude]
        [JsonPropertyName("latest_version_rules_num")]
        public int LatestVersionRulesNum { get; private set; }

        [JsonInclude]
        [JsonPropertyName("name")]
        public string Name { get; private set; }

        [JsonInclude]
        [JsonPropertyName("permission_on_resource")]
        public string PermissionOnResource { get; private set; }

        [JsonInclude]
        [JsonPropertyName("created_by")]
        public string CreatedBy { get; private set; }

        [JsonInclude]
        [JsonPropertyName("creation_time_unix")]
        public long CreationTimeUnix { get; private set; }

        [JsonIgnore]
        public DateTime CreationTime
            => DateTimeOffset.FromUnixTimeSeconds(CreationTimeUnix).DateTime;

        [JsonInclude]
        [JsonPropertyName("archived_time_unix")]
        public long? ArchivedTimeUnix { get; private set; }

        [JsonIgnore]
        public DateTime? ArchivedTime
            => ArchivedTimeUnix.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(ArchivedTimeUnix.Value).DateTime
                : null;

        [JsonInclude]
        [JsonPropertyName("description")]
        public string Description { get; private set; }
    }
}
