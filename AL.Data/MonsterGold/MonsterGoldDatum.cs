#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.MonsterGold;

/// <summary>
///     The base gold a kill of each monster pays, keyed by monster. The server keeps this apart from the monster record
///     and only started sending it at game data version 16846. A monster with no entry pays nothing from this table: the
///     instance-only ones and the newest spawns are absent.
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class MonsterGoldDatum : DatumBase<int?>
{
    [JsonPropertyName("a1")]
    public int? A1 { get; init; }

    [JsonPropertyName("a2")]
    public int? A2 { get; init; }

    [JsonPropertyName("a3")]
    public int? A3 { get; init; }

    [JsonPropertyName("a4")]
    public int? A4 { get; init; }

    [JsonPropertyName("a5")]
    public int? A5 { get; init; }

    [JsonPropertyName("a6")]
    public int? A6 { get; init; }

    [JsonPropertyName("a7")]
    public int? A7 { get; init; }

    [JsonPropertyName("a8")]
    public int? A8 { get; init; }

    [JsonPropertyName("arcticbee")]
    public int? Arcticbee { get; init; }

    [JsonPropertyName("armadillo")]
    public int? Armadillo { get; init; }

    [JsonPropertyName("bat")]
    public int? Bat { get; init; }

    [JsonPropertyName("bbpompom")]
    public int? Bbpompom { get; init; }

    [JsonPropertyName("bee")]
    public int? Bee { get; init; }

    [JsonPropertyName("bgoo")]
    public int? Bgoo { get; init; }

    [JsonPropertyName("bigbird")]
    public int? Bigbird { get; init; }

    [JsonPropertyName("bluefairy")]
    public int? Bluefairy { get; init; }

    [JsonPropertyName("boar")]
    public int? Boar { get; init; }

    [JsonPropertyName("booboo")]
    public int? Booboo { get; init; }

    [JsonPropertyName("bscorpion")]
    public int? Bscorpion { get; init; }

    [JsonPropertyName("cgoo")]
    public int? Cgoo { get; init; }

    [JsonPropertyName("chestm")]
    public int? Chestm { get; init; }

    [JsonPropertyName("crab")]
    public int? Crab { get; init; }

    [JsonPropertyName("crabx")]
    public int? Crabx { get; init; }

    [JsonPropertyName("crabxx")]
    public int? Crabxx { get; init; }

    [JsonPropertyName("croc")]
    public int? Croc { get; init; }

    [JsonPropertyName("cutebee")]
    public int? Cutebee { get; init; }

    [JsonPropertyName("d_wiz")]
    public int? DWiz { get; init; }

    [JsonPropertyName("dknight2")]
    public int? Dknight2 { get; init; }

    [JsonPropertyName("dragold")]
    public int? Dragold { get; init; }

    [JsonPropertyName("dryad")]
    public int? Dryad { get; init; }

    [JsonPropertyName("eelemental")]
    public int? Eelemental { get; init; }

    [JsonPropertyName("ent")]
    public int? Ent { get; init; }

    [JsonPropertyName("felemental")]
    public int? Felemental { get; init; }

    [JsonPropertyName("fieldgen0")]
    public int? Fieldgen0 { get; init; }

    [JsonPropertyName("fireroamer")]
    public int? Fireroamer { get; init; }

    [JsonPropertyName("franky")]
    public int? Franky { get; init; }

    [JsonPropertyName("frog")]
    public int? Frog { get; init; }

    [JsonPropertyName("fvampire")]
    public int? Fvampire { get; init; }

    [JsonPropertyName("gbluepro")]
    public int? Gbluepro { get; init; }

    [JsonPropertyName("ggreenpro")]
    public int? Ggreenpro { get; init; }

    [JsonPropertyName("ghost")]
    public int? Ghost { get; init; }

    [JsonPropertyName("goblin")]
    public int? Goblin { get; init; }

    [JsonPropertyName("goldenbat")]
    public int? Goldenbat { get; init; }

    [JsonPropertyName("goldenbot")]
    public int? Goldenbot { get; init; }

    [JsonPropertyName("goo")]
    public int? Goo { get; init; }

    [JsonPropertyName("gpurplepro")]
    public int? Gpurplepro { get; init; }

    [JsonPropertyName("gredpro")]
    public int? Gredpro { get; init; }

    [JsonPropertyName("greenfairy")]
    public int? Greenfairy { get; init; }

    [JsonPropertyName("greenjr")]
    public int? Greenjr { get; init; }

    [JsonPropertyName("grinch")]
    public int? Grinch { get; init; }

    [JsonPropertyName("gscorpion")]
    public int? Gscorpion { get; init; }

    [JsonPropertyName("harpy")]
    public int? Harpy { get; init; }

    [JsonPropertyName("hen")]
    public int? Hen { get; init; }

    [JsonPropertyName("icegolem")]
    public int? Icegolem { get; init; }

    [JsonPropertyName("iceroamer")]
    public int? Iceroamer { get; init; }

    [JsonPropertyName("jr")]
    public int? Jr { get; init; }

    [JsonPropertyName("jrat")]
    public int? Jrat { get; init; }

    [JsonPropertyName("kitty1")]
    public int? Kitty1 { get; init; }

    [JsonPropertyName("kitty2")]
    public int? Kitty2 { get; init; }

    [JsonPropertyName("kitty3")]
    public int? Kitty3 { get; init; }

    [JsonPropertyName("kitty4")]
    public int? Kitty4 { get; init; }

    [JsonPropertyName("ligerx")]
    public int? Ligerx { get; init; }

    [JsonPropertyName("mechagnome")]
    public int? Mechagnome { get; init; }

    [JsonPropertyName("minimush")]
    public int? Minimush { get; init; }

    [JsonPropertyName("mole")]
    public int? Mole { get; init; }

    [JsonPropertyName("mrgreen")]
    public int? Mrgreen { get; init; }

    [JsonPropertyName("mrpumpkin")]
    public int? Mrpumpkin { get; init; }

    [JsonPropertyName("mummy")]
    public int? Mummy { get; init; }

    [JsonPropertyName("mvampire")]
    public int? Mvampire { get; init; }

    [JsonPropertyName("nelemental")]
    public int? Nelemental { get; init; }

    [JsonPropertyName("nerfedbat")]
    public int? Nerfedbat { get; init; }

    [JsonPropertyName("nerfedmummy")]
    public int? Nerfedmummy { get; init; }

    [JsonPropertyName("odino")]
    public int? Odino { get; init; }

    [JsonPropertyName("oneeye")]
    public int? Oneeye { get; init; }

    [JsonPropertyName("osnake")]
    public int? Osnake { get; init; }

    [JsonPropertyName("phoenix")]
    public int? Phoenix { get; init; }

    [JsonPropertyName("pinkgoblin")]
    public int? Pinkgoblin { get; init; }

    [JsonPropertyName("pinkgoo")]
    public int? Pinkgoo { get; init; }

    [JsonPropertyName("plantoid")]
    public int? Plantoid { get; init; }

    [JsonPropertyName("poisio")]
    public int? Poisio { get; init; }

    [JsonPropertyName("porcupine")]
    public int? Porcupine { get; init; }

    [JsonPropertyName("pppompom")]
    public int? Pppompom { get; init; }

    [JsonPropertyName("prat")]
    public int? Prat { get; init; }

    [JsonPropertyName("puppy1")]
    public int? Puppy1 { get; init; }

    [JsonPropertyName("puppy2")]
    public int? Puppy2 { get; init; }

    [JsonPropertyName("puppy3")]
    public int? Puppy3 { get; init; }

    [JsonPropertyName("puppy4")]
    public int? Puppy4 { get; init; }

    [JsonPropertyName("rat")]
    public int? Rat { get; init; }

    [JsonPropertyName("redfairy")]
    public int? Redfairy { get; init; }

    [JsonPropertyName("rgoo")]
    public int? Rgoo { get; init; }

    [JsonPropertyName("rharpy")]
    public int? Rharpy { get; init; }

    [JsonPropertyName("rimedjinn")]
    public int? Rimedjinn { get; init; }

    [JsonPropertyName("rooster")]
    public int? Rooster { get; init; }

    [JsonPropertyName("rudolph")]
    public int? Rudolph { get; init; }

    [JsonPropertyName("scorpion")]
    public int? Scorpion { get; init; }

    [JsonPropertyName("skeletor")]
    public int? Skeletor { get; init; }

    [JsonPropertyName("slenderman")]
    public int? Slenderman { get; init; }

    [JsonPropertyName("snake")]
    public int? Snake { get; init; }

    [JsonPropertyName("snowman")]
    public int? Snowman { get; init; }

    [JsonPropertyName("sparkbot")]
    public int? Sparkbot { get; init; }

    [JsonPropertyName("spider")]
    public int? Spider { get; init; }

    [JsonPropertyName("spiderbl")]
    public int? Spiderbl { get; init; }

    [JsonPropertyName("spiderbr")]
    public int? Spiderbr { get; init; }

    [JsonPropertyName("spiderr")]
    public int? Spiderr { get; init; }

    [JsonPropertyName("squig")]
    public int? Squig { get; init; }

    [JsonPropertyName("squigtoad")]
    public int? Squigtoad { get; init; }

    [JsonPropertyName("stompy")]
    public int? Stompy { get; init; }

    [JsonPropertyName("stoneworm")]
    public int? Stoneworm { get; init; }

    [JsonPropertyName("target")]
    public int? Target { get; init; }

    [JsonPropertyName("target_a500")]
    public int? TargetA500 { get; init; }

    [JsonPropertyName("target_a750")]
    public int? TargetA750 { get; init; }

    [JsonPropertyName("target_ar500red")]
    public int? TargetAr500Red { get; init; }

    [JsonPropertyName("target_ar900")]
    public int? TargetAr900 { get; init; }

    [JsonPropertyName("target_r500")]
    public int? TargetR500 { get; init; }

    [JsonPropertyName("target_r750")]
    public int? TargetR750 { get; init; }

    [JsonPropertyName("targetron")]
    public int? Targetron { get; init; }

    [JsonPropertyName("tiger")]
    public int? Tiger { get; init; }

    [JsonPropertyName("tinyp")]
    public int? Tinyp { get; init; }

    [JsonPropertyName("tortoise")]
    public int? Tortoise { get; init; }

    [JsonPropertyName("vbat")]
    public int? Vbat { get; init; }

    [JsonPropertyName("wabbit")]
    public int? Wabbit { get; init; }

    [JsonPropertyName("welemental")]
    public int? Welemental { get; init; }

    [JsonPropertyName("wolf")]
    public int? Wolf { get; init; }

    [JsonPropertyName("wolfie")]
    public int? Wolfie { get; init; }

    [JsonPropertyName("xmagefi")]
    public int? Xmagefi { get; init; }

    [JsonPropertyName("xmagefz")]
    public int? Xmagefz { get; init; }

    [JsonPropertyName("xmagen")]
    public int? Xmagen { get; init; }

    [JsonPropertyName("xmagex")]
    public int? Xmagex { get; init; }

    [JsonPropertyName("xscorpion")]
    public int? Xscorpion { get; init; }

    [JsonPropertyName("zapper0")]
    public int? Zapper0 { get; init; }
}