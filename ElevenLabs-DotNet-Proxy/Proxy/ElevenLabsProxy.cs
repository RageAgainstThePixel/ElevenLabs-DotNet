// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace ElevenLabs.Proxy
{
    /// <summary>
    /// Used in ASP.NET Core WebApps to start your own ElevenLabs web api proxy.
    /// </summary>
    public class ElevenLabsProxy
    {
        private ElevenLabsClient elevenLabsClient;
        private IAuthenticationFilter authenticationFilter;

        /// <summary>
        /// Configures the <see cref="ElevenLabsClient"/> and <see cref="IAuthenticationFilter"/> services.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
            => SetupServices(services.BuildServiceProvider());

        /// <summary>
        /// Configures the <see cref="IApplicationBuilder"/> to handle requests and forward them to OpenAI API.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/>.</param>
        /// <param name="env"><see cref="IWebHostEnvironment"/>.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            SetupServices(app.ApplicationServices);

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/health", HealthEndpoint);
                endpoints.MapElevenLabsEndpoints(elevenLabsClient, authenticationFilter);
            });
        }

        /// <summary>
        /// Creates a new <see cref="IHost"/> that acts as a proxy web api for ElevenLabs.
        /// </summary>
        /// <typeparam name="T"><see cref="IAuthenticationFilter"/> type to use to validate your custom issued tokens.</typeparam>
        /// <param name="args">Startup args.</param>
        /// <param name="elevenLabsClient"><see cref="ElevenLabsClient"/> with configured <see cref="ElevenLabsAuthentication"/> and <see cref="ElevenLabsClientSettings"/>.</param>
        public static IHost CreateDefaultHost<T>(string[] args, ElevenLabsClient elevenLabsClient) where T : class, IAuthenticationFilter
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<ElevenLabsProxy>();
                    webBuilder.ConfigureKestrel(ConfigureKestrel);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(elevenLabsClient);
                    services.AddSingleton<IAuthenticationFilter, T>();
                }).Build();

        /// <summary>
        /// Creates a new <see cref="WebApplication"/> that acts as a proxy web api for OpenAI.
        /// </summary>
        /// <typeparam name="T"><see cref="IAuthenticationFilter"/> type to use to validate your custom issued tokens.</typeparam>
        /// <param name="args">Startup args.</param>
        /// <param name="elevenLabsClient"><see cref="ElevenLabsClient"/> with configured <see cref="ElevenLabsAuthentication"/> and <see cref="ElevenLabsClientSettings"/>.</param>
        public static WebApplication CreateWebApplication<T>(string[] args, ElevenLabsClient elevenLabsClient) where T : class, IAuthenticationFilter
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.ConfigureKestrel(ConfigureKestrel);
            builder.Services.AddSingleton(elevenLabsClient);
            builder.Services.AddSingleton<IAuthenticationFilter, T>();
            var app = builder.Build();
            var startup = new ElevenLabsProxy();
            startup.Configure(app, app.Environment);
            return app;
        }

        private static void ConfigureKestrel(KestrelServerOptions options)
        {
            options.AllowSynchronousIO = false;
            options.Limits.MinRequestBodyDataRate = null;
            options.Limits.MinResponseDataRate = null;
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
        }

        private void SetupServices(IServiceProvider serviceProvider)
        {
            elevenLabsClient = serviceProvider.GetRequiredService<ElevenLabsClient>();
            authenticationFilter = serviceProvider.GetRequiredService<IAuthenticationFilter>();
        }

        private static async Task HealthEndpoint(HttpContext context)
        {
            // Respond with a 200 OK status code and a plain text message
            context.Response.StatusCode = StatusCodes.Status200OK;
            const string contentType = "text/plain";
            context.Response.ContentType = contentType;
            const string content = "OK";
            await context.Response.WriteAsync(content);
        }
    }
}
