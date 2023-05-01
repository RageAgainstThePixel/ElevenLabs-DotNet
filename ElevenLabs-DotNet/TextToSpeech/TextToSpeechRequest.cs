// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Models;
using ElevenLabs.Voices;
using System.Text.Json.Serialization;

namespace ElevenLabs.TextToSpeech
{
    public sealed class TextToSpeechRequest
    {
        public TextToSpeechRequest(string text, Model model, VoiceSettings voiceSettings)
        {
            Text = text;
            Model = model;
            VoiceSettings = voiceSettings;
        }

        [JsonPropertyName("text")]
        public string Text { get; }

        [JsonPropertyName("model_id")]
        public string Model { get; }

        [JsonPropertyName("voice_settings")]
        public VoiceSettings VoiceSettings { get; }
    }
}
