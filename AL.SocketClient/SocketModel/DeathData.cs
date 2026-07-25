#region
using System.Collections.Generic;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when an entity dies (node/server.js:11898).
/// </summary>
public sealed record DeathData
{
    /// <summary>
    ///     The id of the entity that died.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    ///     The killer's luck multiplier (1 when the killer had no luck bonus).
    /// </summary>
    [JsonProperty("luckm")]
    public float LuckM { get; init; }

    /// <summary>
    ///     For cooperative monsters (announce-bosses), each contributor's accumulated points by character name;
    ///     absent for non-cooperative deaths.
    /// </summary>
    [JsonProperty("points")]
    public IReadOnlyDictionary<string, float>? Points { get; init; }
}
