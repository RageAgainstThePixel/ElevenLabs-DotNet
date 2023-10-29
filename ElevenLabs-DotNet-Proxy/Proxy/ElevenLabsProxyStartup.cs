// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        private static readonly HashSet<string> ExcludedHeaders = new HashSet<string>()
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

        public void ConfigureServices(IServiceCollection services) { }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            elevenLabsClient = app.ApplicationServices.GetRequiredService<ElevenLabsClient>();
            authenticationFilter = app.ApplicationServices.GetRequiredService<IAuthenticationFilter>();

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
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<ElevenLabsProxyStartup>();
                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.AllowSynchronousIO = false;
                        options.Limits.MinRequestBodyDataRate = null;
                        options.Limits.MinResponseDataRate = null;
                        options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                        options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(elevenLabsClient);
                    services.AddSingleton<IAuthenticationFilter, T>();
                }).Build();
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
                authenticationFilter.ValidateAuthentication(httpContext.Request.Headers);

                var method = new HttpMethod(httpContext.Request.Method);
                var uri = new Uri(string.Format(elevenLabsClient.ElevenLabsClientSettings.BaseRequestUrlFormat, $"{endpoint}{httpContext.Request.QueryString}"));
                var elevenLabsRequest = new HttpRequestMessage(method, uri);

                elevenLabsRequest.Content = new StreamContent(httpContext.Request.Body);

                if (httpContext.Request.ContentType != null)
                {
                    elevenLabsRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(httpContext.Request.ContentType);
                }

                var proxyResponse = await elevenLabsClient.Client.SendAsync(elevenLabsRequest, HttpCompletionOption.ResponseHeadersRead);
                httpContext.Response.StatusCode = (int)proxyResponse.StatusCode;

                foreach (var (key, value) in proxyResponse.Headers)
                {
                    if (ExcludedHeaders.Contains(key)) { continue; }
                    httpContext.Response.Headers[key] = value.ToArray();
                }

                foreach (var (key, value) in proxyResponse.Content.Headers)
                {
                    if (ExcludedHeaders.Contains(key)) { continue; }
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
