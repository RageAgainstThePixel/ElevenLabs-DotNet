// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Models;
using ElevenLabs.Voices;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ElevenLabs.TextToSpeech;

/// <summary>
///     Access to convert text to synthesized speech using a WebSocket connection.
/// </summary>
public sealed class TextToSpeechWebSocketEndpoint : ElevenLabsBaseEndPoint
{
    private const string ModelIdParameter = "model_id";
    private const string EnableLoggingParameter = "enable_logging";
    private const string EnableSsmlParsingParameter = "enable_ssml_parsing";
    private const string OptimizeStreamingLatencyParameter = "optimize_streaming_latency";
    private const string OutputFormatParameter = "output_format";
    private const string InactivityTimeoutParameter = "inactivity_timeout";

    public TextToSpeechWebSocketEndpoint(ElevenLabsClient client) : base(client)
    {
    }

    protected override string Root => "text-to-speech";

    /// <summary>
    ///     Converts text into speech using a voice of your choice and returns audio.
    /// </summary>
    /// <param name="voice">
    ///     <see cref="Voice" /> to use.
    /// </param>
    /// <param name="partialClipCallback">
    ///     Callback for streamed audio as it comes in.<br />
    ///     Returns partial <see cref="VoiceClip" />.
    /// </param>
    /// <param name="voiceSettings">
    ///     Optional, <see cref="VoiceSettings" /> that will override the default settings in <see cref="Voice.Settings" />.
    /// </param>
    /// <param name="generationConfig">Optional, <see cref="GenerationConfig" />.</param>
    /// <param name="model">
    ///     Optional, <see cref="Model" /> to use. Defaults to <see cref="Model.MonoLingualV1" />.
    /// </param>
    /// <param name="outputFormat">
    ///     Output format of the generated audio.<br />
    ///     Defaults to <see cref="OutputFormat.MP3_44100_128" />
    /// </param>
    /// <param name="enableLogging">Optional, enable logging.</param>
    /// <param name="enableSsmlParsing">Optional, enable SSML parsing.</param>
    /// <param name="optimizeStreamingLatency">
    ///     Optional, You can turn on latency optimizations at some cost of quality.
    ///     The best possible final latency varies by model.<br />
    ///     Possible values:<br />
    ///     0 - default mode (no latency optimizations)<br />
    ///     1 - normal latency optimizations (about 50% of possible latency improvement of option 3)<br />
    ///     2 - strong latency optimizations (about 75% of possible latency improvement of option 3)<br />
    ///     3 - max latency optimizations<br />
    ///     4 - max latency optimizations, but also with text normalizer turned off for even more latency savings
    ///     (best latency, but can mispronounce eg numbers and dates).
    /// </param>
    /// <param name="inactivityTimeout">
    ///     The number of seconds that the connection can be inactive before it is automatically closed.
    ///     Defaults to 20 seconds, with a maximum allowed value of 180 seconds.
    /// </param>
    /// <param name="cancellationToken">Optional, <see cref="CancellationToken" />.</param>
    /// <exception cref="ArgumentNullException">Raised when <paramref name="voice" /> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Raised when <paramref name="partialClipCallback" /> is null.</exception>
    public async Task StartTextToSpeechAsync(Voice voice, Func<VoiceClip, Task> partialClipCallback,
        VoiceSettings voiceSettings = null, GenerationConfig generationConfig = null, Model model = null,
        OutputFormat outputFormat = OutputFormat.MP3_44100_128, bool? enableLogging = null,
        bool? enableSsmlParsing = null, int? optimizeStreamingLatency = null, int? inactivityTimeout = null,
        CancellationToken cancellationToken = default)
    {
        if (voice == null ||
            string.IsNullOrWhiteSpace(voice.Id))
        {
            throw new ArgumentNullException(nameof(voice));
        }

        if (partialClipCallback == null)
        {
            throw new ArgumentNullException(nameof(partialClipCallback));
        }

        var parameters = new Dictionary<string, string>
        {
            { ModelIdParameter, model?.Id ?? Model.EnglishV1.Id },
            { OutputFormatParameter, outputFormat.ToString().ToLower() }
        };

        if (enableLogging.HasValue)
        {
            parameters.Add(EnableLoggingParameter, enableLogging.ToString());
        }

        if (enableSsmlParsing.HasValue)
        {
            parameters.Add(EnableSsmlParsingParameter, enableSsmlParsing.ToString());
        }

        if (optimizeStreamingLatency.HasValue)
        {
            parameters.Add(OptimizeStreamingLatencyParameter, optimizeStreamingLatency.ToString());
        }

        if (inactivityTimeout.HasValue)
        {
            parameters.Add(InactivityTimeoutParameter, inactivityTimeout.ToString());
        }

        await client.WebSocketClient.ConnectAsync(
            new Uri(GetWebSocketUrl($"/{voice.Id}/stream-input", parameters)), cancellationToken);

        // start receiving messages in a separate task
        _ = Task.Run(async () => await ReceiveMessagesAsync(partialClipCallback, voice, cancellationToken),
            cancellationToken);

        TextToSpeechWebSocketFirstMessageRequest firstMessageRequest = new(voiceSettings, generationConfig);
        await client.WebSocketClient.SendAsync(firstMessageRequest.ToArraySegment(), WebSocketMessageType.Text, true,
            cancellationToken);
    }

