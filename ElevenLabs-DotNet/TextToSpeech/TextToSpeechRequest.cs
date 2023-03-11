// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using System.Text.Json.Serialization;

namespace ElevenLabs.TextToSpeech
{
    public sealed class TextToSpeechRequest
    {
        public TextToSpeechRequest(string text, VoiceSettings voiceSettings)
        {
            Text = text;
            VoiceSettings = voiceSettings;
        }

        [JsonPropertyName("text")]
        public string Text { get; }

        [JsonPropertyName("voice_settings")]
        public VoiceSettings VoiceSettings { get; }
    }
}
