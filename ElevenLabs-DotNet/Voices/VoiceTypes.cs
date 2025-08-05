using System.Runtime.Serialization;
namespace ElevenLabs.Voices;

/// <summary>
/// Type of the voice to filter by.
/// </summary>
public enum VoiceTypes
{
    /// <summary>
    /// Personal voice.
    /// </summary>
    [EnumMember(Value = "personal")]
    Personal,
    /// <summary>
    /// Community voice.
    /// </summary>
    [EnumMember(Value = "community")]
    Community,
    /// <summary>
    /// Default voice.
    /// </summary>
    [EnumMember(Value = "default")]
    Default,
    /// <summary>
    /// Workspace voice.
    /// </summary>
    [EnumMember(Value = "workspace")]
    Workspace,
    /// <summary>
    /// Non-default voice (all but 'default').
    /// </summary>
    [EnumMember(Value = "non-default")]
    NonDefault
}
