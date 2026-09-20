#region
using System.Text.Json.Serialization;
using AL.SocketClient.Model;
using Chaos.Extensions.Common;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     One cave frame: the daily dungeon's run state, sent whenever it changes. Every frame but "ended" carries the whole
///     state; "chat" and "cue" add the line a traveler or actor just said.
/// </summary>
public sealed record CaveData
{
    /// <summary>A traveler's line, on a "chat" frame.</summary>
    [JsonPropertyName("chat")]
    public CaveChat? Chat { get; init; }

    /// <summary>
    ///     An actor's line spoken in a room, on a "cue" frame.
    /// </summary>
    [JsonPropertyName("cue")]
    public CaveCue? Cue { get; init; }

    /// <summary>
    ///     The run state after this change. Absent on an "ended" frame, or carrying the final state.
    /// </summary>
    [JsonPropertyName("state")]
    public CaveState? State { get; init; }

    /// <summary>
    ///     What changed: "ended", "chat", "cue", "choice", "result", or a plain state update.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = null!;

    /// <summary>
    ///     Whether this frame ends the run for this character: the visit is over, or the character left.
    /// </summary>
    [JsonIgnore]
    public bool Ended => "ended".EqualsI(Type);
}