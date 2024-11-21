#region
using Newtonsoft.Json;
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
    [JsonProperty("ms")]
    public float RemainingMS { get; init; }
}