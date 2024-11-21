#region
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
        public Recipe Bowofthedead { get; init; } = null!;

        [JsonProperty("bronzeingot")]
        public Recipe Bronzeingot { get; init; } = null!;

        [JsonProperty("daggerofthedead")]
        public Recipe Daggerofthedead { get; init; } = null!;

        [JsonProperty("essenceoffire")]
        public Recipe Essenceoffire { get; init; } = null!;

        [JsonProperty("essenceoffrost")]
        public Recipe Essenceoffrost { get; init; } = null!;

        [JsonProperty("essenceoflife")]
        public Recipe Essenceoflife { get; init; } = null!;

        [JsonProperty("essenceofnature")]
        public Recipe Essenceofnature { get; init; } = null!;

        [JsonProperty("fireblade")]
        public Recipe Fireblade { get; init; } = null!;

        [JsonProperty("firebow")]
        public Recipe Firebow { get; init; } = null!;

        [JsonProperty("firestaff")]
        public Recipe Firestaff { get; init; } = null!;

        [JsonProperty("firestars")]
        public Recipe Firestars { get; init; } = null!;

        [JsonProperty("goldenegg")]
        public Recipe Goldenegg { get; init; } = null!;

        [JsonProperty("goldingot")]
        public Recipe Goldingot { get; init; } = null!;

        [JsonProperty("lostearring")]
        public Recipe Lostearring { get; init; } = null!;

        [JsonProperty("maceofthedead")]
        public Recipe Maceofthedead { get; init; } = null!;

        [JsonProperty("molesteeth")]
        public Recipe Molesteeth { get; init; } = null!;

        [JsonProperty("platinumingot")]
        public Recipe Platinumingot { get; init; } = null!;

        [JsonProperty("pmaceofthedead")]
        public Recipe Pmaceofthedead { get; init; } = null!;

        [JsonProperty("spearofthedead")]
        public Recipe Spearofthedead { get; init; } = null!;

        [JsonProperty("staffofthedead")]
        public Recipe Staffofthedead { get; init; } = null!;

        [JsonProperty("swordofthedead")]
        public Recipe Swordofthedead { get; init; } = null!;
    }
}