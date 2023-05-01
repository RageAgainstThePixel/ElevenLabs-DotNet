// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    public sealed class Voice
    {
        public Voice(string id)
        {
            Id = id;
        }

        [JsonInclude]
        [JsonPropertyName("voice_id")]
        public string Id { get; private set; }

        [JsonInclude]
        [JsonPropertyName("name")]
        public string Name { get; private set; }

        [JsonInclude]
        [JsonPropertyName("samples")]
        public IReadOnlyList<Sample> Samples { get; private set; }

        [JsonInclude]
        [JsonPropertyName("category")]
        public string Category { get; private set; }

        [JsonInclude]
        [JsonPropertyName("labels")]
        public IReadOnlyDictionary<string, string> Labels { get; private set; }

        [JsonInclude]
        [JsonPropertyName("preview_url")]
        public string PreviewUrl { get; private set; }

        [JsonInclude]
        [JsonPropertyName("available_for_tiers")]
        public IReadOnlyList<string> AvailableForTiers { get; private set; }

        [JsonInclude]
        [JsonPropertyName("settings")]
        public VoiceSettings Settings { get; internal set; }

        public static implicit operator string(Voice voice) => voice.ToString();

        public override string ToString() => Id;

        #region Premade Voices

        [JsonIgnore]
        public static Voice Adam { get; } = new Voice("pNInz6obpgDQGcFmaJgB");

        [JsonIgnore]
        public static Voice Antoni { get; } = new Voice("ErXwobaYiN019PkySvjV");

        [JsonIgnore]
        public static Voice Arnold { get; } = new Voice("VR6AewLTigWG4xSOukaG");

        [JsonIgnore]
        public static Voice Bella { get; } = new Voice("EXAVITQu4vr4xnSDxMaL");

        [JsonIgnore]
        public static Voice Domi { get; } = new Voice("AZnzlk1XvdvUeBnXmlld");

        [JsonIgnore]
        public static Voice Elli { get; } = new Voice("MF3mGyEYCl7XYWbV9V6O");

        [JsonIgnore]
        public static Voice Josh { get; } = new Voice("TxGEqnHWrfWFTfGW9XjX");

        [JsonIgnore]
        public static Voice Rachel { get; } = new Voice("21m00Tcm4TlvDq8ikWAM");

        [JsonIgnore]
        public static Voice Sam { get; } = new Voice("yoZ06aMxZJJ28mfd3POQ");

        #endregion Premade Voices
    }
}
