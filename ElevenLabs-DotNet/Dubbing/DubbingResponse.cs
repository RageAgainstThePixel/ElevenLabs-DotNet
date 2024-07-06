namespace ElevenLabs.Dubbing;

using System.Text.Json.Serialization;

public sealed class DubbingResponse
{
    [JsonPropertyName("dubbing_id")]
    public string DubbingId { get; set; }

    [JsonPropertyName("expected_duration_sec")]
    public float ExpectedDurationSeconds { get; set; }
}