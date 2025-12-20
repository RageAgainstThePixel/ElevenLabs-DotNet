// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using ElevenLabs.Models;
using ElevenLabs.Voices;

namespace ElevenLabs.TextToSpeech
{
    public sealed class TextToSpeechRequest
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="voice">
        /// <see cref="Voice"/> to use.
        /// </param>
        /// <param name="text">
        /// Text input to synthesize speech for.
        /// </param>
        /// <param name="encoding"><see cref="Encoding"/> to use for <see cref="text"/>.</param>
        /// <param name="voiceSettings">
        /// Optional, <see cref="VoiceSettings"/> that will override the default settings in <see cref="Voice.Settings"/>.
        /// </param>
        /// <param name="model">
        /// Optional, <see cref="Model"/> to use. Defaults to <see cref="Model.FlashV2"/>.
        /// </param>
        /// <param name="outputFormat">
        /// Output format of the generated audio.<br/>
        /// Defaults to <see cref="OutputFormat.MP3_44100_128"/>
        /// </param>
        /// <param name="previousText">
        /// The text that came before the text of the current request.
        /// Can be used to improve the speech's continuity when concatenating together multiple generations or
        /// to influence the speech's continuity in the current generation.
        /// </param>
        /// <param name="nextText">
        /// The text that comes after the text of the current request.
        /// Can be used to improve the speech's continuity when concatenating together multiple generations or
        /// to influence the speech's continuity in the current generation.
        /// </param>
        /// <param name="previousRequestIds">
        /// A list of request_id of the samples that were generated before this generation.
        /// Can be used to improve the speech's continuity when splitting up a large task into multiple requests.
        /// The results will be best when the same model is used across the generations. In case both previous_text and previous_request_ids is send,
        /// previous_text will be ignored. A maximum of 3 request_ids can be send.
        /// </param>
        /// <param name="nextRequestIds">
        /// A list of request_id of the samples that come after this generation.
        /// next_request_ids is especially useful for maintaining the speech's continuity when regenerating a sample that has had some audio quality issues.
        /// For example, if you have generated 3 speech clips, and you want to improve clip 2, passing the request id of clip 3 as a next_request_id
        /// (and that of clip 1 as a previous_request_id) will help maintain natural flow in the combined speech.
        /// The results will be best when the same model is used across the generations.
        /// In case both next_text and next_request_ids is send, next_text will be ignored.
        /// A maximum of 3 request_ids can be send.
        /// </param>
        /// <param name="languageCode">
        /// Optional, Language code (ISO 639-1) used to enforce a language for the model. Currently only <see cref="Model.TurboV2_5"/> supports language enforcement.
        /// For other models, an error will be returned if language code is provided.
        /// </param>
        /// <param name="withTimestamps">
        /// Generate speech from text with precise character-level timing information for audio-text synchronization.
        /// </param>
        /// <param name="seed">
        /// If specified, our system will make a best effort to sample deterministically,
        /// such that repeated requests with the same seed and parameters should return the same result.
        /// Determinism is not guaranteed. Must be integer between 0 and 4294967295.
        /// </param>
        /// <param name="pronunciationDictionaryLocators">
        /// A list of pronunciation dictionary locators (id, version_id) to be applied to the text. They will be applied in order. You may have up to 3 locators per request
        /// </param>
        /// <param name="applyTextNormalization">
        /// This parameter controls text normalization with three modes: 'auto', 'on', and 'off'.
        /// When set to 'auto', the system will automatically decide whether to apply text normalization (e.g., spelling out numbers).
        /// With 'on', text normalization will always be applied, while with 'off', it will be skipped.
        /// For 'eleven_turbo_v2_5' and 'eleven_flash_v2_5' models, text normalization can only be enabled with Enterprise plans.
        /// </param>
        /// <param name="applyLanguageTextNormalization">
        /// This parameter controls language text normalization.
        /// This helps with proper pronunciation of text in some supported languages.
        /// WARNING: This parameter can heavily increase the latency of the request.
        /// Currently only supported for Japanese.
        /// </param>
        public TextToSpeechRequest(
            Voice voice,
            string text,
            Encoding encoding = null,
            VoiceSettings voiceSettings = null,
            OutputFormat outputFormat = OutputFormat.MP3_44100_128,
            Model model = null,
            string previousText = null,
            string nextText = null,
            string[] previousRequestIds = null,
            string[] nextRequestIds = null,
            string languageCode = null,
            bool withTimestamps = false,
            int? seed = null,
            List<PronunciationDictionaryLocator> pronunciationDictionaryLocators = null,
            TextNormalization? applyTextNormalization = null,
            bool? applyLanguageTextNormalization = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
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

            Text = text;
            Model = model ?? Models.Model.FlashV2;
            Voice = string.IsNullOrWhiteSpace(voice) ? Voice.Adam : voice;
            VoiceSettings = voiceSettings ?? voice.Settings;
            OutputFormat = outputFormat;
            PreviousText = previousText;
            NextText = nextText;
            if (previousRequestIds?.Length > 3)
            {
                previousRequestIds = previousRequestIds[..3];
            }
            PreviousRequestIds = previousRequestIds;
            if (nextRequestIds?.Length > 3)
            {
                nextRequestIds = nextRequestIds[..3];
            }
            NextRequestIds = nextRequestIds;
            LanguageCode = languageCode;
            WithTimestamps = withTimestamps;
            Seed = seed;
            ApplyTextNormalization = applyTextNormalization;
        }

        [JsonPropertyName("text")]
        public string Text { get; }

        [JsonPropertyName("model_id")]
        public string Model { get; }

        [JsonIgnore]
        public Voice Voice { get; }

        [JsonPropertyName("voice_settings")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public VoiceSettings VoiceSettings { get; internal set; }

        [JsonPropertyName("previous_text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string PreviousText { get; }

        [JsonIgnore]
        public OutputFormat OutputFormat { get; }

        [JsonPropertyName("next_text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string NextText { get; }

        /// <remarks>
        /// A maximum of three next or previous history item ids can be sent
        /// </remarks>
        [JsonPropertyName("previous_request_ids")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[] PreviousRequestIds { get; }

        /// <remarks>
        /// A maximum of three next or previous history item ids can be sent
        /// </remarks>
        [JsonPropertyName("next_request_ids")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[] NextRequestIds { get; }

        [JsonPropertyName("language_code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string LanguageCode { get; }

        [JsonIgnore]
        public bool WithTimestamps { get; }

        [JsonPropertyName("seed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Seed { get; }

        [JsonPropertyName("apply_text_normalization")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TextNormalization? ApplyTextNormalization { get; }
    }
}
