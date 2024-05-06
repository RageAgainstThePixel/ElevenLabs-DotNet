// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace ElevenLabs.Proxy
{
    /// <summary>
    /// Used in ASP.NET Core WebApps to start your own ElevenLabs web api proxy.
    /// </summary>
    public class ElevenLabsProxyStartup
    {
        private ElevenLabsClient elevenLabsClient;
        private IAuthenticationFilter authenticationFilter;

        // Copied from https://github.com/microsoft/reverse-proxy/blob/51d797986b1fea03500a1ad173d13a1176fb5552/src/ReverseProxy/Forwarder/RequestUtilities.cs#L61-L83
        private static readonly HashSet<string> excludedHeaders = new()
        {
            HeaderNames.Connection,
            HeaderNames.TransferEncoding,
            HeaderNames.KeepAlive,
            HeaderNames.Upgrade,
            "Proxy-Connection",
            "Proxy-Authenticate",
            "Proxy-Authentication-Info",
            "Proxy-Authorization",
            "Proxy-Features",
            "Proxy-Instruction",
            "Security-Scheme",
            "ALPN",
            "Close",
            HeaderNames.TE,
#if NET
            HeaderNames.AltSvc,
#else
            "Alt-Svc",
#endif
        };

        /// <summary>
        /// Configures the <see cref="elevenLabsClient"/> and <see cref="IAuthenticationFilter"/> services.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
            => SetupServices(services.BuildServiceProvider());

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
                endpoints.Map($"{elevenLabsClient.ElevenLabsClientSettings.BaseRequest}{{**endpoint}}", HandleRequest);
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
                    webBuilder.UseStartup<ElevenLabsProxyStartup>();
                    webBuilder.ConfigureKestrel(ConfigureKestrel);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(elevenLabsClient);
                    services.AddSingleton<IAuthenticationFilter, T>();
                }).Build();

        public static WebApplication CreateWebApplication<T>(string[] args, ElevenLabsClient elevenLabsClient) where T : class, IAuthenticationFilter
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.ConfigureKestrel(ConfigureKestrel);
            builder.Services.AddSingleton(elevenLabsClient);
            builder.Services.AddSingleton<IAuthenticationFilter, T>();
            var app = builder.Build();
            var startup = new ElevenLabsProxyStartup();
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

        /// <summary>
        /// Handles incoming requests, validates authentication, and forwards the request to ElevenLabs API
        /// </summary>
        private async Task HandleRequest(HttpContext httpContext, string endpoint)
        {
            try
            {
                // ReSharper disable once MethodHasAsyncOverload
                // just in case either method is implemented we call it twice.
                authenticationFilter.ValidateAuthentication(httpContext.Request.Headers);
                await authenticationFilter.ValidateAuthenticationAsync(httpContext.Request.Headers);

                var method = new HttpMethod(httpContext.Request.Method);
                var uri = new Uri(string.Format(elevenLabsClient.ElevenLabsClientSettings.BaseRequestUrlFormat, $"{endpoint}{httpContext.Request.QueryString}"));
                using var request = new HttpRequestMessage(method, uri);

                request.Content = new StreamContent(httpContext.Request.Body);

                if (httpContext.Request.ContentType != null)
                {
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(httpContext.Request.ContentType);
                }

                var proxyResponse = await elevenLabsClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                httpContext.Response.StatusCode = (int)proxyResponse.StatusCode;

                foreach (var (key, value) in proxyResponse.Headers)
                {
                    if (excludedHeaders.Contains(key)) { continue; }
                    httpContext.Response.Headers[key] = value.ToArray();
                }

                foreach (var (key, value) in proxyResponse.Content.Headers)
                {
                    if (excludedHeaders.Contains(key)) { continue; }
                    httpContext.Response.Headers[key] = value.ToArray();
                }

                httpContext.Response.ContentType = proxyResponse.Content.Headers.ContentType?.ToString() ?? string.Empty;
                const string streamingContent = "text/event-stream";

                if (httpContext.Response.ContentType.Equals(streamingContent))
                {
                    var stream = await proxyResponse.Content.ReadAsStreamAsync();
                    await WriteServerStreamEventsAsync(httpContext, stream);
                }
                else
                {
                    await proxyResponse.Content.CopyToAsync(httpContext.Response.Body);
                }
            }
            catch (AuthenticationException authenticationException)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync(authenticationException.Message);
            }
            catch (Exception e)
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsync(e.Message);
            }
        }

        private static async Task WriteServerStreamEventsAsync(HttpContext httpContext, Stream contentStream)
        {
            var responseStream = httpContext.Response.Body;
            await contentStream.CopyToAsync(responseStream, httpContext.RequestAborted);
            await responseStream.FlushAsync(httpContext.RequestAborted);
        }
    }
}
