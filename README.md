# ElevenLabs-DotNet

[![Discord](https://img.shields.io/discord/855294214065487932.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/xQgMW9ufN4)
[![NuGet version (ElevenLabs-DotNet)](https://img.shields.io/nuget/v/ElevenLabs-DotNet.svg)](https://www.nuget.org/packages/ElevenLabs-DotNet/)
[![Nuget Publish](https://github.com/RageAgainstThePixel/ElevenLabs-DotNet/actions/workflows/Publish-Nuget.yml/badge.svg)](https://github.com/RageAgainstThePixel/ElevenLabs-DotNet/actions/workflows/Publish-Nuget.yml)

A non-official Eleven Labs voice synthesis RESTful client.

I am not affiliated with Eleven Labs and an account with api access is required.

***All copyrights, trademarks, logos, and assets are the property of their respective owners.***

## Getting started

### Install from NuGet

Install package [`OpenAI` from Nuget](https://www.nuget.org/packages/ElevenLabs-DotNet/).  Here's how via command line:

```powershell
Install-Package ElevenLabs-DotNet
```

> Looking to [use OpenAI in the Unity Game Engine](https://github.com/RageAgainstThePixel/com.rest.elevenlabs)? Check out our unity package on OpenUPM:
>
>[![openupm](https://img.shields.io/npm/v/com.rest.elevenlabs?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.rest.elevenlabs/)

---

## Documentation

### Table of Contents

- [Text to Speech](#text-to-speech)
- [Voices](#voices)
  - [Samples](#samples)
- [History](#history)
- [User](#user)

### [Text to Speech](https://api.elevenlabs.io/docs#/text-to-speech)

Convert text to speech.

```csharp
var api = new ElevenLabsClient();
var text = "The quick brown fox jumps over the lazy dog.";
var voice = (await api.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
var (clipPath, audioClip) = await api.TextToSpeechEndpoint.TextToSpeechAsync(text, voice, defaultVoiceSettings);
Debug.Log(clipPath);
```

### [Voices](https://api.elevenlabs.io/docs#/voices)

Access to voices created either by the user or eleven labs.

#### [Samples](https://api.elevenlabs.io/docs#/samples)

Access to your samples, created by you when cloning voices.

### [History](https://api.elevenlabs.io/docs#/history)

Access to your previously synthesized audio clips including its metadata.

### [User](https://api.elevenlabs.io/docs#/user)

Access to your user Information and subscription status.
