// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;

namespace ElevenLabs.SpeechToText
{
    public sealed class SpeechToTextRequest
    {
        public SpeechToTextRequest(
            Stream audioFile,
            string modelId = "scribe_v1",
            string languageCode = null,
            bool tagAudioEvents = true,
            int? numSpeakers = null,
            bool timestamps = false,
            bool diarize = false)
        {
            AudioFile = audioFile;
            ModelId = modelId;
            LanguageCode = languageCode;
            TagAudioEvents = tagAudioEvents;
            NumSpeakers = numSpeakers;
            TimestampsGranularity = timestamps ? "word" : "none";
            Diarize = diarize;
        }

        public Stream AudioFile { get; }

        public string ModelId { get; }

        public string LanguageCode { get; }

        public bool TagAudioEvents { get; }

        public int? NumSpeakers { get; }

        public string TimestampsGranularity { get; }

        public bool Diarize { get; }
    }
}