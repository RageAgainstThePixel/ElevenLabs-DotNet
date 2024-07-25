// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace ElevenLabs;

public sealed class ElevenLabsClientSettings
{
    internal const string HttpProtocol = "http://";
    internal const string HttpsProtocol = "https://";
    internal const string WsProtocol = "ws://";
    internal const string WssProtocol = "wss://";
    internal const string DefaultApiVersion = "v1";
    internal const string ElevenLabsDomain = "api.elevenlabs.io";

    /// <summary>
    ///     Creates a new instance of <see cref="ElevenLabsClientSettings" /> for use with ElevenLabs API.
    /// </summary>
    public ElevenLabsClientSettings()
    {
        Domain = ElevenLabsDomain;
        ApiVersion = DefaultApiVersion;
        Protocol = HttpsProtocol;
        WebSocketProtocol = WssProtocol;
        BaseRequest = $"/{ApiVersion}/";
        BaseRequestUrlFormat = $"{Protocol}{Domain}{BaseRequest}{{0}}";
        BaseRequestWebSocketUrlFormat = $"{WebSocketProtocol}{Domain}{BaseRequest}{{0}}";
    }

    /// <summary>
    ///     Creates a new instance of <see cref="ElevenLabsClientSettings" /> for use with ElevenLabs API.
    /// </summary>
    /// <param name="domain">Base api domain. Starts with https or wss.</param>
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
            throw new ArgumentException(
                $"You're attempting to pass a \"resourceName\" parameter to \"{nameof(domain)}\". Please specify \"resourceName:\" for this parameter in constructor.");
        }

        // extract anything before the :// to split the domain and protocol
        var splitDomain = domain.Split("://", StringSplitOptions.RemoveEmptyEntries);
        if (splitDomain.Length == 2)
        {
            Protocol = splitDomain[0];
            // if the protocol is not https or http, throw an exception
            if (Protocol != HttpsProtocol &&
                Protocol != HttpProtocol)
            {
                throw new ArgumentException(
                    $"The protocol \"{Protocol}\" is not supported. Please use \"{HttpsProtocol}\" or \"{HttpProtocol}\".");
            }

            WebSocketProtocol = Protocol == HttpsProtocol ? WssProtocol : WsProtocol;
            Domain = splitDomain[1];
        }
        else
        {
            Protocol = HttpsProtocol;
            WebSocketProtocol = WssProtocol;
            Domain = domain;
        }

        if (string.IsNullOrWhiteSpace(apiVersion))
        {
            apiVersion = DefaultApiVersion;
        }

        Domain = domain;
        ApiVersion = apiVersion;
        BaseRequest = $"/{ApiVersion}/";
        BaseRequestUrlFormat = $"{Protocol}{Domain}{BaseRequest}{{0}}";
        BaseRequestWebSocketUrlFormat = $"{WebSocketProtocol}{Domain}{BaseRequest}{{0}}";
    }

    public string Protocol { get; }

    public string WebSocketProtocol { get; }

    public string Domain { get; }

    public string ApiVersion { get; }

    public string BaseRequest { get; }

    public string BaseRequestUrlFormat { get; }

    public string BaseRequestWebSocketUrlFormat { get; }

    public static ElevenLabsClientSettings Default { get; } = new();
}