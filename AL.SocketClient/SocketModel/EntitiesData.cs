#region
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.SocketClient.Model;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received for entity loading and updating.
/// </summary>
public sealed record EntitiesData
{
    /// <summary>
    ///     Which instance your character is in.
    ///     <br />
    ///     If you are in a dungeon, it's a unique ID, otherwise it's the map you are in
    /// </summary>
    public string In { get; init; } = null!;

    public string Map { get; init; } = null!;

    /// <summary>
    ///     The monsters you can see.
    /// </summary>
    public IReadOnlyList<Monster> Monsters { get; init; } = new List<Monster>();

    /// <summary>
    ///     The players you can see
    /// </summary>
    public IReadOnlyList<Player> Players { get; init; } = new List<Player>();

    /// <summary>
    ///     The type of entity update. (full load or positional)
    /// </summary>
    [JsonPropertyName("type")]
    public EntitiesUpdateType UpdateType { get; init; }
}