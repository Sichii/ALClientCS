#region
using System.Linq;
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
        public GMonster A1 { get; init; } = null!;

        [JsonProperty("a2")]
        public GMonster A2 { get; init; } = null!;

        [JsonProperty("a3")]
        public GMonster A3 { get; init; } = null!;

        [JsonProperty("a4")]
        public GMonster A4 { get; init; } = null!;

        [JsonProperty("a5")]
        public GMonster A5 { get; init; } = null!;

        [JsonProperty("a6")]
        public GMonster A6 { get; init; } = null!;

        [JsonProperty("a7")]
        public GMonster A7 { get; init; } = null!;

        [JsonProperty("a8")]
        public GMonster A8 { get; init; } = null!;

        [JsonProperty("arcticbee")]
        public GMonster Arcticbee { get; init; } = null!;

        [JsonProperty("armadillo")]
        public GMonster Armadillo { get; init; } = null!;

        [JsonProperty("bat")]
        public GMonster Bat { get; init; } = null!;

        [JsonProperty("bbpompom")]
        public GMonster Bbpompom { get; init; } = null!;

        [JsonProperty("bee")]
        public GMonster Bee { get; init; } = null!;

        [JsonProperty("bgoo")]
        public GMonster Bgoo { get; init; } = null!;

        [JsonProperty("bigbird")]
        public GMonster Bigbird { get; init; } = null!;

        [JsonProperty("bluefairy")]
        public GMonster Bluefairy { get; init; } = null!;

        [JsonProperty("boar")]
        public GMonster Boar { get; init; } = null!;

        [JsonProperty("booboo")]
        public GMonster Booboo { get; init; } = null!;

        [JsonProperty("bscorpion")]
        public GMonster Bscorpion { get; init; } = null!;

        [JsonProperty("cgoo")]
        public GMonster Cgoo { get; init; } = null!;

        [JsonProperty("chestm")]
        public GMonster Chestm { get; init; } = null!;

        [JsonProperty("crab")]
        public GMonster Crab { get; init; } = null!;

        [JsonProperty("crabx")]
        public GMonster Crabx { get; init; } = null!;

        [JsonProperty("crabxx")]
        public GMonster Crabxx { get; init; } = null!;

        [JsonProperty("croc")]
        public GMonster Croc { get; init; } = null!;

        [JsonProperty("cutebee")]
        public GMonster Cutebee { get; init; } = null!;

        [JsonProperty("dknight2")]
        public GMonster Dknight2 { get; init; } = null!;

        [JsonProperty("dragold")]
        public GMonster Dragold { get; init; } = null!;

        [JsonProperty("d_wiz")]
        public GMonster DWiz { get; init; } = null!;

        [JsonProperty("eelemental")]
        public GMonster Eelemental { get; init; } = null!;

        [JsonProperty("ent")]
        public GMonster Ent { get; init; } = null!;

        [JsonProperty("felemental")]
        public GMonster Felemental { get; init; } = null!;

        [JsonProperty("fieldgen0")]
        public GMonster Fieldgen0 { get; init; } = null!;

        [JsonProperty("fireroamer")]
        public GMonster Fireroamer { get; init; } = null!;

        [JsonProperty("franky")]
        public GMonster Franky { get; init; } = null!;

        [JsonProperty("frog")]
        public GMonster Frog { get; init; } = null!;

        [JsonProperty("fvampire")]
        public GMonster Fvampire { get; init; } = null!;

        [JsonProperty("gbluepro")]
        public GMonster Gbluepro { get; init; } = null!;

        [JsonProperty("ggreenpro")]
        public GMonster Ggreenpro { get; init; } = null!;

        [JsonProperty("ghost")]
        public GMonster Ghost { get; init; } = null!;

        [JsonProperty("goblin")]
        public GMonster Goblin { get; init; } = null!;

        [JsonProperty("goldenbat")]
        public GMonster Goldenbat { get; init; } = null!;

        [JsonProperty("goo")]
        public GMonster Goo { get; init; } = null!;

        [JsonProperty("gpurplepro")]
        public GMonster Gpurplepro { get; init; } = null!;

        [JsonProperty("gredpro")]
        public GMonster Gredpro { get; init; } = null!;

        [JsonProperty("greenfairy")]
        public GMonster Greenfairy { get; init; } = null!;

        [JsonProperty("greenjr")]
        public GMonster Greenjr { get; init; } = null!;

        [JsonProperty("grinch")]
        public GMonster Grinch { get; init; } = null!;

        [JsonProperty("gscorpion")]
        public GMonster Gscorpion { get; init; } = null!;

        [JsonProperty("harpy")]
        public GMonster Harpy { get; init; } = null!;

        [JsonProperty("hen")]
        public GMonster Hen { get; init; } = null!;

        [JsonProperty("icegolem")]
        public GMonster Icegolem { get; init; } = null!;

        [JsonProperty("iceroamer")]
        public GMonster Iceroamer { get; init; } = null!;

        [JsonProperty("jr")]
        public GMonster Jr { get; init; } = null!;

        [JsonProperty("jrat")]
        public GMonster Jrat { get; init; } = null!;

        [JsonProperty("kitty1")]
        public GMonster Kitty1 { get; init; } = null!;

        [JsonProperty("kitty2")]
        public GMonster Kitty2 { get; init; } = null!;

        [JsonProperty("kitty3")]
        public GMonster Kitty3 { get; init; } = null!;

        [JsonProperty("kitty4")]
        public GMonster Kitty4 { get; init; } = null!;

        [JsonProperty("ligerx")]
        public GMonster Ligerx { get; init; } = null!;

        [JsonProperty("mechagnome")]
        public GMonster Mechagnome { get; init; } = null!;

        [JsonProperty("minimush")]
        public GMonster Minimush { get; init; } = null!;

        [JsonProperty("mole")]
        public GMonster Mole { get; init; } = null!;

        [JsonProperty("mrgreen")]
        public GMonster Mrgreen { get; init; } = null!;

        [JsonProperty("mrpumpkin")]
        public GMonster Mrpumpkin { get; init; } = null!;

        [JsonProperty("mummy")]
        public GMonster Mummy { get; init; } = null!;

        [JsonProperty("mvampire")]
        public GMonster Mvampire { get; init; } = null!;

        [JsonProperty("nelemental")]
        public GMonster Nelemental { get; init; } = null!;

        [JsonProperty("nerfedbat")]
        public GMonster Nerfedbat { get; init; } = null!;

        [JsonProperty("nerfedmummy")]
        public GMonster Nerfedmummy { get; init; } = null!;

        [JsonProperty("oneeye")]
        public GMonster Oneeye { get; init; } = null!;

        [JsonProperty("osnake")]
        public GMonster Osnake { get; init; } = null!;

        [JsonProperty("phoenix")]
        public GMonster Phoenix { get; init; } = null!;

        [JsonProperty("pinkgoblin")]
        public GMonster Pinkgoblin { get; init; } = null!;

        [JsonProperty("pinkgoo")]
        public GMonster Pinkgoo { get; init; } = null!;

        [JsonProperty("plantoid")]
        public GMonster Plantoid { get; init; } = null!;

        [JsonProperty("poisio")]
        public GMonster Poisio { get; init; } = null!;

        [JsonProperty("porcupine")]
        public GMonster Porcupine { get; init; } = null!;

        [JsonProperty("pppompom")]
        public GMonster Pppompom { get; init; } = null!;

        [JsonProperty("prat")]
        public GMonster Prat { get; init; } = null!;

        [JsonProperty("puppy1")]
        public GMonster Puppy1 { get; init; } = null!;

        [JsonProperty("puppy2")]
        public GMonster Puppy2 { get; init; } = null!;

        [JsonProperty("puppy3")]
        public GMonster Puppy3 { get; init; } = null!;

        [JsonProperty("puppy4")]
        public GMonster Puppy4 { get; init; } = null!;

        [JsonProperty("rat")]
        public GMonster Rat { get; init; } = null!;

        [JsonProperty("redfairy")]
        public GMonster Redfairy { get; init; } = null!;

        [JsonProperty("rgoo")]
        public GMonster Rgoo { get; init; } = null!;

        [JsonProperty("rharpy")]
        public GMonster Rharpy { get; init; } = null!;

        [JsonProperty("rooster")]
        public GMonster Rooster { get; init; } = null!;

        [JsonProperty("rudolph")]
        public GMonster Rudolph { get; init; } = null!;

        [JsonProperty("scorpion")]
        public GMonster Scorpion { get; init; } = null!;

        [JsonProperty("skeletor")]
        public GMonster Skeletor { get; init; } = null!;

        [JsonProperty("slenderman")]
        public GMonster Slenderman { get; init; } = null!;

        [JsonProperty("snake")]
        public GMonster Snake { get; init; } = null!;

        [JsonProperty("snowman")]
        public GMonster Snowman { get; init; } = null!;

        [JsonProperty("spider")]
        public GMonster Spider { get; init; } = null!;

        [JsonProperty("squig")]
        public GMonster Squig { get; init; } = null!;

        [JsonProperty("squigtoad")]
        public GMonster Squigtoad { get; init; } = null!;

        [JsonProperty("stompy")]
        public GMonster Stompy { get; init; } = null!;

        [JsonProperty("stoneworm")]
        public GMonster Stoneworm { get; init; } = null!;

        [JsonProperty("target")]
        public GMonster Target { get; init; } = null!;

        [JsonProperty("target_a500")]
        public GMonster TargetA500 { get; init; } = null!;

        [JsonProperty("target_a750")]
        public GMonster TargetA750 { get; init; } = null!;

        [JsonProperty("target_ar500red")]
        public GMonster TargetAr500Red { get; init; } = null!;

        [JsonProperty("target_ar900")]
        public GMonster TargetAr900 { get; init; } = null!;

        [JsonProperty("target_r500")]
        public GMonster TargetR500 { get; init; } = null!;

        [JsonProperty("target_r750")]
        public GMonster TargetR750 { get; init; } = null!;

        [JsonProperty("tiger")]
        public GMonster Tiger { get; init; } = null!;

        [JsonProperty("tinyp")]
        public GMonster Tinyp { get; init; } = null!;

        [JsonProperty("tortoise")]
        public GMonster Tortoise { get; init; } = null!;

        [JsonProperty("vbat")]
        public GMonster Vbat { get; init; } = null!;

        [JsonProperty("wabbit")]
        public GMonster Wabbit { get; init; } = null!;

        [JsonProperty("welemental")]
        public GMonster Welemental { get; init; } = null!;

        [JsonProperty("wolf")]
        public GMonster Wolf { get; init; } = null!;

        [JsonProperty("wolfie")]
        public GMonster Wolfie { get; init; } = null!;

        [JsonProperty("xmagefi")]
        public GMonster Xmagefi { get; init; } = null!;

        [JsonProperty("xmagefz")]
        public GMonster Xmagefz { get; init; } = null!;

        [JsonProperty("xmagen")]
        public GMonster Xmagen { get; init; } = null!;

        [JsonProperty("xmagex")]
        public GMonster Xmagex { get; init; } = null!;

        [JsonProperty("xscorpion")]
        public GMonster Xscorpion { get; init; } = null!;

        [JsonProperty("zapper0")]
        public GMonster Zapper0 { get; init; } = null!;

        internal override void BuildLookupTable()
        {
            base.BuildLookupTable();

            //map accessors are populated based on the string from the server, not the local copy.
            foreach ((var accessor, var monster) in this.Reverse())
                if (string.IsNullOrEmpty(monster.Accessor))
                    monster.Accessor = accessor;
        }
    }
}