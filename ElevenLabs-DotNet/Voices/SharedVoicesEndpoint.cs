// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.Voices
{
    public sealed class SharedVoicesEndpoint : ElevenLabsBaseEndPoint
    {
        public SharedVoicesEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "shared-voices";

        /// <summary>
        /// Gets a list of shared voices.
        /// </summary>
        /// <param name="query">Optional, <see cref="SharedVoiceQuery"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SharedVoiceList"/>.</returns>
        public async Task<SharedVoiceList> GetSharedVoicesAsync(SharedVoiceQuery query = null, CancellationToken cancellationToken = default)
        {
            using var response = await GetAsync(GetUrl(queryParameters: query?.ToQueryParams()), cancellationToken).ConfigureAwait(false);
            return await response.DeserializeAsync<SharedVoiceList>(EnableDebug, cancellationToken).ConfigureAwait(false);
        }
    }
}