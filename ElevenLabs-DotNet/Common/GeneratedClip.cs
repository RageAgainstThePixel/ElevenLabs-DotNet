// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System;

namespace ElevenLabs
{
    public class GeneratedClip
    {
        internal GeneratedClip(string id, string text, ReadOnlyMemory<byte> clipData, int sampleRate = 44100)
        {
            Id = id;
            Text = text;
            TextHash = $"{id}{text}".GenerateGuid().ToString();
            ClipData = clipData;
            SampleRate = sampleRate;
        }

        /// <summary>
        /// The unique id of this clip.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The text input that generated this clip.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Hash string of id and text.
        /// </summary>
        public string TextHash { get; }

        /// <summary>
        /// The ray clip data.
        /// </summary>
        public ReadOnlyMemory<byte> ClipData { get; }

        public int SampleRate { get; }
    }
}