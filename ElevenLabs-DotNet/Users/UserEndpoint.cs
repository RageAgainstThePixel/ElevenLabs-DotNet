// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System.Threading;
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
        public async Task<UserInfo> GetUserInfoAsync(CancellationToken cancellationToken = default)
        {
            using var response = await GetAsync(GetUrl(), cancellationToken).ConfigureAwait(false);
            return await response.DeserializeAsync<UserInfo>(EnableDebug, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets your subscription info.
        /// </summary>
        public async Task<SubscriptionInfo> GetSubscriptionInfoAsync(CancellationToken cancellationToken = default)
        {
            using var response = await GetAsync(GetUrl("/subscription"), cancellationToken).ConfigureAwait(false);
            return await response.DeserializeAsync<SubscriptionInfo>(EnableDebug, cancellationToken).ConfigureAwait(false);
        }
    }
}
