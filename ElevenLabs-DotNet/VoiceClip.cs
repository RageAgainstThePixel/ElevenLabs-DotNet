// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using ElevenLabs.Voices;
using System;

namespace ElevenLabs
{
    public sealed class VoiceClip
    {
        internal VoiceClip(string id, string text, Voice voice, ReadOnlyMemory<byte> clipData)
        {
            Id = id;
            Text = text;
            Voice = voice;
            TextHash = $"{id}{text}".GenerateGuid().ToString();
            ClipData = clipData;
        }

        public string Id { get; }

        public string Text { get; }

        public Voice Voice { get; }

        public string TextHash { get; }

        public ReadOnlyMemory<byte> ClipData { get; }
    }
}
