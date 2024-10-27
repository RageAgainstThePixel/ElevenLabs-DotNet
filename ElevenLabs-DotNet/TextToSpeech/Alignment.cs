using System.Text.Json.Serialization;

namespace ElevenLabs.TextToSpeech;

public class Alignment
{
    [JsonPropertyName("char_start_times_ms")]
    public int[] CharStartTimesMs { get; }

    [JsonPropertyName("chars_durations_ms")]
    public int[] CharsDurationsMs { get; }

    [JsonPropertyName("chars")]
    public string[] Chars { get; }

    public Alignment(int[] charStartTimesMs, int[] charsDurationsMs, string[] chars)
    {
        CharStartTimesMs = charStartTimesMs;
        CharsDurationsMs = charsDurationsMs;
        Chars = chars;
    }
}