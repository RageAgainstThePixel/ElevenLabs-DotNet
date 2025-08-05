using System.Runtime.Serialization;

namespace ElevenLabs.Voices;

public enum SortDirections
{
    [EnumMember(Value = "created_at_unix")]
    /// <summary>
    /// Sort by the creation time (Unix timestamp). May not be available for older voices.
    /// </summary>
    CreatedAtUnix,

    [EnumMember(Value = "name")]
    /// <summary>
    /// Sort by the name of the voice.
    /// </summary>
    Name
}
