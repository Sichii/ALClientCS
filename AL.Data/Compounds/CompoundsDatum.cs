#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Compounds;

/// <summary>
///     The base chance of a compound succeeding: one row per item grade, keyed inside the row by the level being reached.
///     The server reads it as compounds[grade][level] and applies its own modifiers on top. The payload only began
///     carrying it at game data version 16846.
/// </summary>
/// <remarks>
///     Rows are named by the grade they price rather than by their wire key, so reaching +3 on a normal item reads as
///     <c>Grade0[3]</c>.
/// </remarks>
/// <seealso cref="DatumBase{T}" />
public class CompoundsDatum : DatumBase<IReadOnlyDictionary<int, double>>
{
    [JsonPropertyName("0")]
    public IReadOnlyDictionary<int, double> Grade0 { get; init; } = null!;

    [JsonPropertyName("1")]
    public IReadOnlyDictionary<int, double> Grade1 { get; init; } = null!;

    [JsonPropertyName("2")]
    public IReadOnlyDictionary<int, double> Grade2 { get; init; } = null!;

    /// <summary>
    ///     The base chance of reaching <paramref name="level" /> on an item of <paramref name="grade" />, or null where the
    ///     table has no entry.
    /// </summary>
    public double? ChanceOf(int grade, int level)
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