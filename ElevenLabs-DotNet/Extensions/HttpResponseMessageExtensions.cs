// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.Extensions
{
    internal static class HttpResponseMessageExtensions
    {
        public static async Task<string> ReadAsStringAsync(this HttpResponseMessage response, bool debugResponse = false, CancellationToken cancellationToken = default, [CallerMemberName] string methodName = null)
        {
            var responseAsString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"{methodName} Failed!\n{response.RequestMessage}\n[{response.StatusCode}] {responseAsString}", null, response.StatusCode);
            }

            if (debugResponse)
            {
                Console.WriteLine($"{response.RequestMessage}\n[{response.StatusCode}] {responseAsString}");
            }

            return responseAsString;
        }

        internal static async Task CheckResponseAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default, [CallerMemberName] string methodName = null)
        {
            if (!response.IsSuccessStatusCode)
            {
                var responseAsString = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"{methodName} Failed! HTTP status code: {response.StatusCode} | Response body: {responseAsString}", null, response.StatusCode);
            }
        }
    }
}
