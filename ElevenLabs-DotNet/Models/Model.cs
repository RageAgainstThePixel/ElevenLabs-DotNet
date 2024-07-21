// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        [JsonPropertyName("description")]
        public string Description { get; private set; }

        [JsonInclude]
        [JsonPropertyName("languages")]
        public IReadOnlyList<Language> Languages { get; private set; }

        public static implicit operator string(Model model) => model.ToString();

        public override string ToString() => Id;

        #region Predefined Models

        [JsonIgnore]
        public static Model MonoLingualV1 { get; } = new("eleven_monolingual_v1");

        [JsonIgnore]
        public static Model MultiLingualV1 { get; } = new("eleven_multilingual_v1");

        [JsonIgnore]
        public static Model MultiLingualV2 { get; } = new("eleven_multilingual_v2");

        [JsonIgnore]
        public static Model TurboV2 { get; } = new("eleven_turbo_v2");

        [JsonIgnore]
        public static Model EnglishSpeechToSpeechV2 { get; } = new("eleven_english_sts_v2");

        [JsonIgnore]
        public static Model MultilingualSpeechToSpeechV2 { get; } = new("eleven_multilingual_sts_v2");

        #endregion Predefined Models
    }
}
