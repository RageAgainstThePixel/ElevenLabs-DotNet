// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using System;

namespace ElevenLabs
{
    public sealed class VoiceClip : GeneratedClip
    {
        internal VoiceClip(string id, string text, Voice voice, ReadOnlyMemory<byte> clipData, int sampleRate = 44100)
            : base(id, text, clipData, sampleRate)
        {
            Voice = voice;
        }

        public Voice Voice { get; }

        public TimestampedTranscriptCharacter[] TimestampedTranscriptCharacters { get; internal init; }
    }
}
