// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace ElevenLabs.TextToSpeech
{
    public sealed record TextGenerationConfig
    {
        public IReadOnlyList<double> ChunkLengthSchedule { get; init; }

        public IReadOnlyList<PronunciationDictionary> PronunciationDictionaries { get; init; }
    }
}
