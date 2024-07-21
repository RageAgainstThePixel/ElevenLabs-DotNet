// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ElevenLabs.Proxy
{
    [Obsolete("Use ElevenLabsProxy")]
    public class ElevenLabsProxyStartup
    {
        public void ConfigureServices(IServiceCollection services) { }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) { }

        public static IHost CreateDefaultHost<T>(string[] args, ElevenLabsClient elevenLabsClient)
            where T : class, IAuthenticationFilter => null;

        public static WebApplication CreateWebApplication<T>(string[] args, ElevenLabsClient elevenLabsClient)
            where T : class, IAuthenticationFilter => null;
    }
}