using ElevenLabs.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace ElevenLabs.Tests.DependencyInjection
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            // Create the host builder
            var builder = WebApplication.CreateBuilder(args);

            // Add the ElevenLabs client to the service collection
            builder.Services.AddElevenLabsClient(options =>
            {
                options.ApiKey = "YOUR_API_KEY";
            });

            //OR if you want to use the default configuration section from appsettings
            //builder.Services.AddElevenLabsClient();

            //OR if you want to use a specific configuration section
            builder.Services.AddElevenLabsClient(builder.Configuration.GetSection("YourCustomSection"));

            //OR if you want to use a specific configurations
            //var options = new ElevenLabsClientOptions()
            //{
            //    ApiKey = "YOUR_API_KEY",
            //    ApiVersion = "v1",
            //    Domain = "https://api.eleven-labs.com",
            //    HttpClient = new System.Net.Http.HttpClient()
            //};
            //builder.Services.AddElevenLabsClient(options);

            var app = builder.Build();
            app.Run();
        }
    }
}