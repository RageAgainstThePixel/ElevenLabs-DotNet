// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    public sealed class SharedVoiceInfo
    {
        [JsonInclude]
        [JsonPropertyName("public_owner_id")]
        public string OwnerId { get; private set; }

        [JsonInclude]
        [JsonPropertyName("voice_id")]
        public string VoiceId { get; private set; }

        [JsonInclude]
        [JsonPropertyName("date_unix")]
        public int DateUnix { get; private set; }

        [JsonIgnore]
        public DateTime Date => DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime;

        [JsonInclude]
        [JsonPropertyName("name")]
        public string Name { get; private set; }

        [JsonInclude]
        [JsonPropertyName("accent")]
        public string Accent { get; private set; }

        [JsonInclude]
        [JsonPropertyName("gender")]
        public string Gender { get; private set; }

        [JsonInclude]
        [JsonPropertyName("age")]
        public string Age { get; private set; }

        [JsonInclude]
        [JsonPropertyName("descriptive")]
        public string Descriptive { get; private set; }

        [JsonInclude]
        [JsonPropertyName("use_case")]
        public string UseCase { get; private set; }

        [JsonInclude]
        [JsonPropertyName("category")]
        public string Category { get; private set; }

        [JsonInclude]
        [JsonPropertyName("language")]
        public string Language { get; private set; }

        [JsonInclude]
        [JsonPropertyName("description")]
        public string Description { get; private set; }

        [JsonInclude]
        [JsonPropertyName("preview_url")]
        public string PreviewUrl { get; private set; }

        [JsonInclude]
        [JsonPropertyName("usage_character_count_1y")]
        public int UsageCharacterCount1Y { get; private set; }

        [JsonInclude]
        [JsonPropertyName("usage_character_count_7d")]
        public int UsageCharacterCount7D { get; private set; }

        [JsonInclude]
        [JsonPropertyName("play_api_usage_character_count_1y")]
        public int PlayApiUsageCharacterCount1Y { get; private set; }

        [JsonInclude]
        [JsonPropertyName("cloned_by_count")]
        public int ClonedByCount { get; private set; }

        [JsonInclude]
        [JsonPropertyName("rate")]
        public float Rate { get; private set; }

        [JsonInclude]
        [JsonPropertyName("free_users_allowed")]
        public bool FreeUsersAllowed { get; private set; }

        [JsonInclude]
        [JsonPropertyName("live_moderation_enabled")]
        public bool LiveModerationEnabled { get; private set; }

        [JsonInclude]
        [JsonPropertyName("featured")]
        public bool Featured { get; private set; }

        [JsonInclude]
        [JsonPropertyName("notice_period")]
        public int? NoticePeriod { get; private set; }

        [JsonInclude]
        [JsonPropertyName("instagram_username")]
        public string InstagramUsername { get; private set; }

        [JsonInclude]
        [JsonPropertyName("twitter_username")]
        public string TwitterUsername { get; private set; }

        [JsonInclude]
        [JsonPropertyName("youtube_username")]
        public string YoutubeUsername { get; private set; }

        [JsonInclude]
        [JsonPropertyName("tiktok_username")]
        public string TikTokUsername { get; private set; }

        [JsonInclude]
        [JsonPropertyName("image_url")]
        public string ImageUrl { get; private set; }
    }
}