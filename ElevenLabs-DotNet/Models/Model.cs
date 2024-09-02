// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElevenLabs.Models
{
    public sealed class Model
    {
        public Model(string id)
        {
            Id = id;
        }

        [JsonInclude]
        [JsonPropertyName("model_id")]
        public string Id { get; private set; }

        [JsonInclude]
        [JsonPropertyName("name")]
        public string Name { get; private set; }

        [JsonInclude]
        [JsonPropertyName("description")]
        public string Description { get; private set; }

        [JsonInclude]
        [JsonPropertyName("can_be_finetuned")]
        public bool CanBeFineTuned { get; private set; }

        [JsonInclude]
        [JsonPropertyName("can_do_text_to_speech")]
        public bool CanDoTextToSpeech { get; private set; }

        [JsonInclude]
        [JsonPropertyName("can_do_voice_conversion")]
        public bool CanDoVoiceConversion { get; private set; }

        [JsonInclude]
        [JsonPropertyName("token_cost_factor")]
        public double TokenCostFactor { get; private set; }

        [JsonInclude]
        [JsonPropertyName("languages")]
        public IReadOnlyList<Language> Languages { get; private set; }

        public static implicit operator string(Model model) => model?.ToString();

        public override string ToString() => Id;

        #region Predefined Models

        [JsonIgnore]
        [Obsolete("Use EnglishV1")]
        public static Model MonoLingualV1 => EnglishV1;

        [JsonIgnore]
        [Obsolete("use EnglishTurboV2")]
        public static Model TurboV2 => EnglishTurboV2;

        /// <summary>
        /// Our first ever text to speech model. Now outclassed by Multilingual v2 (for content creation) and Turbo v2.5 (for low latency use cases).
        /// </summary>
        [JsonIgnore]
        public static Model EnglishV1 { get; } = new("eleven_monolingual_v1");

        /// <summary>
        /// Our English-only, low latency model. Best for developer use cases where speed matters and you only need English. Performance is on par with Turbo v2.5.
        /// </summary>
        [JsonIgnore]
        public static Model EnglishTurboV2 { get; } = new("eleven_turbo_v2");

        /// <summary>
        /// Our high quality, low latency model in 32 languages. Best for developer use cases where speed matters and you need non-English languages.
        /// </summary>
        [JsonIgnore]
        public static Model TurboV2_5 { get; } = new("eleven_turbo_v2_5");

        /// <summary>
        /// Our first Multilingual model, capability of generating speech in 10 languages.
        /// Now outclassed by Multilingual v2 (for content creation) and Turbo v2.5 (for low latency use cases).
        /// </summary>
        [JsonIgnore]
        public static Model MultiLingualV1 { get; } = new("eleven_multilingual_v1");

        /// <summary>
        /// Our most life-like, emotionally rich mode in 29 languages. Best for voice overs, audiobooks, post-production, or any other content creation needs.
        /// </summary>
        [JsonIgnore]
        public static Model MultiLingualV2 { get; } = new("eleven_multilingual_v2");

        /// <summary>
        /// Our state-of-the-art speech to speech model suitable for scenarios where you need maximum control over the content and prosody of your generations.
        /// </summary>
        [JsonIgnore]
        public static Model EnglishSpeechToSpeechV2 { get; } = new("eleven_english_sts_v2");

        /// <summary>
        /// Our cutting-edge, multilingual speech-to-speech model is designed for situations that demand unparalleled control over both
        /// the content and the prosody of the generated speech across various languages.
        /// </summary>
        [JsonIgnore]
        public static Model MultilingualSpeechToSpeechV2 { get; } = new("eleven_multilingual_sts_v2");

        #endregion Predefined Models
    }
}
