#region
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.SocketClient.Model;
using Newtonsoft.Json;
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
    [JsonProperty]
    public string In { get; init; } = null!;

    [JsonProperty]
    public string Map { get; init; } = null!;

    /// <summary>
    ///     The monsters you can see.
    /// </summary>
    [JsonProperty]
    public IReadOnlyList<Monster> Monsters { get; init; } = new List<Monster>();

    /// <summary>
    ///     The players you can see
    /// </summary>
    [JsonProperty]
    public IReadOnlyList<Player> Players { get; init; } = new List<Player>();

    /// <summary>
    ///     The type of entity update. (full load or positional)
    /// </summary>
    [JsonProperty("type")]
    public EntitiesUpdateType UpdateType { get; init; }
}