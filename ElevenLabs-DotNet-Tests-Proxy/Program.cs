// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Proxy;
using Microsoft.AspNetCore.Http;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace ElevenLabs.Tests.Proxy
{
    /// <summary>
    /// Example Web App Proxy API.
    /// </summary>
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class Program
    {
        private const string TestUserToken = "aAbBcCdDeE123456789";

        // ReSharper disable once ClassNeverInstantiated.Local
        private class AuthenticationFilter : AbstractAuthenticationFilter
        {
            public override async Task ValidateAuthenticationAsync(IHeaderDictionary request)
            {
                await Task.CompletedTask; // remote resource call

                // You will need to implement your own class to properly test
                // custom issued tokens you've setup for your end users.
                if (!request["xi-api-key"].ToString().Contains(TestUserToken))
                {
                    throw new AuthenticationException("User is not authorized");
                }
            }
        }

        public static void Main(string[] args)
        {
            var auth = ElevenLabsAuthentication.LoadFromEnvironment();
            var client = new ElevenLabsClient(auth);
            ElevenLabsProxy.CreateWebApplication<AuthenticationFilter>(args, client).Run();
        }
    }
}