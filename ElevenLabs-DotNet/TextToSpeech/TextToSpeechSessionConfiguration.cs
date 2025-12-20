// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using System.Text.Json.Serialization;

namespace ElevenLabs.TextToSpeech
{
    public sealed record TextToSpeechSessionConfiguration
    {
        /// <summary>
        /// The voice to use.
        /// </summary>
        [JsonIgnore]
        public Voice Voice { get; init; }

        [JsonIgnore]
        public VoiceSettings VoiceSettings { get; init; }

        /// <summary>
        /// The model to use.
        /// </summary>
        [JsonPropertyName("model_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Model { get; init; }

        /// <summary>
        /// The ISO 639-1 language code (for specific models).
        /// </summary>
        [JsonPropertyName("language_code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string LanguageCode { get; init; }

        /// <summary>
        /// Whether to enable logging of the request.
        /// </summary>
        [JsonPropertyName("enable_logging")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? EnableLogging { get; init; }

        /// <summary>
        /// Whether to enable SSML parsing.
        /// </summary>
        [JsonPropertyName("enable_ssml_parsing")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? EnableSSMLParsing { get; init; }

        /// <summary>
        /// The output audio format
        /// </summary>
        [JsonPropertyName("output_format")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OutputFormat? OutputFormat { get; init; }

        /// <summary>
        /// Timeout for inactivity before a context is closed (seconds), can be up to 180 seconds.
        /// </summary>
        [JsonPropertyName("inactivity_timeout")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? InactivityTimeout { get; init; }

        /// <summary>
        /// Whether to include timing data with every audio chunk.
        /// </summary>
        [JsonPropertyName("sync_alignment")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? SyncAlignment { get; init; }

        /// <summary>
        /// Reduces latency by disabling chunk schedule and buffers. Recommended for full sentences/phrases.
        /// </summary>
        [JsonPropertyName("auto_mode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AutoMode { get; init; }

        /// <summary>
        /// This parameter controls text normalization with three modes - ‘auto’, ‘on’, and ‘off’.
        /// When set to ‘auto’, the system will automatically decide whether to apply text normalization(e.g., spelling out numbers).
        /// With ‘on’, text normalization will always be applied, while with ‘off’,
        /// it will be skipped.For ‘eleven_turbo_v2_5’ and ‘eleven_flash_v2_5’ models,
        /// text normalization can only be enabled with Enterprise plans.Defaults to ‘auto’.
        /// </summary>
        [JsonPropertyName("apply_text_normalization")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TextNormalization? TextNormalization { get; init; }

        [JsonIgnore]
        public TextGenerationConfig TextGenerationConfig { get; init; }

        /// <summary>
        /// If specified, system will best-effort sample deterministically. Integer between 0 and 4294967295.
        /// </summary>
        [JsonPropertyName("seed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public uint? Seed { get; init; }
    }
}
