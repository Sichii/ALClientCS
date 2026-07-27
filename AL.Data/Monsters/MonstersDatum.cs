#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Monsters;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class MonstersDatum : DatumBase<GMonster>
{
    [JsonPropertyName("a1")]
    public GMonster A1 { get; init; } = null!;

    [JsonPropertyName("a2")]
    public GMonster A2 { get; init; } = null!;

    [JsonPropertyName("a3")]
    public GMonster A3 { get; init; } = null!;

    [JsonPropertyName("a4")]
    public GMonster A4 { get; init; } = null!;

    [JsonPropertyName("a5")]
    public GMonster A5 { get; init; } = null!;

    [JsonPropertyName("a6")]
    public GMonster A6 { get; init; } = null!;

    [JsonPropertyName("a7")]
    public GMonster A7 { get; init; } = null!;

    [JsonPropertyName("a8")]
    public GMonster A8 { get; init; } = null!;

    [JsonPropertyName("arcticbee")]
    public GMonster Arcticbee { get; init; } = null!;

    [JsonPropertyName("armadillo")]
    public GMonster Armadillo { get; init; } = null!;

    [JsonPropertyName("bat")]
    public GMonster Bat { get; init; } = null!;

    [JsonPropertyName("bbpompom")]
    public GMonster Bbpompom { get; init; } = null!;

    [JsonPropertyName("bee")]
    public GMonster Bee { get; init; } = null!;

    [JsonPropertyName("bgoo")]
    public GMonster Bgoo { get; init; } = null!;

    [JsonPropertyName("bigbird")]
    public GMonster Bigbird { get; init; } = null!;

    [JsonPropertyName("bluefairy")]
    public GMonster Bluefairy { get; init; } = null!;

    [JsonPropertyName("boar")]
    public GMonster Boar { get; init; } = null!;

    [JsonPropertyName("booboo")]
    public GMonster Booboo { get; init; } = null!;

    [JsonPropertyName("bscorpion")]
    public GMonster Bscorpion { get; init; } = null!;

    [JsonPropertyName("cgoo")]
    public GMonster Cgoo { get; init; } = null!;

    [JsonPropertyName("chestm")]
    public GMonster Chestm { get; init; } = null!;

    [JsonPropertyName("crab")]
    public GMonster Crab { get; init; } = null!;

    [JsonPropertyName("crabx")]
    public GMonster Crabx { get; init; } = null!;

    [JsonPropertyName("crabxx")]
    public GMonster Crabxx { get; init; } = null!;

    [JsonPropertyName("croc")]
    public GMonster Croc { get; init; } = null!;

    [JsonPropertyName("cutebee")]
    public GMonster Cutebee { get; init; } = null!;

    [JsonPropertyName("dknight2")]
    public GMonster Dknight2 { get; init; } = null!;

    [JsonPropertyName("dragold")]
    public GMonster Dragold { get; init; } = null!;

    [JsonPropertyName("dryad")]
    public GMonster Dryad { get; init; } = null!;

    [JsonPropertyName("d_wiz")]
    public GMonster DWiz { get; init; } = null!;

    [JsonPropertyName("eelemental")]
    public GMonster Eelemental { get; init; } = null!;

    [JsonPropertyName("ent")]
    public GMonster Ent { get; init; } = null!;

    [JsonPropertyName("felemental")]
    public GMonster Felemental { get; init; } = null!;

    [JsonPropertyName("fieldgen0")]
    public GMonster Fieldgen0 { get; init; } = null!;

    [JsonPropertyName("fireroamer")]
    public GMonster Fireroamer { get; init; } = null!;

    [JsonPropertyName("franky")]
    public GMonster Franky { get; init; } = null!;

    [JsonPropertyName("frog")]
    public GMonster Frog { get; init; } = null!;

    [JsonPropertyName("fvampire")]
    public GMonster Fvampire { get; init; } = null!;

    [JsonPropertyName("gbluepro")]
    public GMonster Gbluepro { get; init; } = null!;

    [JsonPropertyName("ggreenpro")]
    public GMonster Ggreenpro { get; init; } = null!;

    [JsonPropertyName("ghost")]
    public GMonster Ghost { get; init; } = null!;

    [JsonPropertyName("goblin")]
    public GMonster Goblin { get; init; } = null!;

    [JsonPropertyName("goldenbat")]
    public GMonster Goldenbat { get; init; } = null!;

    [JsonPropertyName("goldenbot")]
    public GMonster Goldenbot { get; init; } = null!;

    [JsonPropertyName("goo")]
    public GMonster Goo { get; init; } = null!;

    [JsonPropertyName("gpurplepro")]
    public GMonster Gpurplepro { get; init; } = null!;

    [JsonPropertyName("gredpro")]
    public GMonster Gredpro { get; init; } = null!;

    [JsonPropertyName("greenfairy")]
    public GMonster Greenfairy { get; init; } = null!;

    [JsonPropertyName("greenjr")]
    public GMonster Greenjr { get; init; } = null!;

    [JsonPropertyName("grinch")]
    public GMonster Grinch { get; init; } = null!;

    [JsonPropertyName("gscorpion")]
    public GMonster Gscorpion { get; init; } = null!;

    [JsonPropertyName("harpy")]
    public GMonster Harpy { get; init; } = null!;

    [JsonPropertyName("hen")]
    public GMonster Hen { get; init; } = null!;

    [JsonPropertyName("icegolem")]
    public GMonster Icegolem { get; init; } = null!;

    [JsonPropertyName("iceroamer")]
    public GMonster Iceroamer { get; init; } = null!;

    [JsonPropertyName("jr")]
    public GMonster Jr { get; init; } = null!;

    [JsonPropertyName("jrat")]
    public GMonster Jrat { get; init; } = null!;

    [JsonPropertyName("kitty1")]
    public GMonster Kitty1 { get; init; } = null!;

    [JsonPropertyName("kitty2")]
    public GMonster Kitty2 { get; init; } = null!;

    [JsonPropertyName("kitty3")]
    public GMonster Kitty3 { get; init; } = null!;

    [JsonPropertyName("kitty4")]
    public GMonster Kitty4 { get; init; } = null!;

    [JsonPropertyName("ligerx")]
    public GMonster Ligerx { get; init; } = null!;

    [JsonPropertyName("mechagnome")]
    public GMonster Mechagnome { get; init; } = null!;

    [JsonPropertyName("minimush")]
    public GMonster Minimush { get; init; } = null!;

    [JsonPropertyName("mole")]
    public GMonster Mole { get; init; } = null!;

    [JsonPropertyName("mrgreen")]
    public GMonster Mrgreen { get; init; } = null!;

    [JsonPropertyName("mrpumpkin")]
    public GMonster Mrpumpkin { get; init; } = null!;

    [JsonPropertyName("mummy")]
    public GMonster Mummy { get; init; } = null!;

    [JsonPropertyName("mvampire")]
    public GMonster Mvampire { get; init; } = null!;

    [JsonPropertyName("nelemental")]
    public GMonster Nelemental { get; init; } = null!;

    [JsonPropertyName("nerfedbat")]
    public GMonster Nerfedbat { get; init; } = null!;

    [JsonPropertyName("nerfedmummy")]
    public GMonster Nerfedmummy { get; init; } = null!;

    [JsonPropertyName("odino")]
    public GMonster Odino { get; init; } = null!;

    [JsonPropertyName("oneeye")]
    public GMonster Oneeye { get; init; } = null!;

    [JsonPropertyName("osnake")]
    public GMonster Osnake { get; init; } = null!;

    [JsonPropertyName("phoenix")]
    public GMonster Phoenix { get; init; } = null!;

    [JsonPropertyName("pinkgoblin")]
    public GMonster Pinkgoblin { get; init; } = null!;

    [JsonPropertyName("pinkgoo")]
    public GMonster Pinkgoo { get; init; } = null!;

    [JsonPropertyName("plantoid")]
    public GMonster Plantoid { get; init; } = null!;

    [JsonPropertyName("poisio")]
    public GMonster Poisio { get; init; } = null!;

    [JsonPropertyName("porcupine")]
    public GMonster Porcupine { get; init; } = null!;

    [JsonPropertyName("pppompom")]
    public GMonster Pppompom { get; init; } = null!;

    [JsonPropertyName("prat")]
    public GMonster Prat { get; init; } = null!;

    [JsonPropertyName("puppy1")]
    public GMonster Puppy1 { get; init; } = null!;

    [JsonPropertyName("puppy2")]
    public GMonster Puppy2 { get; init; } = null!;

    [JsonPropertyName("puppy3")]
    public GMonster Puppy3 { get; init; } = null!;

    [JsonPropertyName("puppy4")]
    public GMonster Puppy4 { get; init; } = null!;

    [JsonPropertyName("rat")]
    public GMonster Rat { get; init; } = null!;

    [JsonPropertyName("redfairy")]
    public GMonster Redfairy { get; init; } = null!;

    [JsonPropertyName("rgoo")]
    public GMonster Rgoo { get; init; } = null!;

    [JsonPropertyName("rharpy")]
    public GMonster Rharpy { get; init; } = null!;

    [JsonPropertyName("rooster")]
    public GMonster Rooster { get; init; } = null!;

    [JsonPropertyName("rudolph")]
    public GMonster Rudolph { get; init; } = null!;

    [JsonPropertyName("scorpion")]
    public GMonster Scorpion { get; init; } = null!;

    [JsonPropertyName("skeletor")]
    public GMonster Skeletor { get; init; } = null!;

    [JsonPropertyName("slenderman")]
    public GMonster Slenderman { get; init; } = null!;

    [JsonPropertyName("snake")]
    public GMonster Snake { get; init; } = null!;

    [JsonPropertyName("snowman")]
    public GMonster Snowman { get; init; } = null!;

    [JsonPropertyName("sparkbot")]
    public GMonster Sparkbot { get; init; } = null!;

    [JsonPropertyName("spider")]
    public GMonster Spider { get; init; } = null!;

    [JsonPropertyName("spiderbl")]
    public GMonster Spiderbl { get; init; } = null!;

    [JsonPropertyName("spiderbr")]
    public GMonster Spiderbr { get; init; } = null!;

    [JsonPropertyName("spiderr")]
    public GMonster Spiderr { get; init; } = null!;

    [JsonPropertyName("squig")]
    public GMonster Squig { get; init; } = null!;

    [JsonPropertyName("squigtoad")]
    public GMonster Squigtoad { get; init; } = null!;

    [JsonPropertyName("stompy")]
    public GMonster Stompy { get; init; } = null!;

    [JsonPropertyName("stoneworm")]
    public GMonster Stoneworm { get; init; } = null!;

    [JsonPropertyName("target")]
    public GMonster Target { get; init; } = null!;

    [JsonPropertyName("target_a500")]
    public GMonster TargetA500 { get; init; } = null!;

    [JsonPropertyName("target_a750")]
    public GMonster TargetA750 { get; init; } = null!;

    [JsonPropertyName("target_ar500red")]
    public GMonster TargetAr500Red { get; init; } = null!;

    [JsonPropertyName("target_ar900")]
    public GMonster TargetAr900 { get; init; } = null!;

    [JsonPropertyName("target_r500")]
    public GMonster TargetR500 { get; init; } = null!;

    [JsonPropertyName("target_r750")]
    public GMonster TargetR750 { get; init; } = null!;

    [JsonPropertyName("targetron")]
    public GMonster Targetron { get; init; } = null!;

    [JsonPropertyName("tiger")]
    public GMonster Tiger { get; init; } = null!;

    [JsonPropertyName("tinyp")]
    public GMonster Tinyp { get; init; } = null!;

    [JsonPropertyName("tortoise")]
    public GMonster Tortoise { get; init; } = null!;

    [JsonPropertyName("vbat")]
    public GMonster Vbat { get; init; } = null!;

    [JsonPropertyName("wabbit")]
    public GMonster Wabbit { get; init; } = null!;

    [JsonPropertyName("welemental")]
    public GMonster Welemental { get; init; } = null!;

    [JsonPropertyName("wolf")]
    public GMonster Wolf { get; init; } = null!;

    [JsonPropertyName("wolfie")]
    public GMonster Wolfie { get; init; } = null!;

    [JsonPropertyName("xmagefi")]
    public GMonster Xmagefi { get; init; } = null!;

    [JsonPropertyName("xmagefz")]
    public GMonster Xmagefz { get; init; } = null!;

    [JsonPropertyName("xmagen")]
    public GMonster Xmagen { get; init; } = null!;

    [JsonPropertyName("xmagex")]
    public GMonster Xmagex { get; init; } = null!;

    [JsonPropertyName("xscorpion")]
    public GMonster Xscorpion { get; init; } = null!;

    [JsonPropertyName("zapper0")]
    public GMonster Zapper0 { get; init; } = null!;

    internal override void BuildLookupTable()
    {
        base.BuildLookupTable();

        //map accessors are populated based on the string from the server, not the local copy.
        foreach ((var accessor, var monster) in Entries.Reverse())
            if (string.IsNullOrEmpty(monster.Accessor))
                monster.Accessor = accessor;
    }
}