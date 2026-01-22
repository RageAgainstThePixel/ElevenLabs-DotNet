// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ElevenLabs.SpeechToText
{
    public sealed record SpeechToTextSessionConfiguration
    {
        /// <summary>
        /// The ID of the model to use for transcription, e.g. "scribe_v1".
        /// </summary>
        [JsonPropertyName("model_id")]
        public string ModelId { get; init; } = "scribe_v1";

        /// <summary>
        /// Whether the session will include word-level timestamps in the committed transcript.
        /// </summary>
        [JsonPropertyName("include_timestamps")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IncludeTimestamps { get; init; }

        /// <summary>
        /// Whether the session will include language detection in the committed transcript.
        /// </summary>
        [JsonPropertyName("include_language_detection")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IncludeLanguageDetection { get; init; }

        /// <summary>
        /// The format of the audio to be sent.
        /// </summary>
        [JsonPropertyName("audio_format")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AudioFormat? AudioFormat { get; init; }

        /// <summary>
        /// Language code in ISO 639-1 or ISO 639-3 format.
        /// </summary>
        [JsonPropertyName("language_code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string LanguageCode { get; init; }

        /// <summary>
        /// Strategy for committing transcriptions.
        /// </summary>
        [JsonPropertyName("commit_strategy")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CommitStrategy? CommitStrategy { get; init; }

        /// <summary>
        /// Silence threshold in seconds for VAD.
        /// </summary>
        [JsonPropertyName("vad_silence_threshold_secs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? VadSilenceThresholdSecs { get; init; }

        /// <summary>
        /// Threshold for voice activity detection.
        /// </summary>
        [JsonPropertyName("vad_threshold")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? VadThreshold { get; init; }

        /// <summary>
        /// Minimum speech duration in milliseconds.
        /// </summary>
        [JsonPropertyName("min_speech_duration_ms")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinSpeechDurationMs { get; init; }

        /// <summary>
        /// Minimum silence duration in milliseconds.
        /// </summary>
        [JsonPropertyName("min_silence_duration_ms")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinSilenceDurationMs { get; init; }

        /// <summary>
        /// Whether to enable logging.
        /// </summary>
        [JsonPropertyName("enable_logging")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? EnableLogging { get; init; }

        internal Dictionary<string, string> ToQueryParams()
        {
            var options = ElevenLabsClient.JsonSerializationOptions;
            var dict = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(ModelId))
            {
                dict.Add("model_id", ModelId);
            }

            if (IncludeTimestamps.HasValue)
            {
                dict.Add("include_timestamps", IncludeTimestamps.Value.ToString().ToLower());
            }

            if (IncludeLanguageDetection.HasValue)
            {
                dict.Add("include_language_detection", IncludeLanguageDetection.Value.ToString().ToLower());
            }

            if (AudioFormat.HasValue)
            {
                // Serialize enum to string using the JsonConverter (removes quotes)
                var json = System.Text.Json.JsonSerializer.Serialize(AudioFormat.Value, options);
                dict.Add("audio_format", json.Trim('"'));
            }

            if (!string.IsNullOrWhiteSpace(LanguageCode))
            {
                dict.Add("language_code", LanguageCode);
            }

            if (CommitStrategy.HasValue)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(CommitStrategy.Value, options);
                dict.Add("commit_strategy", json.Trim('"'));
            }

            if (VadSilenceThresholdSecs.HasValue)
            {
                dict.Add("vad_silence_threshold_secs", VadSilenceThresholdSecs.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (VadThreshold.HasValue)
            {
                dict.Add("vad_threshold", VadThreshold.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (MinSpeechDurationMs.HasValue)
            {
                dict.Add("min_speech_duration_ms", MinSpeechDurationMs.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (MinSilenceDurationMs.HasValue)
            {
                dict.Add("min_silence_duration_ms", MinSilenceDurationMs.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (EnableLogging.HasValue)
            {
                dict.Add("enable_logging", EnableLogging.Value.ToString().ToLower());
            }

            return dict;
        }
    }
}