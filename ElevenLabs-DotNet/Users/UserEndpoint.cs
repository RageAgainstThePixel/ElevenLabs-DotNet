// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System.Text.Json;
using System.Threading.Tasks;

namespace ElevenLabs.User
{
    /// <summary>
    /// Access to your user account information.
    /// </summary>
    public sealed class UserEndpoint : ElevenLabsBaseEndPoint
    {
        public UserEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "user";

        /// <summary>
        /// Gets information about your user account.
        /// </summary>
        public async Task<UserInfo> GetUserInfoAsync()
        {
            var response = await client.Client.GetAsync(GetUrl());
            var responseAsString = await response.ReadAsStringAsync(EnableDebug);
            return JsonSerializer.Deserialize<UserInfo>(responseAsString, ElevenLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Gets your subscription info.
        /// </summary>
        public async Task<SubscriptionInfo> GetSubscriptionInfoAsync()
        {
            var response = await client.Client.GetAsync(GetUrl("/subscription"));
            var responseAsString = await response.ReadAsStringAsync(EnableDebug);
            return JsonSerializer.Deserialize<SubscriptionInfo>(responseAsString, ElevenLabsClient.JsonSerializationOptions);
        }
    }
}
