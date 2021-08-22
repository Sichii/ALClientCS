using System.Linq;
using AL.Core.Json.Converters;
using Chaos.Core.Extensions;
using Newtonsoft.Json;

namespace AL.Data.Monsters
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<GMonster>))]
    public class MonstersDatum : DatumBase<GMonster>
    {
        public GMonster A1 { get; init; } = null!;
        public GMonster A2 { get; init; } = null!;
        public GMonster A3 { get; init; } = null!;
        public GMonster A4 { get; init; } = null!;
        public GMonster A5 { get; init; } = null!;
        public GMonster A6 { get; init; } = null!;
        public GMonster A7 { get; init; } = null!;
        public GMonster A8 { get; init; } = null!;
        public GMonster Arcticbee { get; init; } = null!;
        public GMonster Armadillo { get; init; } = null!;
        public GMonster Bat { get; init; } = null!;
        public GMonster Bbpompom { get; init; } = null!;
        public GMonster Bee { get; init; } = null!;
        public GMonster Bigbird { get; init; } = null!;
        public GMonster Bluefairy { get; init; } = null!;
        public GMonster Boar { get; init; } = null!;
        public GMonster Booboo { get; init; } = null!;
        public GMonster Bscorpion { get; init; } = null!;
        public GMonster Cgoo { get; init; } = null!;
        public GMonster Chestm { get; init; } = null!;
        public GMonster Crab { get; init; } = null!;
        public GMonster Crabx { get; init; } = null!;
        public GMonster Croc { get; init; } = null!;
        public GMonster Cutebee { get; init; } = null!;
        public GMonster Dknight2 { get; init; } = null!;
        public GMonster Dragold { get; init; } = null!;
        [JsonProperty("d_wiz")]
        public GMonster DWiz { get; init; } = null!;
        public GMonster Eelemental { get; init; } = null!;
        public GMonster Ent { get; init; } = null!;
        public GMonster Felemental { get; init; } = null!;
        public GMonster Fieldgen0 { get; init; } = null!;
        public GMonster Fireroamer { get; init; } = null!;
        public GMonster Franky { get; init; } = null!;
        public GMonster Frog { get; init; } = null!;
        public GMonster Fvampire { get; init; } = null!;
        public GMonster Gbluepro { get; init; } = null!;
        public GMonster Ggreenpro { get; init; } = null!;
        public GMonster Ghost { get; init; } = null!;
        public GMonster Goblin { get; init; } = null!;
        public GMonster Goldenbat { get; init; } = null!;
        public GMonster Goo { get; init; } = null!;
        public GMonster Gpurplepro { get; init; } = null!;
        public GMonster Gredpro { get; init; } = null!;
        public GMonster Greenfairy { get; init; } = null!;
        public GMonster Greenjr { get; init; } = null!;
        public GMonster Grinch { get; init; } = null!;
        public GMonster Gscorpion { get; init; } = null!;
        public GMonster Hen { get; init; } = null!;
        public GMonster Icegolem { get; init; } = null!;
        public GMonster Iceroamer { get; init; } = null!;
        public GMonster Jr { get; init; } = null!;
        public GMonster Jrat { get; init; } = null!;
        public GMonster Kitty1 { get; init; } = null!;
        public GMonster Kitty2 { get; init; } = null!;
        public GMonster Kitty3 { get; init; } = null!;
        public GMonster Kitty4 { get; init; } = null!;
        public GMonster Ligerx { get; init; } = null!;
        public GMonster Mechagnome { get; init; } = null!;
        public GMonster Minimush { get; init; } = null!;
        public GMonster Mole { get; init; } = null!;
        public GMonster Mrgreen { get; init; } = null!;
        public GMonster Mrpumpkin { get; init; } = null!;
        public GMonster Mummy { get; init; } = null!;
        public GMonster Mvampire { get; init; } = null!;
        public GMonster Nelemental { get; init; } = null!;
        public GMonster Nerfedmummy { get; init; } = null!;
        public GMonster Oneeye { get; init; } = null!;
        public GMonster Osnake { get; init; } = null!;
        public GMonster Phoenix { get; init; } = null!;
        public GMonster Pinkgoblin { get; init; } = null!;
        public GMonster Pinkgoo { get; init; } = null!;
        public GMonster Plantoid { get; init; } = null!;
        public GMonster Poisio { get; init; } = null!;
        public GMonster Porcupine { get; init; } = null!;
        public GMonster Pppompom { get; init; } = null!;
        public GMonster Prat { get; init; } = null!;
        public GMonster Puppy1 { get; init; } = null!;
        public GMonster Puppy2 { get; init; } = null!;
        public GMonster Puppy3 { get; init; } = null!;
        public GMonster Puppy4 { get; init; } = null!;
        public GMonster Rat { get; init; } = null!;
        public GMonster Redfairy { get; init; } = null!;
        public GMonster Rooster { get; init; } = null!;
        public GMonster Rudolph { get; init; } = null!;
        public GMonster Scorpion { get; init; } = null!;
        public GMonster Skeletor { get; init; } = null!;
        public GMonster Snake { get; init; } = null!;
        public GMonster Snowman { get; init; } = null!;
        public GMonster Spider { get; init; } = null!;
        public GMonster Squig { get; init; } = null!;
        public GMonster Squigtoad { get; init; } = null!;
        public GMonster Stompy { get; init; } = null!;
        public GMonster Stoneworm { get; init; } = null!;
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
        public GMonster Tinyp { get; init; } = null!;
        public GMonster Tortoise { get; init; } = null!;
        public GMonster Vbat { get; init; } = null!;
        public GMonster Wabbit { get; init; } = null!;
        public GMonster Welemental { get; init; } = null!;
        public GMonster Wolf { get; init; } = null!;
        public GMonster Wolfie { get; init; } = null!;
        public GMonster Xmagefi { get; init; } = null!;
        public GMonster Xmagefz { get; init; } = null!;
        public GMonster Xmagen { get; init; } = null!;
        public GMonster Xmagex { get; init; } = null!;
        public GMonster Xscorpion { get; init; } = null!;
        public GMonster Zapper0 { get; init; } = null!;

        internal override void BuildLookupTable()
        {
            base.BuildLookupTable();

            //map accessors are populated based on the string from the server, not the local copy.
            foreach ((var accessor, var monster) in this.Reverse().DistinctBy(kvp => kvp.Value))
                monster.Accessor = accessor;
        }
    }
}