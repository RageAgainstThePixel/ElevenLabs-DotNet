// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs
{
    public abstract class ElevenLabsBaseEndPoint
    {
        internal ElevenLabsBaseEndPoint(ElevenLabsClient client) => this.client = client;

        // ReSharper disable once InconsistentNaming
        protected readonly ElevenLabsClient client;

        private HttpClient HttpClient => client.Client;

        private bool enableDebug;

        /// <summary>
        /// Enables or disables the logging of all http responses of header and body information for this endpoint.<br/>
        /// WARNING! Enabling this in your production build, could potentially leak sensitive information!
        /// </summary>
        public bool EnableDebug
        {
            get => enableDebug || client.EnableDebug;
            set => enableDebug = value;
        }

        /// <summary>
        /// The root endpoint address.
        /// </summary>
        protected abstract string Root { get; }

        protected virtual string ApiVersion => "v1";

        /// <summary>
        /// Custom headers for this endpoint
        /// </summary>
        internal virtual IReadOnlyDictionary<string, IEnumerable<string>> Headers => null;

        protected Task<HttpResponseMessage> GetAsync(string uri, CancellationToken cancellationToken)
            => SendAsync(new(HttpMethod.Get, uri), cancellationToken);

        protected Task<HttpResponseMessage> PostAsync(string uri, HttpContent content, CancellationToken cancellationToken)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, uri);
            message.Content = content;
            return SendAsync(message, cancellationToken);
        }

        protected Task<HttpResponseMessage> PatchAsync(string uri, HttpContent content, CancellationToken cancellationToken)
        {
            var message = new HttpRequestMessage(HttpMethod.Patch, uri);
            message.Content = content;
            return SendAsync(message, cancellationToken);
        }

        protected Task<HttpResponseMessage> DeleteAsync(string uri, CancellationToken cancellationToken)
            => SendAsync(new(HttpMethod.Delete, uri), cancellationToken);

        protected Task<Stream> GetStreamAsync(string uri, CancellationToken cancellationToken)
            => HttpClient.GetStreamAsync(uri, cancellationToken);

        internal Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken cancellationToken)
        {
            if (Headers is { Count: not 0 })
            {
                foreach (var header in Headers)
                {
                    message.Headers.Add(header.Key, header.Value);
                }
            }

            return HttpClient.SendAsync(message, cancellationToken);
        }

        internal Task<HttpResponseMessage> GetServerSentEventStreamAsync(string uri, CancellationToken cancellationToken)
            => ServerSentEventStreamAsync(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken);

        internal Task<HttpResponseMessage> ServerSentEventStreamAsync(HttpRequestMessage message, CancellationToken cancellationToken)
        {
            if (Headers is { Count: > 0 })
            {
                foreach (var (key, value) in Headers)
                {
                    message.Headers.Add(key, value);
                }
            }

            return HttpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }

        /// <summary>
        /// Gets the full formatted url for the API endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint url.</param>
        /// <param name="queryParameters">Optional, parameters to add to the endpoint.</param>
        protected string GetUrl(string endpoint = "", Dictionary<string, string> queryParameters = null)
            => GetEndpoint(client.Settings.BaseRequestUrlFormat, endpoint, queryParameters);

        protected string GetWebsocketUri(string endpoint = "", Dictionary<string, string> queryParameters = null)
            => GetEndpoint(client.Settings.BaseWebSocketUrlFormat, endpoint, queryParameters);

        private string GetEndpoint(string baseUrlFormat, string endpoint = "", Dictionary<string, string> queryParameters = null)
        {
            var result = string.Format(baseUrlFormat, ApiVersion, $"{Root}{endpoint}");

            foreach (var defaultQueryParameter in client.Settings.DefaultQueryParameters)
            {
                queryParameters ??= new Dictionary<string, string>();
                queryParameters.Add(defaultQueryParameter.Key, defaultQueryParameter.Value);
            }

            if (queryParameters is { Count: not 0 })
            {
                result += $"?{string.Join('&', queryParameters.Select(parameter => $"{parameter.Key}={parameter.Value}"))}";
            }

            return result;
        }
    }
}
