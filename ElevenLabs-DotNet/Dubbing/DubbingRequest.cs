namespace ElevenLabs.Dubbing;

public sealed class DubbingRequest
{
    /// <summary>
    /// automatic or manual. Manual mode is only supported when creating a dubbing studio project
    /// </summary>
    public string Mode { get; init; } = "automatic";

    /// <summary>
    /// A video (MediaType: "video/mp4") or audio (MediaType: "audio/mpeg") file intended for voice cloning
    /// </summary>
    public (string FilePath, string MediaType) File { get; init; }

    /// <summary>
    /// CSV file containing transcription/translation metadata
    /// </summary>
    public string CsvFilePath { get; init; }

    /// <summary>
    /// For use only with csv input
    /// </summary>
    public string ForegroundAudioFilePath { get; init; }

    /// <summary>
    /// For use only with csv input
    /// </summary>
    public string BackgroundAudioFilePath { get; init; }

    /// <summary>
    /// Name of the dubbing project.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// URL of the source video/audio file.
    /// </summary>
    public string SourceUrl { get; init; }

    /// <summary>
    /// Source language.
    /// </summary>
    /// <remarks>
    /// A list of supported languages can be found at: https://elevenlabs.io/docs/api-reference/how-to-dub-a-video#list-of-supported-languages-for-dubbing
    /// </remarks>
    public string SourceLanguage { get; init; }

    /// <summary>
    /// The Target language to dub the content into. Can be none if dubbing studio editor is enabled and running manual mode
    /// </summary>
    /// <remarks>
    /// A list of supported languages can be found at: https://elevenlabs.io/docs/api-reference/how-to-dub-a-video#list-of-supported-languages-for-dubbing
    /// </remarks>
    public string TargetLanguage { get; init; }

    /// <summary>
    /// Number of speakers to use for the dubbing. Set to 0 to automatically detect the number of speakers
    /// </summary>
    public int? NumSpeakers { get; init; }

    /// <summary>
    /// Whether to apply watermark to the output video.
    /// </summary>
    public bool? Watermark { get; init; }

    /// <summary>
    /// Start time of the source video/audio file.
    /// </summary>
    public int? StartTime { get; init; }

    /// <summary>
    /// End time of the source video/audio file.
    /// </summary>
    public int? EndTime { get; init; }

    /// <summary>
    /// Whether to use the highest resolution available.
    /// </summary>
    public bool? HighestResolution { get; init; }

    /// <summary>
    /// Whether to prepare dub for edits in dubbing studio.
    /// </summary>
    public bool? DubbingStudio { get; init; }
}
