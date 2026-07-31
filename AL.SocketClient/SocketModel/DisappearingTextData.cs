#region
using System.Text.Json.Nodes;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when disappearing text appears on the UI.
/// </summary>
public sealed record DisappearingTextData
{
    /// <summary>
    ///     If populated, contains various UI datas
    /// </summary>
    public JsonNode? Args { get; init; }

    /// <summary>
    ///     The id of the entity this text appears over, if any. Omitted by every text the server anchors to a point
    ///     rather than an entity - the gold and xp texts over a corpse or a chest (node/server.js:2825, :2847, :10147).
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    ///     The raw text that appears.
    /// </summary>
    public string Message { get; init; } = null!;

    public float X { get; init; }
    public float Y { get; init; }
}