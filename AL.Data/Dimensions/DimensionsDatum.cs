#region
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Dimensions;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class DimensionsDatum : DatumBase<IReadOnlyList<float>>
{
    [JsonPropertyName("arcticbee")]
    public IReadOnlyList<float> Arcticbee { get; init; } = null!;

    [JsonPropertyName("armadillo")]
    public IReadOnlyList<float> Armadillo { get; init; } = null!;

    [JsonPropertyName("bat")]
    public IReadOnlyList<float> Bat { get; init; } = null!;

    [JsonPropertyName("bbpompom")]
    public IReadOnlyList<float> Bbpompom { get; init; } = null!;

    [JsonPropertyName("bee")]
    public IReadOnlyList<float> Bee { get; init; } = null!;

    [JsonPropertyName("bigbird")]
    public IReadOnlyList<float> Bigbird { get; init; } = null!;

    [JsonPropertyName("boar")]
    public IReadOnlyList<float> Boar { get; init; } = null!;

    [JsonPropertyName("bscorpion")]
    public IReadOnlyList<float> Bscorpion { get; init; } = null!;

    [JsonPropertyName("cgoo")]
    public IReadOnlyList<float> Cgoo { get; init; } = null!;

    [JsonPropertyName("crabx")]
    public IReadOnlyList<float> Crabx { get; init; } = null!;

    [JsonPropertyName("croc")]
    public IReadOnlyList<float> Croc { get; init; } = null!;

    [JsonPropertyName("default_character")]
    public IReadOnlyList<float> DefaultCharacter { get; init; } = null!;

    [JsonPropertyName("dknight2")]
    public IReadOnlyList<float> Dknight2 { get; init; } = null!;

    [JsonPropertyName("ent")]
    public IReadOnlyList<float> Ent { get; init; } = null!;

    [JsonPropertyName("fireroamer")]
    public IReadOnlyList<float> Fireroamer { get; init; } = null!;

    [JsonPropertyName("frog")]
    public IReadOnlyList<float> Frog { get; init; } = null!;

    [JsonPropertyName("ghost")]
    public IReadOnlyList<float> Ghost { get; init; } = null!;

    [JsonPropertyName("goldenbat")]
    public IReadOnlyList<float> Goldenbat { get; init; } = null!;

    [JsonPropertyName("goo")]
    public IReadOnlyList<float> Goo { get; init; } = null!;

    [JsonPropertyName("goo0")]
    public IReadOnlyList<float> Goo0 { get; init; } = null!;

    [JsonPropertyName("goo1")]
    public IReadOnlyList<float> Goo1 { get; init; } = null!;

    [JsonPropertyName("goo2")]
    public IReadOnlyList<float> Goo2 { get; init; } = null!;

    [JsonPropertyName("goo3")]
    public IReadOnlyList<float> Goo3 { get; init; } = null!;

    [JsonPropertyName("goo4")]
    public IReadOnlyList<float> Goo4 { get; init; } = null!;

    [JsonPropertyName("goo5")]
    public IReadOnlyList<float> Goo5 { get; init; } = null!;

    [JsonPropertyName("goo6")]
    public IReadOnlyList<float> Goo6 { get; init; } = null!;

    [JsonPropertyName("goo7")]
    public IReadOnlyList<float> Goo7 { get; init; } = null!;

    [JsonPropertyName("goo8")]
    public IReadOnlyList<float> Goo8 { get; init; } = null!;

    [JsonPropertyName("gooD")]
    public IReadOnlyList<float> GooD { get; init; } = null!;

    [JsonPropertyName("grinch")]
    public IReadOnlyList<float> Grinch { get; init; } = null!;

    [JsonPropertyName("gscorpion")]
    public IReadOnlyList<float> Gscorpion { get; init; } = null!;

    [JsonPropertyName("harpy")]
    public IReadOnlyList<float> Harpy { get; init; } = null!;

    [JsonPropertyName("harpy_fly")]
    public IReadOnlyList<float> HarpyFly { get; init; } = null!;

    [JsonPropertyName("hen")]
    public IReadOnlyList<float> Hen { get; init; } = null!;

    [JsonPropertyName("iceroamer")]
    public IReadOnlyList<float> Iceroamer { get; init; } = null!;

    [JsonPropertyName("jrat")]
    public IReadOnlyList<float> Jrat { get; init; } = null!;

    [JsonPropertyName("kitty1")]
    public IReadOnlyList<float> Kitty1 { get; init; } = null!;

    [JsonPropertyName("kitty2")]
    public IReadOnlyList<float> Kitty2 { get; init; } = null!;

    [JsonPropertyName("kitty3")]
    public IReadOnlyList<float> Kitty3 { get; init; } = null!;

    [JsonPropertyName("kitty4")]
    public IReadOnlyList<float> Kitty4 { get; init; } = null!;

    [JsonPropertyName("mechagnome")]
    public IReadOnlyList<float> Mechagnome { get; init; } = null!;

    [JsonPropertyName("minimush")]
    public IReadOnlyList<float> Minimush { get; init; } = null!;

    [JsonPropertyName("mole")]
    public IReadOnlyList<float> Mole { get; init; } = null!;

    [JsonPropertyName("oneeye")]
    public IReadOnlyList<float> Oneeye { get; init; } = null!;

    [JsonPropertyName("osnake")]
    public IReadOnlyList<float> Osnake { get; init; } = null!;

    [JsonPropertyName("phoenix")]
    public IReadOnlyList<float> Phoenix { get; init; } = null!;

    [JsonPropertyName("pinkgoblin")]
    public IReadOnlyList<float> Pinkgoblin { get; init; } = null!;

    [JsonPropertyName("pinkgoo")]
    public IReadOnlyList<float> Pinkgoo { get; init; } = null!;

    [JsonPropertyName("plantoid")]
    public IReadOnlyList<float> Plantoid { get; init; } = null!;

    [JsonPropertyName("poisio")]
    public IReadOnlyList<float> Poisio { get; init; } = null!;

    [JsonPropertyName("pppompom")]
    public IReadOnlyList<float> Pppompom { get; init; } = null!;

    [JsonPropertyName("prat")]
    public IReadOnlyList<float> Prat { get; init; } = null!;

    [JsonPropertyName("puppy1")]
    public IReadOnlyList<float> Puppy1 { get; init; } = null!;

    [JsonPropertyName("puppy2")]
    public IReadOnlyList<float> Puppy2 { get; init; } = null!;

    [JsonPropertyName("puppy3")]
    public IReadOnlyList<float> Puppy3 { get; init; } = null!;

    [JsonPropertyName("puppy4")]
    public IReadOnlyList<float> Puppy4 { get; init; } = null!;

    [JsonPropertyName("rat")]
    public IReadOnlyList<float> Rat { get; init; } = null!;

    [JsonPropertyName("rharpy")]
    public IReadOnlyList<float> Rharpy { get; init; } = null!;

    [JsonPropertyName("rharpy_fly")]
    public IReadOnlyList<float> RharpyFly { get; init; } = null!;

    [JsonPropertyName("rooster")]
    public IReadOnlyList<float> Rooster { get; init; } = null!;

    [JsonPropertyName("rudolph")]
    public IReadOnlyList<float> Rudolph { get; init; } = null!;

    [JsonPropertyName("scorpion")]
    public IReadOnlyList<float> Scorpion { get; init; } = null!;

    [JsonPropertyName("skeletor")]
    public IReadOnlyList<float> Skeletor { get; init; } = null!;

    [JsonPropertyName("snake")]
    public IReadOnlyList<float> Snake { get; init; } = null!;

    [JsonPropertyName("spider")]
    public IReadOnlyList<float> Spider { get; init; } = null!;

    [JsonPropertyName("squig")]
    public IReadOnlyList<float> Squig { get; init; } = null!;

    [JsonPropertyName("squigtoad")]
    public IReadOnlyList<float> Squigtoad { get; init; } = null!;

    [JsonPropertyName("stoneworm")]
    public IReadOnlyList<float> Stoneworm { get; init; } = null!;

    [JsonPropertyName("tiger")]
    public IReadOnlyList<float> Tiger { get; init; } = null!;

    [JsonPropertyName("tortoise")]
    public IReadOnlyList<float> Tortoise { get; init; } = null!;

    [JsonPropertyName("wabbit")]
    public IReadOnlyList<float> Wabbit { get; init; } = null!;

    [JsonPropertyName("wolf")]
    public IReadOnlyList<float> Wolf { get; init; } = null!;

    [JsonPropertyName("wolfie")]
    public IReadOnlyList<float> Wolfie { get; init; } = null!;

    [JsonPropertyName("xscorpion")]
    public IReadOnlyList<float> Xscorpion { get; init; } = null!;
}