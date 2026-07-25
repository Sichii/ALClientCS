#region
using System.Linq;
using System.Text.Json.Serialization;
using AL.Core.Json.Converters;
using Newtonsoft.Json;
#endregion

namespace AL.Data.Monsters
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<GMonster>))]
    public class MonstersDatum : DatumBase<GMonster>
    {
        [JsonProperty("a1")]
        [JsonPropertyName("a1")]
        public GMonster A1 { get; init; } = null!;

        [JsonProperty("a2")]
        [JsonPropertyName("a2")]
        public GMonster A2 { get; init; } = null!;

        [JsonProperty("a3")]
        [JsonPropertyName("a3")]
        public GMonster A3 { get; init; } = null!;

        [JsonProperty("a4")]
        [JsonPropertyName("a4")]
        public GMonster A4 { get; init; } = null!;

        [JsonProperty("a5")]
        [JsonPropertyName("a5")]
        public GMonster A5 { get; init; } = null!;

        [JsonProperty("a6")]
        [JsonPropertyName("a6")]
        public GMonster A6 { get; init; } = null!;

        [JsonProperty("a7")]
        [JsonPropertyName("a7")]
        public GMonster A7 { get; init; } = null!;

        [JsonProperty("a8")]
        [JsonPropertyName("a8")]
        public GMonster A8 { get; init; } = null!;

        [JsonProperty("arcticbee")]
        [JsonPropertyName("arcticbee")]
        public GMonster Arcticbee { get; init; } = null!;

        [JsonProperty("armadillo")]
        [JsonPropertyName("armadillo")]
        public GMonster Armadillo { get; init; } = null!;

        [JsonProperty("bat")]
        [JsonPropertyName("bat")]
        public GMonster Bat { get; init; } = null!;

        [JsonProperty("bbpompom")]
        [JsonPropertyName("bbpompom")]
        public GMonster Bbpompom { get; init; } = null!;

        [JsonProperty("bee")]
        [JsonPropertyName("bee")]
        public GMonster Bee { get; init; } = null!;

        [JsonProperty("bgoo")]
        [JsonPropertyName("bgoo")]
        public GMonster Bgoo { get; init; } = null!;

        [JsonProperty("bigbird")]
        [JsonPropertyName("bigbird")]
        public GMonster Bigbird { get; init; } = null!;

        [JsonProperty("bluefairy")]
        [JsonPropertyName("bluefairy")]
        public GMonster Bluefairy { get; init; } = null!;

        [JsonProperty("boar")]
        [JsonPropertyName("boar")]
        public GMonster Boar { get; init; } = null!;

        [JsonProperty("booboo")]
        [JsonPropertyName("booboo")]
        public GMonster Booboo { get; init; } = null!;

        [JsonProperty("bscorpion")]
        [JsonPropertyName("bscorpion")]
        public GMonster Bscorpion { get; init; } = null!;

        [JsonProperty("cgoo")]
        [JsonPropertyName("cgoo")]
        public GMonster Cgoo { get; init; } = null!;

        [JsonProperty("chestm")]
        [JsonPropertyName("chestm")]
        public GMonster Chestm { get; init; } = null!;

        [JsonProperty("crab")]
        [JsonPropertyName("crab")]
        public GMonster Crab { get; init; } = null!;

        [JsonProperty("crabx")]
        [JsonPropertyName("crabx")]
        public GMonster Crabx { get; init; } = null!;

        [JsonProperty("crabxx")]
        [JsonPropertyName("crabxx")]
        public GMonster Crabxx { get; init; } = null!;

        [JsonProperty("croc")]
        [JsonPropertyName("croc")]
        public GMonster Croc { get; init; } = null!;

        [JsonProperty("cutebee")]
        [JsonPropertyName("cutebee")]
        public GMonster Cutebee { get; init; } = null!;

        [JsonProperty("dknight2")]
        [JsonPropertyName("dknight2")]
        public GMonster Dknight2 { get; init; } = null!;

        [JsonProperty("dragold")]
        [JsonPropertyName("dragold")]
        public GMonster Dragold { get; init; } = null!;

        [JsonProperty("dryad")]
        [JsonPropertyName("dryad")]
        public GMonster Dryad { get; init; } = null!;

        [JsonProperty("d_wiz")]
        [JsonPropertyName("d_wiz")]
        public GMonster DWiz { get; init; } = null!;

        [JsonProperty("eelemental")]
        [JsonPropertyName("eelemental")]
        public GMonster Eelemental { get; init; } = null!;

        [JsonProperty("ent")]
        [JsonPropertyName("ent")]
        public GMonster Ent { get; init; } = null!;

        [JsonProperty("felemental")]
        [JsonPropertyName("felemental")]
        public GMonster Felemental { get; init; } = null!;

        [JsonProperty("fieldgen0")]
        [JsonPropertyName("fieldgen0")]
        public GMonster Fieldgen0 { get; init; } = null!;

        [JsonProperty("fireroamer")]
        [JsonPropertyName("fireroamer")]
        public GMonster Fireroamer { get; init; } = null!;

        [JsonProperty("franky")]
        [JsonPropertyName("franky")]
        public GMonster Franky { get; init; } = null!;

        [JsonProperty("frog")]
        [JsonPropertyName("frog")]
        public GMonster Frog { get; init; } = null!;

        [JsonProperty("fvampire")]
        [JsonPropertyName("fvampire")]
        public GMonster Fvampire { get; init; } = null!;

        [JsonProperty("gbluepro")]
        [JsonPropertyName("gbluepro")]
        public GMonster Gbluepro { get; init; } = null!;

        [JsonProperty("ggreenpro")]
        [JsonPropertyName("ggreenpro")]
        public GMonster Ggreenpro { get; init; } = null!;

        [JsonProperty("ghost")]
        [JsonPropertyName("ghost")]
        public GMonster Ghost { get; init; } = null!;

        [JsonProperty("goblin")]
        [JsonPropertyName("goblin")]
        public GMonster Goblin { get; init; } = null!;

        [JsonProperty("goldenbat")]
        [JsonPropertyName("goldenbat")]
        public GMonster Goldenbat { get; init; } = null!;

        [JsonProperty("goldenbot")]
        [JsonPropertyName("goldenbot")]
        public GMonster Goldenbot { get; init; } = null!;

        [JsonProperty("goo")]
        [JsonPropertyName("goo")]
        public GMonster Goo { get; init; } = null!;

        [JsonProperty("gpurplepro")]
        [JsonPropertyName("gpurplepro")]
        public GMonster Gpurplepro { get; init; } = null!;

        [JsonProperty("gredpro")]
        [JsonPropertyName("gredpro")]
        public GMonster Gredpro { get; init; } = null!;

        [JsonProperty("greenfairy")]
        [JsonPropertyName("greenfairy")]
        public GMonster Greenfairy { get; init; } = null!;

        [JsonProperty("greenjr")]
        [JsonPropertyName("greenjr")]
        public GMonster Greenjr { get; init; } = null!;

        [JsonProperty("grinch")]
        [JsonPropertyName("grinch")]
        public GMonster Grinch { get; init; } = null!;

        [JsonProperty("gscorpion")]
        [JsonPropertyName("gscorpion")]
        public GMonster Gscorpion { get; init; } = null!;

        [JsonProperty("harpy")]
        [JsonPropertyName("harpy")]
        public GMonster Harpy { get; init; } = null!;

        [JsonProperty("hen")]
        [JsonPropertyName("hen")]
        public GMonster Hen { get; init; } = null!;

        [JsonProperty("icegolem")]
        [JsonPropertyName("icegolem")]
        public GMonster Icegolem { get; init; } = null!;

        [JsonProperty("iceroamer")]
        [JsonPropertyName("iceroamer")]
        public GMonster Iceroamer { get; init; } = null!;

        [JsonProperty("jr")]
        [JsonPropertyName("jr")]
        public GMonster Jr { get; init; } = null!;

        [JsonProperty("jrat")]
        [JsonPropertyName("jrat")]
        public GMonster Jrat { get; init; } = null!;

        [JsonProperty("kitty1")]
        [JsonPropertyName("kitty1")]
        public GMonster Kitty1 { get; init; } = null!;

        [JsonProperty("kitty2")]
        [JsonPropertyName("kitty2")]
        public GMonster Kitty2 { get; init; } = null!;

        [JsonProperty("kitty3")]
        [JsonPropertyName("kitty3")]
        public GMonster Kitty3 { get; init; } = null!;

        [JsonProperty("kitty4")]
        [JsonPropertyName("kitty4")]
        public GMonster Kitty4 { get; init; } = null!;

        [JsonProperty("ligerx")]
        [JsonPropertyName("ligerx")]
        public GMonster Ligerx { get; init; } = null!;

        [JsonProperty("mechagnome")]
        [JsonPropertyName("mechagnome")]
        public GMonster Mechagnome { get; init; } = null!;

        [JsonProperty("minimush")]
        [JsonPropertyName("minimush")]
        public GMonster Minimush { get; init; } = null!;

        [JsonProperty("mole")]
        [JsonPropertyName("mole")]
        public GMonster Mole { get; init; } = null!;

        [JsonProperty("mrgreen")]
        [JsonPropertyName("mrgreen")]
        public GMonster Mrgreen { get; init; } = null!;

        [JsonProperty("mrpumpkin")]
        [JsonPropertyName("mrpumpkin")]
        public GMonster Mrpumpkin { get; init; } = null!;

        [JsonProperty("mummy")]
        [JsonPropertyName("mummy")]
        public GMonster Mummy { get; init; } = null!;

        [JsonProperty("mvampire")]
        [JsonPropertyName("mvampire")]
        public GMonster Mvampire { get; init; } = null!;

        [JsonProperty("nelemental")]
        [JsonPropertyName("nelemental")]
        public GMonster Nelemental { get; init; } = null!;

        [JsonProperty("nerfedbat")]
        [JsonPropertyName("nerfedbat")]
        public GMonster Nerfedbat { get; init; } = null!;

        [JsonProperty("nerfedmummy")]
        [JsonPropertyName("nerfedmummy")]
        public GMonster Nerfedmummy { get; init; } = null!;

        [JsonProperty("odino")]
        [JsonPropertyName("odino")]
        public GMonster Odino { get; init; } = null!;

        [JsonProperty("oneeye")]
        [JsonPropertyName("oneeye")]
        public GMonster Oneeye { get; init; } = null!;

        [JsonProperty("osnake")]
        [JsonPropertyName("osnake")]
        public GMonster Osnake { get; init; } = null!;

        [JsonProperty("phoenix")]
        [JsonPropertyName("phoenix")]
        public GMonster Phoenix { get; init; } = null!;

        [JsonProperty("pinkgoblin")]
        [JsonPropertyName("pinkgoblin")]
        public GMonster Pinkgoblin { get; init; } = null!;

        [JsonProperty("pinkgoo")]
        [JsonPropertyName("pinkgoo")]
        public GMonster Pinkgoo { get; init; } = null!;

        [JsonProperty("plantoid")]
        [JsonPropertyName("plantoid")]
        public GMonster Plantoid { get; init; } = null!;

        [JsonProperty("poisio")]
        [JsonPropertyName("poisio")]
        public GMonster Poisio { get; init; } = null!;

        [JsonProperty("porcupine")]
        [JsonPropertyName("porcupine")]
        public GMonster Porcupine { get; init; } = null!;

        [JsonProperty("pppompom")]
        [JsonPropertyName("pppompom")]
        public GMonster Pppompom { get; init; } = null!;

        [JsonProperty("prat")]
        [JsonPropertyName("prat")]
        public GMonster Prat { get; init; } = null!;

        [JsonProperty("puppy1")]
        [JsonPropertyName("puppy1")]
        public GMonster Puppy1 { get; init; } = null!;

        [JsonProperty("puppy2")]
        [JsonPropertyName("puppy2")]
        public GMonster Puppy2 { get; init; } = null!;

        [JsonProperty("puppy3")]
        [JsonPropertyName("puppy3")]
        public GMonster Puppy3 { get; init; } = null!;

        [JsonProperty("puppy4")]
        [JsonPropertyName("puppy4")]
        public GMonster Puppy4 { get; init; } = null!;

        [JsonProperty("rat")]
        [JsonPropertyName("rat")]
        public GMonster Rat { get; init; } = null!;

        [JsonProperty("redfairy")]
        [JsonPropertyName("redfairy")]
        public GMonster Redfairy { get; init; } = null!;

        [JsonProperty("rgoo")]
        [JsonPropertyName("rgoo")]
        public GMonster Rgoo { get; init; } = null!;

        [JsonProperty("rharpy")]
        [JsonPropertyName("rharpy")]
        public GMonster Rharpy { get; init; } = null!;

        [JsonProperty("rooster")]
        [JsonPropertyName("rooster")]
        public GMonster Rooster { get; init; } = null!;

        [JsonProperty("rudolph")]
        [JsonPropertyName("rudolph")]
        public GMonster Rudolph { get; init; } = null!;

        [JsonProperty("scorpion")]
        [JsonPropertyName("scorpion")]
        public GMonster Scorpion { get; init; } = null!;

        [JsonProperty("skeletor")]
        [JsonPropertyName("skeletor")]
        public GMonster Skeletor { get; init; } = null!;

        [JsonProperty("slenderman")]
        [JsonPropertyName("slenderman")]
        public GMonster Slenderman { get; init; } = null!;

        [JsonProperty("snake")]
        [JsonPropertyName("snake")]
        public GMonster Snake { get; init; } = null!;

        [JsonProperty("snowman")]
        [JsonPropertyName("snowman")]
        public GMonster Snowman { get; init; } = null!;

        [JsonProperty("sparkbot")]
        [JsonPropertyName("sparkbot")]
        public GMonster Sparkbot { get; init; } = null!;

        [JsonProperty("spider")]
        [JsonPropertyName("spider")]
        public GMonster Spider { get; init; } = null!;

        [JsonProperty("spiderbl")]
        [JsonPropertyName("spiderbl")]
        public GMonster Spiderbl { get; init; } = null!;

        [JsonProperty("spiderbr")]
        [JsonPropertyName("spiderbr")]
        public GMonster Spiderbr { get; init; } = null!;

        [JsonProperty("spiderr")]
        [JsonPropertyName("spiderr")]
        public GMonster Spiderr { get; init; } = null!;

        [JsonProperty("squig")]
        [JsonPropertyName("squig")]
        public GMonster Squig { get; init; } = null!;

        [JsonProperty("squigtoad")]
        [JsonPropertyName("squigtoad")]
        public GMonster Squigtoad { get; init; } = null!;

        [JsonProperty("stompy")]
        [JsonPropertyName("stompy")]
        public GMonster Stompy { get; init; } = null!;

        [JsonProperty("stoneworm")]
        [JsonPropertyName("stoneworm")]
        public GMonster Stoneworm { get; init; } = null!;

        [JsonProperty("target")]
        [JsonPropertyName("target")]
        public GMonster Target { get; init; } = null!;

        [JsonProperty("target_a500")]
        [JsonPropertyName("target_a500")]
        public GMonster TargetA500 { get; init; } = null!;

        [JsonProperty("target_a750")]
        [JsonPropertyName("target_a750")]
        public GMonster TargetA750 { get; init; } = null!;

        [JsonProperty("target_ar500red")]
        [JsonPropertyName("target_ar500red")]
        public GMonster TargetAr500Red { get; init; } = null!;

        [JsonProperty("target_ar900")]
        [JsonPropertyName("target_ar900")]
        public GMonster TargetAr900 { get; init; } = null!;

        [JsonProperty("target_r500")]
        [JsonPropertyName("target_r500")]
        public GMonster TargetR500 { get; init; } = null!;

        [JsonProperty("target_r750")]
        [JsonPropertyName("target_r750")]
        public GMonster TargetR750 { get; init; } = null!;

        [JsonProperty("targetron")]
        [JsonPropertyName("targetron")]
        public GMonster Targetron { get; init; } = null!;

        [JsonProperty("tiger")]
        [JsonPropertyName("tiger")]
        public GMonster Tiger { get; init; } = null!;

        [JsonProperty("tinyp")]
        [JsonPropertyName("tinyp")]
        public GMonster Tinyp { get; init; } = null!;

        [JsonProperty("tortoise")]
        [JsonPropertyName("tortoise")]
        public GMonster Tortoise { get; init; } = null!;

        [JsonProperty("vbat")]
        [JsonPropertyName("vbat")]
        public GMonster Vbat { get; init; } = null!;

        [JsonProperty("wabbit")]
        [JsonPropertyName("wabbit")]
        public GMonster Wabbit { get; init; } = null!;

        [JsonProperty("welemental")]
        [JsonPropertyName("welemental")]
        public GMonster Welemental { get; init; } = null!;

        [JsonProperty("wolf")]
        [JsonPropertyName("wolf")]
        public GMonster Wolf { get; init; } = null!;

        [JsonProperty("wolfie")]
        [JsonPropertyName("wolfie")]
        public GMonster Wolfie { get; init; } = null!;

        [JsonProperty("xmagefi")]
        [JsonPropertyName("xmagefi")]
        public GMonster Xmagefi { get; init; } = null!;

        [JsonProperty("xmagefz")]
        [JsonPropertyName("xmagefz")]
        public GMonster Xmagefz { get; init; } = null!;

        [JsonProperty("xmagen")]
        [JsonPropertyName("xmagen")]
        public GMonster Xmagen { get; init; } = null!;

        [JsonProperty("xmagex")]
        [JsonPropertyName("xmagex")]
        public GMonster Xmagex { get; init; } = null!;

        [JsonProperty("xscorpion")]
        [JsonPropertyName("xscorpion")]
        public GMonster Xscorpion { get; init; } = null!;

        [JsonProperty("zapper0")]
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
}