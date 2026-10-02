#region
using System.Text.Json.Nodes;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when disappearing text appears on the UI.
/// </summary>
public sealed record DisappearingTextData
{
    /// <summary>If populated, contains various UI datas</summary>
    public JsonNode? Args { get; init; }

    /// <summary>
    ///     If populated, the id of the entity this text appears over. A text anchored to a point, such as the gold and xp over
    ///     a corpse or a chest, omits it.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>The raw text that appears.</summary>
    public string Message { get; init; } = null!;

    public float X { get; init; }
    public float Y { get; init; }
}