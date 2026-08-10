#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Tokens;

/// <summary>
///     <inheritdoc cref="DatumBase{T}" />
///     <br />
///     What each kind of token buys. Every entry maps an item name to the number of tokens it costs. A cost below 1
///     runs the other way: 0.01 means one token buys 100 of the item. A name may carry a <c>-suffix</c>, which the
///     server splits off and stores on the item it creates.
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class TokensDatum : DatumBase<IReadOnlyDictionary<string, float>>
{
    [JsonPropertyName("friendtoken")]
    public IReadOnlyDictionary<string, float> Friendtoken { get; init; } = null!;

    [JsonPropertyName("funtoken")]
    public IReadOnlyDictionary<string, float> Funtoken { get; init; } = null!;

    [JsonPropertyName("monstertoken")]
    public IReadOnlyDictionary<string, float> Monstertoken { get; init; } = null!;

    [JsonPropertyName("pvptoken")]
    public IReadOnlyDictionary<string, float> Pvptoken { get; init; } = null!;
}