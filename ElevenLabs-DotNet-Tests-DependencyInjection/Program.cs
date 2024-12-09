using ElevenLabs.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace ElevenLabs.Tests.DependencyInjection
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            // Create the host builder
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            // Add the ElevenLabs client to the service collection
            builder.Services.AddElevenLabsClient(options =>
            {
                options.ApiKey = "YOUR_API_KEY";
            });

            //OR if you want to use the default configuration section from appsettings
            builder.Services.AddElevenLabsClient();

            //OR if you want to use a specific configuration section
            var options = new ElevenLabsClientOptions()
            {
                ApiKey = "YOUR_API_KEY",
                ApiVersion = "v1",
                Domain = "https://api.eleven-labs.com",
                HttpClient = new System.Net.Http.HttpClient()
            };
            builder.Services.AddElevenLabsClient(options);




            using IHost host = builder.Build();

            await host.RunAsync();
        }
    }
}