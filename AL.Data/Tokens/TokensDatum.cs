#region
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Tokens;

/// <summary>
///     <inheritdoc />
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