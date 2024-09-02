// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.Models
{
    public sealed class ModelsEndpoint : ElevenLabsBaseEndPoint
    {
        public ModelsEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "models";

        /// <summary>
        /// Access the different models available to the platform.
        /// </summary>
        /// <returns>A list of <see cref="Model"/>s you can use.</returns>
        public async Task<IReadOnlyList<Model>> GetModelsAsync(CancellationToken cancellationToken = default)
        {
            using var response = await client.Client.GetAsync(GetUrl(), cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<IReadOnlyList<Model>>(responseAsString, ElevenLabsClient.JsonSerializationOptions);
        }
    }
}
