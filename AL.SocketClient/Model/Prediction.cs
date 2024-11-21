#region
using System.Collections.Generic;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents a prediction for an upgrade/compound action.
/// </summary>
public sealed record Prediction
{
    /// <summary>
    ///     The chance for the upgrade/compound to succeed.
    /// </summary>
    public float Chance { get; init; }

    /// <summary>
    ///     The current level of the item.
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    ///     The name of the item.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     TODO: What's this?
    /// </summary>
    public IReadOnlyList<int> Nums { get; init; } = new List<int>();

    /// <summary>
    ///     The name of the scroll being used to upgrade the item.
    /// </summary>
    [JsonProperty("scroll")]
    public string ScrollName { get; init; } = null!;

    /// <summary>
    ///     Whether or not the upgrade succeeded. (This will always be false for predictions)
    /// </summary>
    public bool Success { get; init; }
}