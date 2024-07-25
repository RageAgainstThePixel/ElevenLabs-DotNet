// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.TextToSpeech;

public sealed class GenerationConfig
{
    [JsonPropertyName("chunk_length_schedule")]
    public int[] ChunkLengthSchedule { get; }

    public GenerationConfig() : this([120, 160, 250, 290])
    {
    }

    public GenerationConfig(int[] chunkLengthSchedule)
    {
        ChunkLengthSchedule = chunkLengthSchedule;
    }
}