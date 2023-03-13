# ElevenLabs-DotNet

[![Discord](https://img.shields.io/discord/855294214065487932.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/xQgMW9ufN4)
[![NuGet version (ElevenLabs-DotNet)](https://img.shields.io/nuget/v/ElevenLabs-DotNet.svg)](https://www.nuget.org/packages/ElevenLabs-DotNet/)
[![Nuget Publish](https://github.com/RageAgainstThePixel/ElevenLabs-DotNet/actions/workflows/Publish-Nuget.yml/badge.svg)](https://github.com/RageAgainstThePixel/ElevenLabs-DotNet/actions/workflows/Publish-Nuget.yml)

A non-official [Eleven Labs](https://elevenlabs.io/) voice synthesis RESTful client.

I am not affiliated with Eleven Labs and an account with api access is required.

***All copyrights, trademarks, logos, and assets are the property of their respective owners.***

## Getting started

### Install from NuGet

Install package [`ElevenLabs` from Nuget](https://www.nuget.org/packages/ElevenLabs-DotNet/).  Here's how via command line:

```powershell
Install-Package ElevenLabs-DotNet
```

> Looking to [use ElevenLabs in the Unity Game Engine](https://github.com/RageAgainstThePixel/com.rest.elevenlabs)? Check out our unity package on OpenUPM:
>
>[![openupm](https://img.shields.io/npm/v/com.rest.elevenlabs?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.rest.elevenlabs/)

---

## Documentation

### Table of Contents

- [Text to Speech](#text-to-speech)
- [Voices](#voices)
  - [Get All Voices](#get-all-voices)
  - [Get Default Voice Settings](#get-default-voice-settings)
  - [Get Voice](#get-voice)
  - [Edit Voice Settings](#edit-voice-settings)
  - [Add Voice](#add-voice)
  - [Edit Voice](#edit-voice)
  - [Delete Voice](#delete-voice)
  - [Samples](#samples)
    - [Get Voice Sample](#get-voice-sample)
    - [Delete Voice Sample](#delete-voice-sample)
- [History](#history)
  - [Get History](#get-history)
  - [Get History Audio](#get-history-audio)
  - [Download All History](#download-all-history)
  - [Delete History Item](#delete-history-item)
- [User](#user)
  - [Get User Info](#get-user-info)
  - [Get Subscription Info](#get-subscription-info)

### [Text to Speech](https://api.elevenlabs.io/docs#/text-to-speech)

Convert text to speech.

```csharp
var api = new ElevenLabsClient();
var text = "The quick brown fox jumps over the lazy dog.";
var voice = (await api.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
var clipPath = await api.TextToSpeechEndpoint.TextToSpeechAsync(text, voice, defaultVoiceSettings);
Console.WriteLine(clipPath);
```

### [Voices](https://api.elevenlabs.io/docs#/voices)

Access to voices created either by the user or ElevenLabs.

#### Get All Voices

Gets a list of all available voices.

```csharp
var api = new ElevenLabsClient();
var allVoices = await api.VoicesEndpoint.GetAllVoicesAsync();

foreach (var voice in allVoices)
{
    Console.WriteLine($"{voice.Id} | {voice.Name} | similarity boost: {voice.Settings?.SimilarityBoost} | stability: {voice.Settings?.Stability}");
}
```

#### Get Default Voice Settings

Gets the global default voice settings.

```csharp
var api = new ElevenLabsClient();
var result = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
Console.WriteLine($"stability: {result.Stability} | similarity boost: {result.SimilarityBoost}");
```

#### Get Voice

```csharp
var api = new ElevenLabsClient();
var voice = await api.VoicesEndpoint.GetVoiceAsync("voiceId");
Console.WriteLine($"{voice.Id} | {voice.Name} | {voice.PreviewUrl}");
```

#### Edit Voice Settings

Edit the settings for a specific voice.

```csharp
var api = new ElevenLabsClient();
var success = await api.VoicesEndpoint.EditVoiceSettingsAsync(voice, new VoiceSettings(0.7f, 0.7f));
Console.WriteLine($"Was successful? {success}");
```

#### Add Voice

```csharp
var api = new ElevenLabsClient();
var labels = new Dictionary<string, string>
{
    { "accent", "american" }
};
var audioSamplePaths = new List<string>();
var voice = await api.VoicesEndpoint.AddVoiceAsync("Voice Name", audioSamplePaths, labels);
```

#### Edit Voice

```csharp
var api = new ElevenLabsClient();
var labels = new Dictionary<string, string>
{
    { "age", "young" }
};
var audioSamplePaths = new List<string>();
var success = await api.VoicesEndpoint.EditVoiceAsync(voice, audioSamplePaths, labels);
Console.WriteLine($"Was successful? {success}");
```

#### Delete Voice

```csharp
var api = new ElevenLabsClient();
var success = await api.VoicesEndpoint.DeleteVoiceAsync(voiceId);
Console.WriteLine($"Was successful? {success}");
```

#### [Samples](https://api.elevenlabs.io/docs#/samples)

Access to your samples, created by you when cloning voices.

##### Get Voice Sample

```csharp
var api = new ElevenLabsClient();
var clipPath = await api.VoicesEndpoint.GetVoiceSampleAsync(voiceId, sampleId);
Console.WriteLine(clipPath);
```

##### Delete Voice Sample

```csharp
var api = new ElevenLabsClient();
var success = await api.VoicesEndpoint.DeleteVoiceSampleAsync(voiceId, sampleId);
Console.WriteLine($"Was successful? {success}");
```

### [History](https://api.elevenlabs.io/docs#/history)

Access to your previously synthesized audio clips including its metadata.

#### Get History

```csharp
var api = new ElevenLabsClient();
var historyItems = await api.HistoryEndpoint.GetHistoryAsync();

foreach (var historyItem in historyItems.OrderBy(historyItem => historyItem.Date))
{
    Console.WriteLine($"{historyItem.State} {historyItem.Date} | {historyItem.Id} | {historyItem.Text.Length} | {historyItem.Text}");
}
```

#### Get History Audio

```csharp
var api = new ElevenLabsClient();
var clipPath = await api.HistoryEndpoint.GetHistoryAudioAsync(historyItem);
Console.WriteLine(clipPath);
```

#### Download All History

```csharp
var api = new ElevenLabsClient();
var success = await api.HistoryEndpoint.DownloadHistoryItemsAsync();
```

#### Delete History Item

```csharp
var api = new ElevenLabsClient();
var result = await api.HistoryEndpoint.DeleteHistoryItemAsync(historyItem);
Console.WriteLine($"Was successful? {success}");
```

### [User](https://api.elevenlabs.io/docs#/user)

Access to your user Information and subscription status.

#### Get User Info

Gets information about your user account with ElevenLabs.

```csharp
var api = new ElevenLabsClient();
var userInfo = await api.UserEndpoint.GetUserInfoAsync();
```

#### Get Subscription Info

Gets information about your subscription with ElevenLabs.

```csharp
var api = new ElevenLabsClient();
var subscriptionInfo = await api.UserEndpoint.GetSubscriptionInfoAsync();
```
