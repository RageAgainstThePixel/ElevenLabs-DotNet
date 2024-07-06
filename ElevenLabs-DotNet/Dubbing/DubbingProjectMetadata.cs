namespace ElevenLabs.Dubbing;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public sealed class DubbingProjectMetadata
{
    [JsonPropertyName("dubbing_id")]
    public string DubbingId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("target_languages")]
    public List<string> TargetLanguages { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }
}
