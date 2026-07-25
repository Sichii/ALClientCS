#region
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
#endregion

namespace AL.Data.Dimensions
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class DimensionsDatum : DatumBase<IReadOnlyList<float>>
    {
        [JsonProperty("arcticbee")]
        [JsonPropertyName("arcticbee")]
        public IReadOnlyList<float> Arcticbee { get; init; } = null!;

        [JsonProperty("armadillo")]
        [JsonPropertyName("armadillo")]
        public IReadOnlyList<float> Armadillo { get; init; } = null!;

        [JsonProperty("bat")]
        [JsonPropertyName("bat")]
        public IReadOnlyList<float> Bat { get; init; } = null!;

        [JsonProperty("bbpompom")]
        [JsonPropertyName("bbpompom")]
        public IReadOnlyList<float> Bbpompom { get; init; } = null!;

        [JsonProperty("bee")]
        [JsonPropertyName("bee")]
        public IReadOnlyList<float> Bee { get; init; } = null!;

        [JsonProperty("bigbird")]
        [JsonPropertyName("bigbird")]
        public IReadOnlyList<float> Bigbird { get; init; } = null!;

        [JsonProperty("boar")]
        [JsonPropertyName("boar")]
        public IReadOnlyList<float> Boar { get; init; } = null!;

        [JsonProperty("bscorpion")]
        [JsonPropertyName("bscorpion")]
        public IReadOnlyList<float> Bscorpion { get; init; } = null!;

        [JsonProperty("cgoo")]
        [JsonPropertyName("cgoo")]
        public IReadOnlyList<float> Cgoo { get; init; } = null!;

        [JsonProperty("crabx")]
        [JsonPropertyName("crabx")]
        public IReadOnlyList<float> Crabx { get; init; } = null!;

        [JsonProperty("croc")]
        [JsonPropertyName("croc")]
        public IReadOnlyList<float> Croc { get; init; } = null!;

        [JsonProperty("default_character")]
        [JsonPropertyName("default_character")]
        public IReadOnlyList<float> DefaultCharacter { get; init; } = null!;

        [JsonProperty("dknight2")]
        [JsonPropertyName("dknight2")]
        public IReadOnlyList<float> Dknight2 { get; init; } = null!;

        [JsonProperty("ent")]
        [JsonPropertyName("ent")]
        public IReadOnlyList<float> Ent { get; init; } = null!;

        [JsonProperty("fireroamer")]
        [JsonPropertyName("fireroamer")]
        public IReadOnlyList<float> Fireroamer { get; init; } = null!;

        [JsonProperty("frog")]
        [JsonPropertyName("frog")]
        public IReadOnlyList<float> Frog { get; init; } = null!;

        [JsonProperty("ghost")]
        [JsonPropertyName("ghost")]
        public IReadOnlyList<float> Ghost { get; init; } = null!;

        [JsonProperty("goldenbat")]
        [JsonPropertyName("goldenbat")]
        public IReadOnlyList<float> Goldenbat { get; init; } = null!;

        [JsonProperty("goo")]
        [JsonPropertyName("goo")]
        public IReadOnlyList<float> Goo { get; init; } = null!;

        [JsonProperty("goo0")]
        [JsonPropertyName("goo0")]
        public IReadOnlyList<float> Goo0 { get; init; } = null!;

        [JsonProperty("goo1")]
        [JsonPropertyName("goo1")]
        public IReadOnlyList<float> Goo1 { get; init; } = null!;

        [JsonProperty("goo2")]
        [JsonPropertyName("goo2")]
        public IReadOnlyList<float> Goo2 { get; init; } = null!;

        [JsonProperty("goo3")]
        [JsonPropertyName("goo3")]
        public IReadOnlyList<float> Goo3 { get; init; } = null!;

        [JsonProperty("goo4")]
        [JsonPropertyName("goo4")]
        public IReadOnlyList<float> Goo4 { get; init; } = null!;

        [JsonProperty("goo5")]
        [JsonPropertyName("goo5")]
        public IReadOnlyList<float> Goo5 { get; init; } = null!;

        [JsonProperty("goo6")]
        [JsonPropertyName("goo6")]
        public IReadOnlyList<float> Goo6 { get; init; } = null!;

        [JsonProperty("goo7")]
        [JsonPropertyName("goo7")]
        public IReadOnlyList<float> Goo7 { get; init; } = null!;

        [JsonProperty("goo8")]
        [JsonPropertyName("goo8")]
        public IReadOnlyList<float> Goo8 { get; init; } = null!;

        [JsonProperty("gooD")]
        [JsonPropertyName("gooD")]
        public IReadOnlyList<float> GooD { get; init; } = null!;

        [JsonProperty("grinch")]
        [JsonPropertyName("grinch")]
        public IReadOnlyList<float> Grinch { get; init; } = null!;

        [JsonProperty("gscorpion")]
        [JsonPropertyName("gscorpion")]
        public IReadOnlyList<float> Gscorpion { get; init; } = null!;

        [JsonProperty("harpy")]
        [JsonPropertyName("harpy")]
        public IReadOnlyList<float> Harpy { get; init; } = null!;

        [JsonProperty("harpy_fly")]
        [JsonPropertyName("harpy_fly")]
        public IReadOnlyList<float> HarpyFly { get; init; } = null!;

        [JsonProperty("hen")]
        [JsonPropertyName("hen")]
        public IReadOnlyList<float> Hen { get; init; } = null!;

        [JsonProperty("iceroamer")]
        [JsonPropertyName("iceroamer")]
        public IReadOnlyList<float> Iceroamer { get; init; } = null!;

        [JsonProperty("jrat")]
        [JsonPropertyName("jrat")]
        public IReadOnlyList<float> Jrat { get; init; } = null!;

        [JsonProperty("kitty1")]
        [JsonPropertyName("kitty1")]
        public IReadOnlyList<float> Kitty1 { get; init; } = null!;

        [JsonProperty("kitty2")]
        [JsonPropertyName("kitty2")]
        public IReadOnlyList<float> Kitty2 { get; init; } = null!;

        [JsonProperty("kitty3")]
        [JsonPropertyName("kitty3")]
        public IReadOnlyList<float> Kitty3 { get; init; } = null!;

        [JsonProperty("kitty4")]
        [JsonPropertyName("kitty4")]
        public IReadOnlyList<float> Kitty4 { get; init; } = null!;

        [JsonProperty("mechagnome")]
        [JsonPropertyName("mechagnome")]
        public IReadOnlyList<float> Mechagnome { get; init; } = null!;

        [JsonProperty("minimush")]
        [JsonPropertyName("minimush")]
        public IReadOnlyList<float> Minimush { get; init; } = null!;

        [JsonProperty("mole")]
        [JsonPropertyName("mole")]
        public IReadOnlyList<float> Mole { get; init; } = null!;

        [JsonProperty("oneeye")]
        [JsonPropertyName("oneeye")]
        public IReadOnlyList<float> Oneeye { get; init; } = null!;

        [JsonProperty("osnake")]
        [JsonPropertyName("osnake")]
        public IReadOnlyList<float> Osnake { get; init; } = null!;

        [JsonProperty("phoenix")]
        [JsonPropertyName("phoenix")]
        public IReadOnlyList<float> Phoenix { get; init; } = null!;

        [JsonProperty("pinkgoblin")]
        [JsonPropertyName("pinkgoblin")]
        public IReadOnlyList<float> Pinkgoblin { get; init; } = null!;

        [JsonProperty("pinkgoo")]
        [JsonPropertyName("pinkgoo")]
        public IReadOnlyList<float> Pinkgoo { get; init; } = null!;

        [JsonProperty("plantoid")]
        [JsonPropertyName("plantoid")]
        public IReadOnlyList<float> Plantoid { get; init; } = null!;

        [JsonProperty("poisio")]
        [JsonPropertyName("poisio")]
        public IReadOnlyList<float> Poisio { get; init; } = null!;

        [JsonProperty("pppompom")]
        [JsonPropertyName("pppompom")]
        public IReadOnlyList<float> Pppompom { get; init; } = null!;

        [JsonProperty("prat")]
        [JsonPropertyName("prat")]
        public IReadOnlyList<float> Prat { get; init; } = null!;

        [JsonProperty("puppy1")]
        [JsonPropertyName("puppy1")]
        public IReadOnlyList<float> Puppy1 { get; init; } = null!;

        [JsonProperty("puppy2")]
        [JsonPropertyName("puppy2")]
        public IReadOnlyList<float> Puppy2 { get; init; } = null!;

        [JsonProperty("puppy3")]
        [JsonPropertyName("puppy3")]
        public IReadOnlyList<float> Puppy3 { get; init; } = null!;

        [JsonProperty("puppy4")]
        [JsonPropertyName("puppy4")]
        public IReadOnlyList<float> Puppy4 { get; init; } = null!;

        [JsonProperty("rat")]
        [JsonPropertyName("rat")]
        public IReadOnlyList<float> Rat { get; init; } = null!;

        [JsonProperty("rharpy")]
        [JsonPropertyName("rharpy")]
        public IReadOnlyList<float> Rharpy { get; init; } = null!;

        [JsonProperty("rharpy_fly")]
        [JsonPropertyName("rharpy_fly")]
        public IReadOnlyList<float> RharpyFly { get; init; } = null!;

        [JsonProperty("rooster")]
        [JsonPropertyName("rooster")]
        public IReadOnlyList<float> Rooster { get; init; } = null!;

        [JsonProperty("rudolph")]
        [JsonPropertyName("rudolph")]
        public IReadOnlyList<float> Rudolph { get; init; } = null!;

        [JsonProperty("scorpion")]
        [JsonPropertyName("scorpion")]
        public IReadOnlyList<float> Scorpion { get; init; } = null!;

        [JsonProperty("skeletor")]
        [JsonPropertyName("skeletor")]
        public IReadOnlyList<float> Skeletor { get; init; } = null!;

        [JsonProperty("snake")]
        [JsonPropertyName("snake")]
        public IReadOnlyList<float> Snake { get; init; } = null!;

        [JsonProperty("spider")]
        [JsonPropertyName("spider")]
        public IReadOnlyList<float> Spider { get; init; } = null!;

        [JsonProperty("squig")]
        [JsonPropertyName("squig")]
        public IReadOnlyList<float> Squig { get; init; } = null!;

        [JsonProperty("squigtoad")]
        [JsonPropertyName("squigtoad")]
        public IReadOnlyList<float> Squigtoad { get; init; } = null!;

        [JsonProperty("stoneworm")]
        [JsonPropertyName("stoneworm")]
        public IReadOnlyList<float> Stoneworm { get; init; } = null!;

        [JsonProperty("tiger")]
        [JsonPropertyName("tiger")]
        public IReadOnlyList<float> Tiger { get; init; } = null!;

        [JsonProperty("tortoise")]
        [JsonPropertyName("tortoise")]
        public IReadOnlyList<float> Tortoise { get; init; } = null!;

        [JsonProperty("wabbit")]
        [JsonPropertyName("wabbit")]
        public IReadOnlyList<float> Wabbit { get; init; } = null!;

        [JsonProperty("wolf")]
        [JsonPropertyName("wolf")]
        public IReadOnlyList<float> Wolf { get; init; } = null!;

        [JsonProperty("wolfie")]
        [JsonPropertyName("wolfie")]
        public IReadOnlyList<float> Wolfie { get; init; } = null!;

        [JsonProperty("xscorpion")]
        [JsonPropertyName("xscorpion")]
        public IReadOnlyList<float> Xscorpion { get; init; } = null!;
    }
}