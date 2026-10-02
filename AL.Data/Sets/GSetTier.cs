#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Sets;

/// <summary>
///     Represents what one piece count of a set is worth.
/// </summary>
public sealed record GSetTier
{
    /// <summary>
    ///     This tier's own line, as the wire carries it. Empty where the tier is <c>{}</c>, as most single-piece tiers are.
    /// </summary>
    public GSetBonus Adds { get; init; } = new();

    /// <summary>
    ///     What the server applies at this count: every tier up to and including this one, summed.
    /// </summary>
    public GSetBonus InEffect { get; init; } = new();

    /// <summary>How many pieces of the set are worn.</summary>
    public int Pieces { get; init; }

    /// <summary>
    ///     The tier's heading in the game-data explorer, such as "2 pieces".
    /// </summary>
    [JsonIgnore]
    public string Name => Pieces == 1 ? "1 piece" : $"{Pieces} pieces";
}