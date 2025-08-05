// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;

namespace ElevenLabs.Proxy
{
    public static class EndpointRouteBuilder
    {
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
            "Set-Cookie",
            HeaderNames.TE,
#if NET
            HeaderNames.AltSvc,
#else
            "Alt-Svc",
#endif
        };

        /// <summary>
        /// Maps the <see cref="ElevenLabsClient"/> endpoints.
        /// </summary>
        /// <param name="endpoints"><see cref="IEndpointRouteBuilder"/>.</param>
        /// <param name="client"><see cref="ElevenLabsClient"/>.</param>
        /// <param name="authenticationFilter"><see cref="IAuthenticationFilter"/>.</param>
        /// <param name="routePrefix">Optional, custom route prefix. i.e. '/elevenlabs'.</param>
        public static void MapElevenLabsEndpoints(this IEndpointRouteBuilder endpoints, ElevenLabsClient client, IAuthenticationFilter authenticationFilter, string routePrefix = "")
        {
            endpoints.Map($"{routePrefix}/{{version}}/{{**endpoint}}", HandleRequest);
            return;

            async Task HandleRequest(HttpContext httpContext, string version, string endpoint)
            {
                try
                {
                    await authenticationFilter.ValidateAuthenticationAsync(httpContext.Request.Headers).ConfigureAwait(false);
                    var method = new HttpMethod(httpContext.Request.Method);
                    var originalQuery = QueryHelpers.ParseQuery(httpContext.Request.QueryString.Value ?? "");
                    var modifiedQuery = new Dictionary<string, string>(originalQuery.Count);

                    foreach (var pair in originalQuery)
                    {
                        modifiedQuery[pair.Key] = pair.Value.FirstOrDefault();
                    }

                    var baseUri = client.Settings.BuildUrl(endpoint, apiVersion: version);
                    var uri = new Uri(QueryHelpers.AddQueryString(baseUri, modifiedQuery));

                    using var request = new HttpRequestMessage(method, uri);
                    request.Content = new StreamContent(httpContext.Request.Body);

                    if (httpContext.Request.ContentType != null)
                    {
                        request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(httpContext.Request.ContentType);
                    }

                    var proxyResponse = await client.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, httpContext.RequestAborted).ConfigureAwait(false);
                    httpContext.Response.StatusCode = (int)proxyResponse.StatusCode;
                    httpContext.Response.ContentLength = proxyResponse.Content.Headers.ContentLength;
                    httpContext.Response.ContentType = proxyResponse.Content.Headers.ContentType?.ToString();

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

                    const string streamingContent = "text/event-stream";

                    if (httpContext.Response.ContentType != null &&
                        httpContext.Response.ContentType.Equals(streamingContent, StringComparison.OrdinalIgnoreCase))
                    {
                        var stream = await proxyResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        await WriteServerStreamEventsAsync(httpContext, stream).ConfigureAwait(false);
                    }
                    else
                    {
                        await proxyResponse.Content.CopyToAsync(httpContext.Response.Body, httpContext.RequestAborted).ConfigureAwait(false);
                    }
                }
                catch (AuthenticationException authenticationException)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await httpContext.Response.WriteAsync(authenticationException.Message).ConfigureAwait(false);
                }
                catch (WebSocketException)
                {
                    // ignore
                    throw;
                }
                catch (Exception e)
                {
                    if (httpContext.Response.HasStarted) { throw; }
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    var response = JsonSerializer.Serialize(new { error = new { e.Message, e.StackTrace } });
                    await httpContext.Response.WriteAsync(response).ConfigureAwait(false);
                }

                static async Task WriteServerStreamEventsAsync(HttpContext httpContext, Stream contentStream)
                {
                    var responseStream = httpContext.Response.Body;
                    await contentStream.CopyToAsync(responseStream, httpContext.RequestAborted).ConfigureAwait(false);
                    await responseStream.FlushAsync(httpContext.RequestAborted).ConfigureAwait(false);
                }
            }
        }
    }
}
