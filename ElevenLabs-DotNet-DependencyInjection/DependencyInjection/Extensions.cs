using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;

#nullable enable

namespace ElevenLabs.DependencyInjection
{

    /// <summary>
    /// Extensions to add the ElevenLabs client to the service collection.
    /// </summary>
    public static class Extensions
    {
        private const string ElevenlabsHttpClientName = "ElevenLabsClient";

        /// <summary>
        /// Add the ElevenLabs client to the service collection with the specified API key.
        /// </summary>
        /// <param name="services">The service collection to add the ElevenLabs client to.</param>
        /// <param name="apiKey">The API key to use for the ElevenLabs client.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddElevenLabsClient(this IServiceCollection services, string apiKey) =>
             AddElevenLabsClient(services, new ElevenLabsClientOptions() { ApiKey = apiKey });

        /// <summary>
        /// Add the ElevenLabs client to the service collection.
        /// The ElevenLabs options are configured using the "ElevenLabs" section.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddElevenLabsClient(this IServiceCollection services)
        {
            var optionsBuilder = services.AddOptions<ElevenLabsClientOptions>();
            optionsBuilder.BindConfiguration("Elevenlabs");
            ValidateElevenLabsOptions(optionsBuilder);
            AddElevenLabsServices(services);

            return services;
        }

        /// <summary>
        /// Add the ElevenLabs client to the service collection with the specified configuration section.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="namedConfigurationSection">The section where the ElevenLabs options are configured.</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddElevenLabsClient(
            this IServiceCollection services,
            IConfiguration namedConfigurationSection
        )
        {
            OptionsBuilder<ElevenLabsClientOptions> optionsBuilder = services.AddOptions<ElevenLabsClientOptions>();
            optionsBuilder.Bind(namedConfigurationSection);
            ValidateElevenLabsOptions(optionsBuilder);
            AddElevenLabsServices(services);
            return services;
        }

        /// <summary>
        /// Add the ElevenLabs client to the service collection with the specified configurations.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureOptions">Configure the client options using this callback.</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddElevenLabsClient(
            this IServiceCollection services,
            Action<ElevenLabsClientOptions> configureOptions
        )
            => services.AddElevenLabsClient((_, options) => configureOptions(options));

        /// <inheritdoc cref="AddElevenLabsClient(IServiceCollection,Action{ClientOptions})"/>
        public static IServiceCollection AddElevenLabsClient(
            this IServiceCollection services,
            Action<IServiceProvider, ElevenLabsClientOptions> configureOptions
        )
        {
            var optionsBuilder = services.AddOptions<ElevenLabsClientOptions>();
            optionsBuilder.Configure<IServiceProvider>((options, provider) => configureOptions(provider, options));
            ValidateElevenLabsOptions(optionsBuilder);
            AddElevenLabsServices(services);

            return services;
        }

        /// <summary>
        /// Add the ElevenLabs client to the service collection with ElevenLabsClientOptions.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="options">The ElevenLabs client options</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddElevenLabsClient(this IServiceCollection services, ElevenLabsClientOptions options)
        {
            OptionsBuilder<ElevenLabsClientOptions> optionsBuilder = services.AddOptions<ElevenLabsClientOptions>();
            optionsBuilder.Configure(delegate (ElevenLabsClientOptions optionsToConfigure, IServiceProvider _)
            {
                optionsToConfigure.ApiKey = options.ApiKey;
                optionsToConfigure.HttpClient = options.HttpClient;
                optionsToConfigure.ApiVersion = options.ApiVersion;
                optionsToConfigure.Domain = options.Domain;
            });
            ValidateElevenLabsOptions(optionsBuilder);
            AddElevenLabsServices(services);
            return services;
        }

        private static void AddElevenLabsServices(IServiceCollection services)
        {
            services.AddHttpClient(ElevenlabsHttpClientName);
            services.AddTransient(CreateElevenlabsClient);
        }

        private static ElevenLabsClient CreateElevenlabsClient(IServiceProvider provider)
        {
            var options = provider.GetRequiredService<IOptionsSnapshot<ElevenLabsClientOptions>>().Value;
            options.HttpClient ??= provider.GetRequiredService<IHttpClientFactory>().CreateClient(ElevenlabsHttpClientName);
            return new ElevenLabsClient(options.ApiKey, new ElevenLabsClientSettings(options.Domain, options.ApiVersion), options.HttpClient);
        }

        /// <summary>
        /// Validates the <see cref="ElevenLabsClientOptions"/> to ensure that the ApiKey is not null or empty.
        /// </summary>
        /// <param name="optionsBuilder">The <see cref="OptionsBuilder{TOptions}"/> used to configure the <see cref="ElevenLabsClientOptions"/>.</param>
        /// <exception cref="OptionsValidationException">Thrown if the ApiKey is null or empty.</exception>
        private static void ValidateElevenLabsOptions(OptionsBuilder<ElevenLabsClientOptions> optionsBuilder)
        {
            optionsBuilder.Validate(options => !string.IsNullOrEmpty(options.ApiKey),
                "ElevenLabs:ApiKey is required."
            );
        }
    }

    /// <summary>
    /// Represents the options for configuring the ElevenLabs client.
    /// </summary>
    public partial class ElevenLabsClientOptions
    {
      /// <summary>
      /// Gets or sets the API key required for authentication.
      /// </summary>
      public required string ApiKey { get; set; }

      /// <summary>
      /// Gets or sets the optional HttpClient instance to be used for making requests.
      /// </summary>
      public HttpClient? HttpClient { get; set; }

      /// <summary>
      /// Gets or sets the optional domain for the API.
      /// </summary>
      public string? Domain { get; set; }

      /// <summary>
      /// Gets or sets the optional API version.
      /// </summary>
      public string? ApiVersion { get; set; }
    }
}
