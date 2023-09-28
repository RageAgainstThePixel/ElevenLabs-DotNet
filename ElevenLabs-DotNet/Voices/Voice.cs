// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
        [JsonPropertyName("high_quality_base_model_ids")]
        public IReadOnlyList<string> HighQualityBaseModelIds { get; private set; }

        [JsonInclude]
        [JsonPropertyName("settings")]
        public VoiceSettings Settings { get; internal set; }

        public static implicit operator string(Voice voice) => voice.ToString();

        public override string ToString() => Id;

        #region Premade Voices

        [JsonIgnore]
        public static Voice Adam { get; } = new Voice("pNInz6obpgDQGcFmaJgB") { Name = nameof(Adam) };

        [JsonIgnore]
        public static Voice Antoni { get; } = new Voice("ErXwobaYiN019PkySvjV") { Name = nameof(Antoni) };

        [JsonIgnore]
        public static Voice Arnold { get; } = new Voice("VR6AewLTigWG4xSOukaG") { Name = nameof(Arnold) };

        [JsonIgnore]
        public static Voice Bella { get; } = new Voice("EXAVITQu4vr4xnSDxMaL") { Name = nameof(Bella) };

        [JsonIgnore]
        public static Voice Domi { get; } = new Voice("AZnzlk1XvdvUeBnXmlld") { Name = nameof(Domi) };

        [JsonIgnore]
        public static Voice Elli { get; } = new Voice("MF3mGyEYCl7XYWbV9V6O") { Name = nameof(Elli) };

        [JsonIgnore]
        public static Voice Josh { get; } = new Voice("TxGEqnHWrfWFTfGW9XjX") { Name = nameof(Josh) };

        [JsonIgnore]
        public static Voice Rachel { get; } = new Voice("21m00Tcm4TlvDq8ikWAM") { Name = nameof(Rachel) };

        [JsonIgnore]
        public static Voice Sam { get; } = new Voice("yoZ06aMxZJJ28mfd3POQ") { Name = nameof(Sam) };

        #endregion Premade Voices

        public bool Equals(Voice other)
        {
            if (ReferenceEquals(null, other))
            {
                return string.IsNullOrWhiteSpace(Id);
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Name == other.Name &&
                   Id == other.Id &&
                   Equals(Samples, other.Samples) &&
                   Category == other.Category &&
                   Equals(Labels, other.Labels) &&
                   PreviewUrl == other.PreviewUrl &&
                   Equals(AvailableForTiers, other.AvailableForTiers) &&
                   Equals(HighQualityBaseModelIds, other.HighQualityBaseModelIds) &&
                   Equals(Settings, other.Settings);
        }

        public override bool Equals(object voice)
            => ReferenceEquals(this, voice) || voice is Voice other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Name, Id, Samples, Category, Labels, PreviewUrl, AvailableForTiers, Settings);

        public static bool operator !=(Voice left, Voice right) => !(left == right);

        public static bool operator ==(Voice left, Voice right)
        {
            if (left is null && right is null)
            {
                return true;
            }

            if (left is null)
            {
                return string.IsNullOrWhiteSpace(right.Id);
            }

            if (right is null)
            {
                return string.IsNullOrWhiteSpace(left.Id);
            }

            return left.Equals(right);
        }
    }
}