    /// <summary>
    ///     Sends text to the WebSocket for speech synthesis.
    /// </summary>
    /// <param name="text">Text input to synthesize speech for. Needs to end with a space and cannot be null or empty.</param>
    /// <param name="flush">
    ///     Forces the generation of audio. Set this value to true when you have finished sending text, but
    ///     want to keep the websocket connection open.
    /// </param>
    /// <param name="tryTriggerGeneration">
    ///     Use this to attempt to immediately trigger the generation of audio. Most users
    ///     shouldn't use this.
    /// </param>
    /// <param name="cancellationToken">Optional, <see cref="CancellationToken" />.</param>
    /// <exception cref="InvalidOperationException">Raised when the WebSocket is not open.</exception>
    /// <exception cref="ArgumentNullException">Raised when <paramref name="text" /> is null or empty.</exception>
    public async Task SendTextToSpeechAsync(string text, bool? flush = null, bool tryTriggerGeneration = false,
        CancellationToken cancellationToken = default)
    {
        if (client.WebSocketClient.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not open!");
        }

        TextToSpeechWebSocketRequest request = new(text, flush, tryTriggerGeneration);
        await client.WebSocketClient.SendAsync(request.ToArraySegment(), WebSocketMessageType.Text, true,
            cancellationToken);
    }

    /// <summary>
    ///     Closes the text to speech WebSocket connection.
    /// </summary>
    /// <param name="cancellationToken">Optional, <see cref="CancellationToken" />.</param>
    /// <exception cref="InvalidOperationException">Raised when the WebSocket is not open.</exception>
    public async Task EndTextToSpeechAsync(CancellationToken cancellationToken = default)
    {
        if (client.WebSocketClient.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not open!");
        }

        TextToSpeechWebSocketLastMessageRequest lastMessageRequest = new();
        await client.WebSocketClient.SendAsync(lastMessageRequest.ToArraySegment(), WebSocketMessageType.Text, true,
            cancellationToken);
        await client.WebSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
    }

    private async Task ReceiveMessagesAsync(Func<VoiceClip, Task> partialClipCallback, Voice voice,
        CancellationToken cancellationToken)
    {
        try
        {
            byte[] buffer = new byte[8192];
            StringBuilder message = new();

            while (client.WebSocketClient.State == WebSocketState.Open)
            {
                WebSocketReceiveResult receiveResult = await client.WebSocketClient.ReceiveAsync(
                    new ArraySegment<byte>(buffer), cancellationToken);

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await client.WebSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
                        cancellationToken);
                    break;
                }

                string jsonString = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                message.Append(jsonString);

                if (!receiveResult.EndOfMessage)
                {
                    continue;
                }

                TextToSpeechWebSocketResponse response = JsonSerializer.Deserialize<TextToSpeechWebSocketResponse>(
                    message.ToString(), ElevenLabsClient.JsonSerializationOptions);

                if (response == null)
                {
                    throw new ArgumentException("Failed to parse response!");
                }

                message.Clear();

                if (!string.IsNullOrWhiteSpace(response.Audio))
                {
                    string text = response.Alignment is { Chars: not null }
                        ? string.Concat(response.Alignment.Chars)
                        : null;
                    VoiceClip voiceClip = new(string.Empty, text, voice, response.AudioBytes);
                    await partialClipCallback(voiceClip);
                }
                else
                {
                    await partialClipCallback(null);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            client.ReinitializeWebSocketClient();
        }
    }
}