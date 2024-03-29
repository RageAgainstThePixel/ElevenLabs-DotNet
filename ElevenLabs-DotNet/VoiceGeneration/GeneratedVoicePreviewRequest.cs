// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text.Json.Serialization;

namespace ElevenLabs.VoiceGeneration
{
    public sealed class GeneratedVoicePreviewRequest
    {
        /// <summary>
        /// Voice Generation Preview Request.
        /// Use <see cref="VoiceGenerationEndpoint.GetVoiceGenerationOptionsAsync"/> to get a full list of options.
        /// </summary>
        /// <param name="text">Sample text to return for voice generation. Must be between 100 and 1000 characters.</param>
        /// <param name="gender">The gender of the voice to generate.</param>
        /// <param name="accent">The accent of the voice to generate.</param>
        /// <param name="age">The age of the voice to generate.</param>
        /// <param name="accentStrength">Optional, accept strength, between 0.3 - 2.</param>
        public GeneratedVoicePreviewRequest(string text, Gender gender, Accent accent, Age age, double accentStrength = 1)
        {
            switch (text.Length)
            {
                case < 100:
                    throw new ArgumentOutOfRangeException(nameof(text), $"{nameof(text)} must be longer than 100 characters.");
                case > 1000:
                    throw new ArgumentOutOfRangeException(nameof(text), $"{nameof(text)} cannot be longer than 1000 characters.");
                default:
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        throw new ArgumentNullException(nameof(text));
                    }

                    break;
            }

            Text = text;
            Gender = gender.Code;
            Accent = accent.Code;
            Age = age.Code;

            accentStrength = accentStrength switch
            {
                < 0.3f => 0.3f,
                > 2f => 2f,
                _ => accentStrength
            };

            AccentStrength = accentStrength;
        }

        [JsonPropertyName("text")]
        public string Text { get; }

        [JsonPropertyName("gender")]
        public string Gender { get; }

        [JsonPropertyName("accent")]
        public string Accent { get; }

        [JsonPropertyName("age")]
        public string Age { get; }

        [JsonPropertyName("accent_strength")]
        public double AccentStrength { get; }
    }
}
