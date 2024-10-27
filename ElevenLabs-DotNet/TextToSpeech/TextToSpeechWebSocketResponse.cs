// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text.Json.Serialization;

namespace ElevenLabs.TextToSpeech;

public sealed class TextToSpeechWebSocketResponse
{
    /// <summary>
    ///     A generated partial audio chunk, encoded using the selected output_format,
    ///     by default this is MP3 encoded as a base64 string.
    /// </summary>
    [JsonPropertyName("audio")]
    public string Audio { get; }

    /// <summary>
    ///     A generated partial audio chunk, encoded using the selected output_format,
    /// </summary>
    [JsonIgnore]
    public byte[] AudioBytes { get; }

    /// <summary>
    ///     Indicates if the generation is complete. If set to True, audio will be null.
    /// </summary>
    [JsonPropertyName("isFinal")]
    public bool? IsFinal { get; }

    /// <summary>
    ///     Alignment information for the generated audio given the input normalized text sequence.
    /// </summary>
    [JsonPropertyName("normalizedAlignment")]
    public Alignment NormalizedAlignment { get; }

    /// <summary>
    ///     Alignment information for the generated audio given the original text sequence.
    /// </summary>
    [JsonPropertyName("alignment")]
    public Alignment Alignment { get; }

    public TextToSpeechWebSocketResponse(string audio, bool? isFinal, Alignment normalizedAlignment, Alignment alignment)
    {
        Audio = audio;
        IsFinal = isFinal;
        NormalizedAlignment = normalizedAlignment;
        Alignment = alignment;
        AudioBytes = audio != null ? Convert.FromBase64String(audio) : null;
    }
}