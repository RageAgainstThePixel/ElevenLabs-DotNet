// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using System.Text.Json;
using ElevenLabs.Voices;
using System.Text.Json.Serialization;

namespace ElevenLabs.TextToSpeech;

public sealed class TextToSpeechWebSocketFirstMessageRequest
{
    public TextToSpeechWebSocketFirstMessageRequest(
        VoiceSettings voiceSettings = null,
        GenerationConfig generationConfig = null)
    {
        VoiceSettings = voiceSettings;
        GenerationConfig = generationConfig;
    }

    [JsonPropertyName("text"), JsonInclude]
    public string Text { get; } = " ";

    [JsonPropertyName("voice_settings")]
    public VoiceSettings VoiceSettings { get; }

    [JsonPropertyName("generation_config")]
    public GenerationConfig GenerationConfig { get; }
    
    public ArraySegment<byte> ToArraySegment()
    {
        string json = JsonSerializer.Serialize(this);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return new ArraySegment<byte>(bytes);
    }
}