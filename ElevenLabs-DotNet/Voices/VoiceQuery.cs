// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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
        public string NextPageToken { get; init; }

        /// <summary>
        /// Optional. How many voices to return at maximum. Can not exceed 100, defaults to 10. Page 0 may include more voices due to default voices being included.
        /// </summary>
        public int? PageSize { get; init; }

        /// <summary>
        /// Optional. Search term to filter voices by. Searches in name, description, labels, category.
        /// </summary>
        public string Search { get; init; }

        /// <summary>
        /// Optional. Which field to sort by, one of ‘created_at_unix’ or ‘name’. ‘created_at_unix’ may not be available for older voices.
        /// </summary>
        public string Sort { get; init; }

        /// <summary>
        /// Optional. Which direction to sort the voices in. 'asc' or 'desc'.
        /// </summary>
        public SortDirections? SortDirection { get; init; }

        /// <summary>
        /// Optional. Type of the voice to filter by. One of ‘personal’, ‘community’, ‘default’, ‘workspace’, ‘non-default’. ‘non-default’ is equal to all but ‘default’.
        /// </summary>
        public VoiceTypes? VoiceType { get; init; }

        /// <summary>
        /// Optional. Category of the voice to filter by. One of 'premade', 'cloned', 'generated', 'professional'.
        /// </summary>
        public CategoryTypes? Category { get; init; }

        /// <summary>
        /// Optional. State of the voice’s fine-tuning to filter by. Applicable only to professional voices clones. One of ‘draft’, ‘not_verified’, ‘not_started’, ‘queued’, ‘fine_tuning’, ‘fine_tuned’, ‘failed’, ‘delayed’.
        /// </summary>
        public FineTuningStateTypes? FineTuningState { get; init; }

        /// <summary>
        /// Optional. Collection ID to filter voices by.
        /// </summary>
        public string CollectionId { get; init; }

        /// <summary>
        /// Optional. Whether to include the total count of voices found in the response. Incurs a performance cost. Defaults to true.
        /// </summary>
        public bool? IncludeTotalCount { get; init; }

        /// <summary>
        /// Optional. Voice IDs to lookup by. Maximum 100 voice IDs.
        /// </summary>
        public IReadOnlyList<string> VoiceIds { get; init; }

        public VoiceQuery WithNextPageToken(string nextPageToken) => this with { NextPageToken = nextPageToken };

        public static implicit operator Dictionary<string, string>(VoiceQuery query) => query?.ToQueryParams();

        /// <summary>
        /// Converts the current query object to a dictionary of HTTP query parameters.
        /// </summary>
        public Dictionary<string, string> ToQueryParams()
        {
            var json = JsonSerializer.Serialize(this, ElevenLabsClient.JsonSerializationOptions);
            var dict = new Dictionary<string, string>();
            using var doc = JsonDocument.Parse(json);

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Array)
                {
                    // Flatten arrays as comma-separated values
                    var arr = string.Join(",", prop.Value.EnumerateArray().Select(e => e.GetString()));

                    if (!string.IsNullOrWhiteSpace(arr))
                    {
                        dict.Add(prop.Name, arr);
                    }
                }
                else if (prop.Value.ValueKind != JsonValueKind.Null &&
                         prop.Value.ValueKind != JsonValueKind.Undefined)
                {
                    var val = prop.Value.ToString();

                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        dict.Add(prop.Name, val);
                    }
                }
            }

            return dict;
        }
    }
}