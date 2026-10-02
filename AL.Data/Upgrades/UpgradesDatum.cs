#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Upgrades;

/// <summary>
///     Represents the base chance of an upgrade succeeding, one row per item grade keyed by the level being reached. The
///     server applies its own modifiers on top.
/// </summary>
/// <remarks>
///     Present in the game data only from version 16846.
/// </remarks>
/// <seealso cref="DatumBase{T}" />
public class UpgradesDatum : DatumBase<IReadOnlyDictionary<int, double>>
{
    [JsonPropertyName("0")]
    public IReadOnlyDictionary<int, double> Grade0 { get; init; } = null!;

    [JsonPropertyName("1")]
    public IReadOnlyDictionary<int, double> Grade1 { get; init; } = null!;

    [JsonPropertyName("2")]
    public IReadOnlyDictionary<int, double> Grade2 { get; init; } = null!;

    /// <summary>
    ///     Gets the base chance of reaching a level on an item of a grade.
    /// </summary>
    /// <param name="grade">
    ///     The item's grade, which picks the row.
    /// </param>
    /// <param name="level">
    ///     The level being reached.
    /// </param>
    /// <returns>
    ///     The base chance, or null where the table has no entry.
    /// </returns>
    public double? GetChance(int grade, int level)
        => grade switch
           {
               0 => Grade0,
               1 => Grade1,
               2 => Grade2,
               _ => null
           } is { } row
           && row.TryGetValue(level, out var chance)
            ? chance
            : null;
}