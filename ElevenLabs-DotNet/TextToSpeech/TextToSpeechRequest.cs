// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Models;
using ElevenLabs.Voices;
using System;
using System.Text;
using System.Text.Json.Serialization;

namespace ElevenLabs.TextToSpeech
{
    public sealed class TextToSpeechRequest
    {
        public TextToSpeechRequest(string text, Model model, VoiceSettings voiceSettings) :
            this(null, text, voiceSettings: voiceSettings, model: model)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="voice">
        /// <see cref="Voice"/> to use.
        /// </param>
        /// <param name="text">
        /// Text input to synthesize speech for. Maximum 5000 characters.
        /// </param>
        /// <param name="encoding"><see cref="Encoding"/> to use for <see cref="text"/>.</param>
        /// <param name="voiceSettings">
        /// Optional, <see cref="VoiceSettings"/> that will override the default settings in <see cref="Voice.Settings"/>.
        /// </param>
        /// <param name="model">
        /// Optional, <see cref="Model"/> to use. Defaults to <see cref="Model.MonoLingualV1"/>.
        /// </param>
        /// <param name="languageCode">
        /// Optional, Language code (ISO 639-1) used to enforce a language for the model. Currently only <see cref="Model.TurboV2_5"/> supports language enforcement. 
        /// For other models, an error will be returned if language code is provided.
        /// </param>
        /// <param name="outputFormat">
        /// Output format of the generated audio.<br/>
        /// Defaults to <see cref="OutputFormat.MP3_44100_128"/>
        /// </param>
        /// <param name="optimizeStreamingLatency">
        /// Optional, You can turn on latency optimizations at some cost of quality.
        /// The best possible final latency varies by model.<br/>
        /// Possible values:<br/>
        /// 0 - default mode (no latency optimizations)<br/>
        /// 1 - normal latency optimizations (about 50% of possible latency improvement of option 3)<br/>
        /// 2 - strong latency optimizations (about 75% of possible latency improvement of option 3)<br/>
        /// 3 - max latency optimizations<br/>
        /// 4 - max latency optimizations, but also with text normalizer turned off for even more latency savings
        /// (best latency, but can mispronounce e.g. numbers and dates).
        /// </param>
        /// <param name="previousText"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public TextToSpeechRequest(
            Voice voice,
            string text,
            Encoding encoding = null,
            VoiceSettings voiceSettings = null,
            OutputFormat outputFormat = OutputFormat.MP3_44100_128,
            int? optimizeStreamingLatency = null,
            Model model = null,
            string languageCode = null,
            string previousText = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (text.Length > 5000)
            {
                throw new ArgumentOutOfRangeException(nameof(text), $"{nameof(text)} cannot exceed 5000 characters");
            }

            if (voice == null ||
                string.IsNullOrWhiteSpace(voice.Id))
            {
                throw new ArgumentNullException(nameof(voice));
            }

            if (encoding?.Equals(Encoding.UTF8) == false)
            {
                text = Encoding.UTF8.GetString(encoding.GetBytes(text));
            }

            if (!string.IsNullOrEmpty(languageCode) && model != Models.Model.TurboV2_5)
            {
                throw new ArgumentException($"Currently only Turbo v2.5 model supports language enforcement.", nameof(languageCode));
            }

            Text = text;
            Model = model ?? Models.Model.MultiLingualV2;
            Voice = voice;
            VoiceSettings = voiceSettings ?? voice.Settings ?? throw new ArgumentNullException(nameof(voiceSettings));
            PreviousText = previousText;
            OutputFormat = outputFormat;
            OptimizeStreamingLatency = optimizeStreamingLatency;
            LanguageCode = languageCode;
        }

        [JsonPropertyName("text")]
        public string Text { get; }

        [JsonPropertyName("model_id")]
        public string Model { get; }

        [JsonPropertyName("language_code")]
        public string LanguageCode { get; }

        [JsonIgnore]
        public Voice Voice { get; }

        [JsonPropertyName("voice_settings")]
        public VoiceSettings VoiceSettings { get; }

        [JsonPropertyName("previous_text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string PreviousText { get; }

        [JsonIgnore]
        public OutputFormat OutputFormat { get; }

        [JsonIgnore]
        public int? OptimizeStreamingLatency { get; }
    }
}
