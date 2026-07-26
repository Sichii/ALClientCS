#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Dismantle;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class DismantleDatum : DatumBase<Recipe>
{
    [JsonPropertyName("bowofthedead")]
    public Recipe Bowofthedead { get; init; } = null!;

    [JsonPropertyName("bronzeingot")]
    public Recipe Bronzeingot { get; init; } = null!;

    [JsonPropertyName("daggerofthedead")]
    public Recipe Daggerofthedead { get; init; } = null!;

    [JsonPropertyName("essenceoffire")]
    public Recipe Essenceoffire { get; init; } = null!;

    [JsonPropertyName("essenceoffrost")]
    public Recipe Essenceoffrost { get; init; } = null!;

    [JsonPropertyName("essenceoflife")]
    public Recipe Essenceoflife { get; init; } = null!;

    [JsonPropertyName("essenceofnature")]
    public Recipe Essenceofnature { get; init; } = null!;

    [JsonPropertyName("fireblade")]
    public Recipe Fireblade { get; init; } = null!;

    [JsonPropertyName("firebow")]
    public Recipe Firebow { get; init; } = null!;

    [JsonPropertyName("firestaff")]
    public Recipe Firestaff { get; init; } = null!;

    [JsonPropertyName("firestars")]
    public Recipe Firestars { get; init; } = null!;

    [JsonPropertyName("goldenegg")]
    public Recipe Goldenegg { get; init; } = null!;

    [JsonPropertyName("goldingot")]
    public Recipe Goldingot { get; init; } = null!;

    [JsonPropertyName("lostearring")]
    public Recipe Lostearring { get; init; } = null!;

    [JsonPropertyName("maceofthedead")]
    public Recipe Maceofthedead { get; init; } = null!;

    [JsonPropertyName("molesteeth")]
    public Recipe Molesteeth { get; init; } = null!;

    [JsonPropertyName("platinumingot")]
    public Recipe Platinumingot { get; init; } = null!;

    [JsonPropertyName("pmaceofthedead")]
    public Recipe Pmaceofthedead { get; init; } = null!;

    [JsonPropertyName("spearofthedead")]
    public Recipe Spearofthedead { get; init; } = null!;

    [JsonPropertyName("staffofthedead")]
    public Recipe Staffofthedead { get; init; } = null!;

    [JsonPropertyName("swordofthedead")]
    public Recipe Swordofthedead { get; init; } = null!;
}