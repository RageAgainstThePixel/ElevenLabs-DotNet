// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace ElevenLabs
{
    public sealed class ElevenLabsClientSettings
    {
        internal const string Http = "http://";
        internal const string Https = "https://";
        internal const string ElevenLabsDomain = "api.elevenlabs.io";

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsClientSettings"/> for use with ElevenLabs API.
        /// </summary>
        public ElevenLabsClientSettings()
        {
            Domain = ElevenLabsDomain;
            BaseRequestUrlFormat = $"{Https}{Domain}/{{0}}/{{1}}";
        }

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsClientSettings"/> for use with ElevenLabs API.
        /// </summary>
        /// <param name="domain">Base api domain.</param>
        public ElevenLabsClientSettings(string domain)
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
            BaseRequestUrlFormat = $"{Domain}/{{0}}/{{1}}";
        }

        public string Domain { get; }

        internal string BaseRequestUrlFormat { get; }

        public static ElevenLabsClientSettings Default { get; } = new();
    }
}
