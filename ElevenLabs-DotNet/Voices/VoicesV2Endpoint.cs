// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.Voices
{
    /// <summary>
    /// Access to voices created either by you or us.
    /// </summary>
    public sealed class VoicesV2Endpoint : ElevenLabsBaseEndPoint
    {
        public VoicesV2Endpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "voices";
        protected override string ApiVersion => "v2";

        /// <summary>
        /// Gets a list of all available voices for a user, and downloads all their settings.
        /// </summary>
        /// <param name="query">Optional, voice query.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="IReadOnlyList{T}"/> of <see cref="Voice"/>s.</returns>
        public async Task<VoiceList> GetVoicesAsync(VoiceQuery query = null, CancellationToken cancellationToken = default)
        {
            using var response = await GetAsync(GetUrl(queryParameters: query), cancellationToken).ConfigureAwait(false);
            return await response.DeserializeAsync<VoiceList>(EnableDebug, cancellationToken).ConfigureAwait(false);
        }
    }
}
