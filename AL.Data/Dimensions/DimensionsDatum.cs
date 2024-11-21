#region
using System.Collections.Generic;
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
        public IReadOnlyList<float> Arcticbee { get; init; } = null!;

        [JsonProperty("armadillo")]
        public IReadOnlyList<float> Armadillo { get; init; } = null!;

        [JsonProperty("bat")]
        public IReadOnlyList<float> Bat { get; init; } = null!;

        [JsonProperty("bbpompom")]
        public IReadOnlyList<float> Bbpompom { get; init; } = null!;

        [JsonProperty("bee")]
        public IReadOnlyList<float> Bee { get; init; } = null!;

        [JsonProperty("bigbird")]
        public IReadOnlyList<float> Bigbird { get; init; } = null!;

        [JsonProperty("boar")]
        public IReadOnlyList<float> Boar { get; init; } = null!;

        [JsonProperty("bscorpion")]
        public IReadOnlyList<float> Bscorpion { get; init; } = null!;

        [JsonProperty("cgoo")]
        public IReadOnlyList<float> Cgoo { get; init; } = null!;

        [JsonProperty("crabx")]
        public IReadOnlyList<float> Crabx { get; init; } = null!;

        [JsonProperty("croc")]
        public IReadOnlyList<float> Croc { get; init; } = null!;

        [JsonProperty("default_character")]
        public IReadOnlyList<float> DefaultCharacter { get; init; } = null!;

        [JsonProperty("dknight2")]
        public IReadOnlyList<float> Dknight2 { get; init; } = null!;

        [JsonProperty("ent")]
        public IReadOnlyList<float> Ent { get; init; } = null!;

        [JsonProperty("fireroamer")]
        public IReadOnlyList<float> Fireroamer { get; init; } = null!;

        [JsonProperty("frog")]
        public IReadOnlyList<float> Frog { get; init; } = null!;

        [JsonProperty("ghost")]
        public IReadOnlyList<float> Ghost { get; init; } = null!;

        [JsonProperty("goldenbat")]
        public IReadOnlyList<float> Goldenbat { get; init; } = null!;

        [JsonProperty("goo")]
        public IReadOnlyList<float> Goo { get; init; } = null!;

        [JsonProperty("goo0")]
        public IReadOnlyList<float> Goo0 { get; init; } = null!;

        [JsonProperty("goo1")]
        public IReadOnlyList<float> Goo1 { get; init; } = null!;

        [JsonProperty("goo2")]
        public IReadOnlyList<float> Goo2 { get; init; } = null!;

        [JsonProperty("goo3")]
        public IReadOnlyList<float> Goo3 { get; init; } = null!;

        [JsonProperty("goo4")]
        public IReadOnlyList<float> Goo4 { get; init; } = null!;

        [JsonProperty("goo5")]
        public IReadOnlyList<float> Goo5 { get; init; } = null!;

        [JsonProperty("goo6")]
        public IReadOnlyList<float> Goo6 { get; init; } = null!;

        [JsonProperty("goo7")]
        public IReadOnlyList<float> Goo7 { get; init; } = null!;

        [JsonProperty("goo8")]
        public IReadOnlyList<float> Goo8 { get; init; } = null!;

        [JsonProperty("gooD")]
        public IReadOnlyList<float> GooD { get; init; } = null!;

        [JsonProperty("grinch")]
        public IReadOnlyList<float> Grinch { get; init; } = null!;

        [JsonProperty("gscorpion")]
        public IReadOnlyList<float> Gscorpion { get; init; } = null!;

        [JsonProperty("harpy")]
        public IReadOnlyList<float> Harpy { get; init; } = null!;

        [JsonProperty("harpy_fly")]
        public IReadOnlyList<float> HarpyFly { get; init; } = null!;

        [JsonProperty("hen")]
        public IReadOnlyList<float> Hen { get; init; } = null!;

        [JsonProperty("iceroamer")]
        public IReadOnlyList<float> Iceroamer { get; init; } = null!;

        [JsonProperty("jrat")]
        public IReadOnlyList<float> Jrat { get; init; } = null!;

        [JsonProperty("kitty1")]
        public IReadOnlyList<float> Kitty1 { get; init; } = null!;

        [JsonProperty("kitty2")]
        public IReadOnlyList<float> Kitty2 { get; init; } = null!;

        [JsonProperty("kitty3")]
        public IReadOnlyList<float> Kitty3 { get; init; } = null!;

        [JsonProperty("kitty4")]
        public IReadOnlyList<float> Kitty4 { get; init; } = null!;

        [JsonProperty("mechagnome")]
        public IReadOnlyList<float> Mechagnome { get; init; } = null!;

        [JsonProperty("minimush")]
        public IReadOnlyList<float> Minimush { get; init; } = null!;

        [JsonProperty("mole")]
        public IReadOnlyList<float> Mole { get; init; } = null!;

        [JsonProperty("oneeye")]
        public IReadOnlyList<float> Oneeye { get; init; } = null!;

        [JsonProperty("osnake")]
        public IReadOnlyList<float> Osnake { get; init; } = null!;

        [JsonProperty("phoenix")]
        public IReadOnlyList<float> Phoenix { get; init; } = null!;

        [JsonProperty("pinkgoblin")]
        public IReadOnlyList<float> Pinkgoblin { get; init; } = null!;

        [JsonProperty("pinkgoo")]
        public IReadOnlyList<float> Pinkgoo { get; init; } = null!;

        [JsonProperty("plantoid")]
        public IReadOnlyList<float> Plantoid { get; init; } = null!;

        [JsonProperty("poisio")]
        public IReadOnlyList<float> Poisio { get; init; } = null!;

        [JsonProperty("pppompom")]
        public IReadOnlyList<float> Pppompom { get; init; } = null!;

        [JsonProperty("prat")]
        public IReadOnlyList<float> Prat { get; init; } = null!;

        [JsonProperty("puppy1")]
        public IReadOnlyList<float> Puppy1 { get; init; } = null!;

        [JsonProperty("puppy2")]
        public IReadOnlyList<float> Puppy2 { get; init; } = null!;

        [JsonProperty("puppy3")]
        public IReadOnlyList<float> Puppy3 { get; init; } = null!;

        [JsonProperty("puppy4")]
        public IReadOnlyList<float> Puppy4 { get; init; } = null!;

        [JsonProperty("rat")]
        public IReadOnlyList<float> Rat { get; init; } = null!;

        [JsonProperty("rharpy")]
        public IReadOnlyList<float> Rharpy { get; init; } = null!;

        [JsonProperty("rharpy_fly")]
        public IReadOnlyList<float> RharpyFly { get; init; } = null!;

        [JsonProperty("rooster")]
        public IReadOnlyList<float> Rooster { get; init; } = null!;

        [JsonProperty("rudolph")]
        public IReadOnlyList<float> Rudolph { get; init; } = null!;

        [JsonProperty("scorpion")]
        public IReadOnlyList<float> Scorpion { get; init; } = null!;

        [JsonProperty("skeletor")]
        public IReadOnlyList<float> Skeletor { get; init; } = null!;

        [JsonProperty("snake")]
        public IReadOnlyList<float> Snake { get; init; } = null!;

        [JsonProperty("spider")]
        public IReadOnlyList<float> Spider { get; init; } = null!;

        [JsonProperty("squig")]
        public IReadOnlyList<float> Squig { get; init; } = null!;

        [JsonProperty("squigtoad")]
        public IReadOnlyList<float> Squigtoad { get; init; } = null!;

        [JsonProperty("stoneworm")]
        public IReadOnlyList<float> Stoneworm { get; init; } = null!;

        [JsonProperty("tiger")]
        public IReadOnlyList<float> Tiger { get; init; } = null!;

        [JsonProperty("tortoise")]
        public IReadOnlyList<float> Tortoise { get; init; } = null!;

        [JsonProperty("wabbit")]
        public IReadOnlyList<float> Wabbit { get; init; } = null!;

        [JsonProperty("wolf")]
        public IReadOnlyList<float> Wolf { get; init; } = null!;

        [JsonProperty("wolfie")]
        public IReadOnlyList<float> Wolfie { get; init; } = null!;

        [JsonProperty("xscorpion")]
        public IReadOnlyList<float> Xscorpion { get; init; } = null!;
    }
}