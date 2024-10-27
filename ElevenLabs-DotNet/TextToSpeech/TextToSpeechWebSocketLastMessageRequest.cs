// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElevenLabs.TextToSpeech;

public sealed class TextToSpeechWebSocketLastMessageRequest
{
    [JsonPropertyName("text"), JsonInclude]
    public string Text { get; } = "";
    
    public ArraySegment<byte> ToArraySegment()
    {
        string json = JsonSerializer.Serialize(this);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return new ArraySegment<byte>(bytes);
    }
}