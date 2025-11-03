// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Dubbing;
using ElevenLabs.Extensions;
using ElevenLabs.History;
using ElevenLabs.Models;
using ElevenLabs.SoundGeneration;
using ElevenLabs.TextToSpeech;
using ElevenLabs.User;
using ElevenLabs.VoiceGeneration;
using ElevenLabs.Voices;
using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElevenLabs
{
    public sealed class ElevenLabsClient : IDisposable
    {
        /// <summary>
        /// Creates a new client for the Eleven Labs API, handling auth and allowing for access to various API endpoints.
        /// </summary>
        /// <param name="authentication">The API authentication information to use for API calls,
        /// or <see langword="null"/> to attempt to use the <see cref="ElevenLabsAuthentication.Default"/>,
        /// potentially loading from environment vars or from a config file.
        /// </param>
        /// <param name="settings">
        /// Optional, <see cref="ElevenLabsClientSettings"/> for specifying a proxy domain.
        /// </param>
        /// <param name="httpClient">Optional, <see cref="HttpClient"/>.</param>
        /// <exception cref="AuthenticationException">Raised when authentication details are missing or invalid.</exception>
        /// <see cref="ElevenLabsClient"/> implements <see cref="IDisposable"/> to manage the lifecycle of the resources it uses, including <see cref="HttpClient"/>.
        /// <remarks>
        /// When you initialize <see cref="ElevenLabsClient"/>, it will create an internal <see cref="HttpClient"/> instance if one is not provided.
        /// This internal HttpClient is disposed of when ElevenLabsClient is disposed of.
        /// If you provide an external HttpClient instance to ElevenLabsClient, you are responsible for managing its disposal.
        /// </remarks>
        public ElevenLabsClient(ElevenLabsAuthentication authentication = null, ElevenLabsClientSettings settings = null, HttpClient httpClient = null)
        {
            ElevenLabsAuthentication = authentication ?? ElevenLabsAuthentication.Default;
            Settings = settings ?? ElevenLabsClientSettings.Default;

            if (string.IsNullOrWhiteSpace(ElevenLabsAuthentication?.ApiKey))
            {
                throw new AuthenticationException("You must provide API authentication.  Please refer to https://github.com/RageAgainstThePixel/ElevenLabs-DotNet#authentication for details.");
            }

            Client = SetupHttpClient(httpClient);
            UserEndpoint = new UserEndpoint(this);
            VoicesEndpoint = new VoicesEndpoint(this);
            VoicesV2Endpoint = new VoicesV2Endpoint(this);
            SharedVoicesEndpoint = new SharedVoicesEndpoint(this);
            ModelsEndpoint = new ModelsEndpoint(this);
            HistoryEndpoint = new HistoryEndpoint(this);
            TextToSpeechEndpoint = new TextToSpeechEndpoint(this);
            VoiceGenerationEndpoint = new VoiceGenerationEndpoint(this);
            SoundGenerationEndpoint = new SoundGenerationEndpoint(this);
            DubbingEndpoint = new DubbingEndpoint(this);
        }

        ~ElevenLabsClient() => Dispose(false);

        #region IDisposable

        private bool isDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed && disposing)
            {
                if (!isCustomClient)
                {
                    Client?.Dispose();
                }

                isDisposed = true;
            }
        }

        #endregion IDisposable

        private bool isCustomClient;

        /// <summary>
        /// <see cref="HttpClient"/> to use when making calls to the API.
        /// </summary>
        internal HttpClient Client { get; }

        /// <summary>
        /// The <see cref="JsonSerializationOptions"/> to use when making calls to the API.
        /// </summary>
        internal static JsonSerializerOptions JsonSerializationOptions { get; } = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverterFactory(),
            },
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        /// <summary>
        /// The API authentication information to use for API calls
        /// </summary>
        public ElevenLabsAuthentication ElevenLabsAuthentication { get; }

        internal ElevenLabsClientSettings Settings { get; }

        /// <summary>
        /// Enables or disables debugging for all endpoints.
        /// </summary>
        public bool EnableDebug { get; set; }

        #region Endpoints

        public UserEndpoint UserEndpoint { get; }

        public VoicesEndpoint VoicesEndpoint { get; }

        public VoicesV2Endpoint VoicesV2Endpoint { get; }

        public SharedVoicesEndpoint SharedVoicesEndpoint { get; }

        public ModelsEndpoint ModelsEndpoint { get; }

        public HistoryEndpoint HistoryEndpoint { get; }

        public TextToSpeechEndpoint TextToSpeechEndpoint { get; }

        public VoiceGenerationEndpoint VoiceGenerationEndpoint { get; }

        public SoundGenerationEndpoint SoundGenerationEndpoint { get; }

        public DubbingEndpoint DubbingEndpoint { get; }

        #endregion Endpoints

        private HttpClient SetupHttpClient(HttpClient client = null)
        {
            if (client == null)
            {
                client = new HttpClient(new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(15)
                });
            }
            else
            {
                isCustomClient = true;
            }

            client.DefaultRequestHeaders.Add("User-Agent", "ElevenLabs-DotNet");
            client.DefaultRequestHeaders.Add("xi-api-key", ElevenLabsAuthentication.ApiKey);
            return client;
        }
    }
}
