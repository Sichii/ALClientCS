#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     One piece of a generated map bundle. The server streams a daily dungeon's floors as a run of these under one run
///     id, in index order; the pieces joined in that order are one JSON document (see <c>GeneratedMapBundle</c> ).
/// </summary>
public sealed record MapChunkData
{
    /// <summary>How many chunks the bundle has in total.</summary>
    [JsonPropertyName("count")]
    public int Count { get; init; }

    /// <summary>This chunk's position in the bundle, from zero.</summary>
    [JsonPropertyName("index")]
    public int Index { get; init; }

    /// <summary>
    ///     The run this bundle belongs to: a 24-character hex id that also names every floor it delivers.
    /// </summary>
    [JsonPropertyName("run")]
    public string Run { get; init; } = null!;

    /// <summary>This chunk's slice of the bundle text.</summary>
    [JsonPropertyName("text")]
    public string Text { get; init; } = null!;
}