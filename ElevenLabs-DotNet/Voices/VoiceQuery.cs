// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    /// <summary>
    /// Represents a container for query parameters used by the VoicesV2Endpoint.
    /// </summary>
    public sealed record VoiceQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceQuery"/> class with optional parameters.
        /// </summary>
        public VoiceQuery(
            string nextPageToken = null,
            int? pageSize = null,
            string search = null,
            string sort = null,
            SortDirections? sortDirection = null,
            VoiceTypes? voiceType = null,
            CategoryTypes? category = null,
            FineTuningStateTypes? fineTuningState = null,
            string collectionId = null,
            bool? includeTotalCount = null,
            IEnumerable<string> voiceIds = null)
        {
            NextPageToken = nextPageToken;
            PageSize = pageSize;
            Search = search;
            Sort = sort;
            SortDirection = sortDirection;
            VoiceType = voiceType;
            Category = category;
            FineTuningState = fineTuningState;
            CollectionId = collectionId;
            IncludeTotalCount = includeTotalCount;
            VoiceIds = voiceIds?.ToList();
        }

        /// <summary>
        /// Optional. The next page token to use for pagination. Returned from the previous request.
        /// </summary>
        [JsonPropertyName("next_page_token")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string NextPageToken { get; init; }

        /// <summary>
        /// Optional. How many voices to return at maximum. Can not exceed 100, defaults to 10. Page 0 may include more voices due to default voices being included.
        /// </summary>
        [JsonPropertyName("page_size")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? PageSize { get; init; }

        /// <summary>
        /// Optional. Search term to filter voices by. Searches in name, description, labels, category.
        /// </summary>
        [JsonPropertyName("search")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Search { get; init; }

        /// <summary>
        /// Optional. Which field to sort by, one of ‘created_at_unix’ or ‘name’. ‘created_at_unix’ may not be available for older voices.
        /// </summary>
        [JsonPropertyName("sort")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Sort { get; init; }

        /// <summary>
        /// Optional. Which direction to sort the voices in. 'asc' or 'desc'.
        /// </summary>
        [JsonPropertyName("sort_direction")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SortDirections? SortDirection { get; init; }

        /// <summary>
        /// Optional. Type of the voice to filter by. One of ‘personal’, ‘community’, ‘default’, ‘workspace’, ‘non-default’. ‘non-default’ is equal to all but ‘default’.
        /// </summary>
        [JsonPropertyName("voice_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public VoiceTypes? VoiceType { get; init; }

        /// <summary>
        /// Optional. Category of the voice to filter by. One of 'premade', 'cloned', 'generated', 'professional'.
        /// </summary>
        [JsonPropertyName("category")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CategoryTypes? Category { get; init; }

        /// <summary>
        /// Optional. State of the voice’s fine-tuning to filter by. Applicable only to professional voices clones. One of ‘draft’, ‘not_verified’, ‘not_started’, ‘queued’, ‘fine_tuning’, ‘fine_tuned’, ‘failed’, ‘delayed’.
        /// </summary>
        [JsonPropertyName("fine_tuning_state")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FineTuningStateTypes? FineTuningState { get; init; }

        /// <summary>
        /// Optional. Collection ID to filter voices by.
        /// </summary>
        [JsonPropertyName("collection_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string CollectionId { get; init; }

        /// <summary>
        /// Optional. Whether to include the total count of voices found in the response. Incurs a performance cost. Defaults to true.
        /// </summary>
        [JsonPropertyName("include_total_count")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IncludeTotalCount { get; init; }

        /// <summary>
        /// Optional. Voice IDs to lookup by. Maximum 100 voice IDs.
        /// </summary>
        [JsonPropertyName("voice_ids")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyList<string> VoiceIds { get; init; }

        public VoiceQuery WithNextPageToken(string nextPageToken) => this with { NextPageToken = nextPageToken };

        public static implicit operator Dictionary<string, string>(VoiceQuery query) => query?.ToQueryParams();

        /// <summary>
        /// Converts the current query object to a dictionary of HTTP query parameters.
        /// </summary>
        public Dictionary<string, string> ToQueryParams()
        {
            var parameters = new Dictionary<string, string>();
            var json = JsonSerializer.Serialize(this, ElevenLabsClient.JsonSerializationOptions);
            using var doc = JsonDocument.Parse(json);

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.Array:
                    {
                        // Flatten arrays as comma-separated values
                        var array = string.Join(",", property.Value.EnumerateArray().Select(e => e.GetString()));

                        if (!string.IsNullOrWhiteSpace(array))
                        {
                            parameters.Add(property.Name, array);
                        }

                        break;
                    }
                    default:
                    {
                        if (property.Value.ValueKind != JsonValueKind.Null &&
                            property.Value.ValueKind != JsonValueKind.Undefined)
                        {
                            var value = property.Value.ToString();

                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                parameters.Add(property.Name, value);
                            }
                        }

                        break;
                    }
                }
            }

            return parameters;
        }
    }
}