# ElevenLabs-DotNet-DependencyInjection
This project provides dependency injection extensions for the ElevenLabs client in .NET applications. It allows you to easily add and configure the ElevenLabs client within your service collection using various methods, such as providing an API key directly, using configuration sections, or specifying custom options.

Key features include:
- Adding the ElevenLabs client to the service collection with an API key.
- Configuring the ElevenLabs client using a configuration section.
- Customizing the ElevenLabs client options through callbacks.
- Ensuring the API key is validated and required for the client to function.

The project simplifies integrating the ElevenLabs client into your .NET applications by leveraging the dependency injection framework.

## Getting started

### Install from NuGet
To install the ElevenLabs-DotNet-DependencyInjection package from NuGet, run the following command in the Package Manager Console:
```sh
Install-Package ElevenLabs.DotNet.DependencyInjection
```

## Documentation

### Example Usage
In your `Program.cs` or `Startup.cs`, you can then add the services to the service collection:

```csharp
// Program.cs or Startup.cs
var services = new ServiceCollection();
services.AddElevenLabsServices();
var serviceProvider = services.BuildServiceProvider();
```

### Running Tests
To see an example of how dependency injection works with the ElevenLabs-DotNet-DependencyInjection project, navigate to the `ElevenLabs-DotNet-Tests-DependencyInjection` project directory and run the console application using the following command:

```sh
dotnet run
```

This will execute the console application and demonstrate how to use the dependency injection features provided by the ElevenLabs-DotNet-DependencyInjection package.
