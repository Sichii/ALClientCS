#region
using System.Text.Json.Serialization;
using Newtonsoft.Json;
#endregion

namespace AL.Data.Dismantle
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class DismantleDatum : DatumBase<Recipe>
    {
        [JsonProperty("bowofthedead")]
        [JsonPropertyName("bowofthedead")]
        public Recipe Bowofthedead { get; init; } = null!;

        [JsonProperty("bronzeingot")]
        [JsonPropertyName("bronzeingot")]
        public Recipe Bronzeingot { get; init; } = null!;

        [JsonProperty("daggerofthedead")]
        [JsonPropertyName("daggerofthedead")]
        public Recipe Daggerofthedead { get; init; } = null!;

        [JsonProperty("essenceoffire")]
        [JsonPropertyName("essenceoffire")]
        public Recipe Essenceoffire { get; init; } = null!;

        [JsonProperty("essenceoffrost")]
        [JsonPropertyName("essenceoffrost")]
        public Recipe Essenceoffrost { get; init; } = null!;

        [JsonProperty("essenceoflife")]
        [JsonPropertyName("essenceoflife")]
        public Recipe Essenceoflife { get; init; } = null!;

        [JsonProperty("essenceofnature")]
        [JsonPropertyName("essenceofnature")]
        public Recipe Essenceofnature { get; init; } = null!;

        [JsonProperty("fireblade")]
        [JsonPropertyName("fireblade")]
        public Recipe Fireblade { get; init; } = null!;

        [JsonProperty("firebow")]
        [JsonPropertyName("firebow")]
        public Recipe Firebow { get; init; } = null!;

        [JsonProperty("firestaff")]
        [JsonPropertyName("firestaff")]
        public Recipe Firestaff { get; init; } = null!;

        [JsonProperty("firestars")]
        [JsonPropertyName("firestars")]
        public Recipe Firestars { get; init; } = null!;

        [JsonProperty("goldenegg")]
        [JsonPropertyName("goldenegg")]
        public Recipe Goldenegg { get; init; } = null!;

        [JsonProperty("goldingot")]
        [JsonPropertyName("goldingot")]
        public Recipe Goldingot { get; init; } = null!;

        [JsonProperty("lostearring")]
        [JsonPropertyName("lostearring")]
        public Recipe Lostearring { get; init; } = null!;

        [JsonProperty("maceofthedead")]
        [JsonPropertyName("maceofthedead")]
        public Recipe Maceofthedead { get; init; } = null!;

        [JsonProperty("molesteeth")]
        [JsonPropertyName("molesteeth")]
        public Recipe Molesteeth { get; init; } = null!;

        [JsonProperty("platinumingot")]
        [JsonPropertyName("platinumingot")]
        public Recipe Platinumingot { get; init; } = null!;

        [JsonProperty("pmaceofthedead")]
        [JsonPropertyName("pmaceofthedead")]
        public Recipe Pmaceofthedead { get; init; } = null!;

        [JsonProperty("spearofthedead")]
        [JsonPropertyName("spearofthedead")]
        public Recipe Spearofthedead { get; init; } = null!;

        [JsonProperty("staffofthedead")]
        [JsonPropertyName("staffofthedead")]
        public Recipe Staffofthedead { get; init; } = null!;

        [JsonProperty("swordofthedead")]
        [JsonPropertyName("swordofthedead")]
        public Recipe Swordofthedead { get; init; } = null!;
    }
}