// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace ElevenLabs
{
    public sealed class ElevenLabsClientSettings
    {
        internal const string ElevenLabsDomain = "";

        public ElevenLabsClientSettings()
        {
            Domain = ElevenLabsDomain;
            ApiVersion = "v1";
            BaseRequest = $"/{ApiVersion}/";
            BaseRequestUrlFormat = $"https://{Domain}{BaseRequest}{{0}}";
        }

        public string Domain { get; }

        public string ApiVersion { get; }

        public string BaseRequest { get; }

        public string BaseRequestUrlFormat { get; }

        public static ElevenLabsClientSettings Default { get; } = new ElevenLabsClientSettings();
    }
}