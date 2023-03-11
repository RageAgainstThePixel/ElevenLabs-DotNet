// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.User
{
    public sealed class UserInfo
    {
        [JsonInclude]
        [JsonPropertyName("subscription")]
        public SubscriptionInfo SubscriptionInfo { get; private set; }

        [JsonInclude]
        [JsonPropertyName("is_new_user")]
        public bool IsNewUser { get; private set; }

        [JsonInclude]
        [JsonPropertyName("xi_api_key")]
        public string XiApiKey { get; private set; }
    }
}
