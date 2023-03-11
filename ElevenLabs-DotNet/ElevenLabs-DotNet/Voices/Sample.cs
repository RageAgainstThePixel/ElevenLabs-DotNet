// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace ElevenLabs.Voices
{
    public sealed class Sample
    {
        public static implicit operator string(Sample sample) => sample.Id;

        [JsonInclude]
        [JsonPropertyName("sample_id")]
        public string Id { get; private set; }

        [JsonInclude]
        [JsonPropertyName("file_name")]
        public string FileName { get; private set; }

        [JsonInclude]
        [JsonPropertyName("mime_type")]
        public string MimeType { get; private set; }

        [JsonInclude]
        [JsonPropertyName("size_bytes")]
        public int SizeBytes { get; private set; }

        [JsonInclude]
        [JsonPropertyName("hash")]
        public string Hash { get; private set; }
    }
}
