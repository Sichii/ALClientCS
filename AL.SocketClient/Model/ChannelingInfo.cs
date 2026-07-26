#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Provides information about the channeling of a skill.
/// </summary>
public sealed record ChannelingInfo
{
    /// <summary>
    ///     The remaining MS needed to complete channeling.
    /// </summary>
    [JsonPropertyName("ms")]
    public float RemainingMS { get; init; }
}