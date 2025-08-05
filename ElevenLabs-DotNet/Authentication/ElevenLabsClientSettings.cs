// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace ElevenLabs
{
    public sealed class ElevenLabsClientSettings
    {
        internal const string Http = "http://";
        internal const string Https = "https://";
        internal const string DefaultApiVersion = "v1";
        internal const string ElevenLabsDomain = "api.elevenlabs.io";

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsClientSettings"/> for use with ElevenLabs API.
        /// </summary>
        public ElevenLabsClientSettings()
        {
            Domain = ElevenLabsDomain;
            ApiVersion = "v1";
            BaseVersionedRequestUrlFormat = $"{Https}{Domain}/{{1}}/{{0}}";
        }

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsClientSettings"/> for use with ElevenLabs API.
        /// </summary>
        /// <param name="domain">Base api domain.</param>
        /// <param name="apiVersion">The version of the ElevenLabs api you want to use.</param>
        public ElevenLabsClientSettings(string domain, string apiVersion = DefaultApiVersion)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                domain = ElevenLabsDomain;
            }

            if (!domain.Contains('.') &&
                !domain.Contains(':'))
            {
                throw new ArgumentException($"You're attempting to pass a \"resourceName\" parameter to \"{nameof(domain)}\". Please specify \"resourceName:\" for this parameter in constructor.");
            }

            if (string.IsNullOrWhiteSpace(apiVersion))
            {
                apiVersion = DefaultApiVersion;
            }

            var protocol = Https;

            if (domain.StartsWith(Http))
            {
                protocol = Http;
                domain = domain.Replace(Http, string.Empty);
            }
            else if (domain.StartsWith(Https))
            {
                protocol = Https;
                domain = domain.Replace(Https, string.Empty);
            }

            Domain = $"{protocol}{domain}";
            ApiVersion = apiVersion;
            BaseVersionedRequestUrlFormat = $"{Domain}/{{1}}/{{0}}";
        }

        public string Domain { get; }

        public string ApiVersion { get; }

        /// <summary>
        /// String with interpolation for the endpoint name (0) and api version (1).
        /// </summary>
        private string BaseVersionedRequestUrlFormat { get; }

        public static ElevenLabsClientSettings Default { get; } = new();

        /// <summary>
        /// Build a according to this settings with the given endpoint and api version.
        /// </summary>
        /// <param name="endpoint">The endpoint to build the url for (required).</param>
        /// <param name="apiVersion">The version of the ElevenLabs api you want to use (optional).</param>
        /// <returns>A string representing the built URL.</returns>
        /// <exception cref="ArgumentException">Thrown when the endpoint is null or empty.</exception>
        public string BuildUrl(string endpoint, string apiVersion = null)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint cannot be null or empty.", nameof(endpoint));
            }
            if (string.IsNullOrWhiteSpace(apiVersion))
            {
                apiVersion = ApiVersion;
            }

            return string.Format(BaseVersionedRequestUrlFormat, endpoint, apiVersion);
        }
    }
}
