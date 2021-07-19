using AL.Core.Json.Converters;
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
        public GMonster A1 { get; set; } = null!;
        public GMonster A2 { get; set; } = null!;
        public GMonster A3 { get; set; } = null!;
        public GMonster A4 { get; set; } = null!;
        public GMonster A5 { get; set; } = null!;
        public GMonster A6 { get; set; } = null!;
        public GMonster A7 { get; set; } = null!;
        public GMonster A8 { get; set; } = null!;
        public GMonster ArcticBee { get; set; } = null!;
        public GMonster Armadillo { get; set; } = null!;
        public GMonster Bat { get; set; } = null!;
        public GMonster Bbpompom { get; set; } = null!;
        public GMonster Bee { get; set; } = null!;
        public GMonster BigBird { get; set; } = null!;
        public GMonster BlueFairy { get; set; } = null!;
        public GMonster Boar { get; set; } = null!;
        public GMonster Booboo { get; set; } = null!;
        public GMonster BScorpion { get; set; } = null!;
        public GMonster Cgoo { get; set; } = null!;
        public GMonster Crab { get; set; } = null!;
        public GMonster CrabX { get; set; } = null!;
        public GMonster Croc { get; set; } = null!;
        public GMonster CuteBee { get; set; } = null!;
        public GMonster DKnight2 { get; set; } = null!;
        public GMonster Dragold { get; set; } = null!;
        [JsonProperty("d_wiz")]
        public GMonster DWiz { get; set; } = null!;
        public GMonster EElemental { get; set; } = null!;
        public GMonster Ent { get; set; } = null!;
        public GMonster FElemental { get; set; } = null!;
        public GMonster FieldGen0 { get; set; } = null!;
        public GMonster FireRoamer { get; set; } = null!;
        public GMonster Franky { get; set; } = null!;
        public GMonster Frog { get; set; } = null!;
        public GMonster FVampire { get; set; } = null!;
        public GMonster GBluePro { get; set; } = null!;
        public GMonster GGreenPro { get; set; } = null!;
        public GMonster Ghost { get; set; } = null!;
        public GMonster Goblin { get; set; } = null!;
        public GMonster GoldenBat { get; set; } = null!;
        public GMonster Goo { get; set; } = null!;
        public GMonster GPurplePro { get; set; } = null!;
        public GMonster GRedPro { get; set; } = null!;
        public GMonster GreenFairy { get; set; } = null!;
        public GMonster GreenJr { get; set; } = null!;
        public GMonster Grinch { get; set; } = null!;
        public GMonster GScorpion { get; set; } = null!;
        public GMonster Hen { get; set; } = null!;
        public GMonster IceGolem { get; set; } = null!;
        public GMonster IceRoamer { get; set; } = null!;
        public GMonster Jr { get; set; } = null!;
        public GMonster Kitty1 { get; set; } = null!;
        public GMonster Kitty2 { get; set; } = null!;
        public GMonster Kitty3 { get; set; } = null!;
        public GMonster Kitty4 { get; set; } = null!;
        public GMonster LigerX { get; set; } = null!;
        public GMonster MechaGnome { get; set; } = null!;
        public GMonster MiniMush { get; set; } = null!;
        public GMonster Mole { get; set; } = null!;
        public GMonster MrGreen { get; set; } = null!;
        public GMonster MrPumpkin { get; set; } = null!;
        public GMonster Mummy { get; set; } = null!;
        public GMonster MVampire { get; set; } = null!;
        public GMonster NElemental { get; set; } = null!;
        public GMonster OneEye { get; set; } = null!;
        public GMonster OSnake { get; set; } = null!;
        public GMonster Phoenix { get; set; } = null!;
        public GMonster PinkGoblin { get; set; } = null!;
        public GMonster PinkGoo { get; set; } = null!;
        public GMonster Plantoid { get; set; } = null!;
        public GMonster Poisio { get; set; } = null!;
        public GMonster Porcupine { get; set; } = null!;
        public GMonster Pppompom { get; set; } = null!;
        public GMonster Prat { get; set; } = null!;
        public GMonster Puppy1 { get; set; } = null!;
        public GMonster Puppy2 { get; set; } = null!;
        public GMonster Puppy3 { get; set; } = null!;
        public GMonster Puppy4 { get; set; } = null!;
        public GMonster Rat { get; set; } = null!;
        public GMonster RedFairy { get; set; } = null!;
        public GMonster Rooster { get; set; } = null!;
        public GMonster Rudolph { get; set; } = null!;
        public GMonster Scorpion { get; set; } = null!;
        public GMonster Skeletor { get; set; } = null!;
        public GMonster Snake { get; set; } = null!;
        public GMonster SnowMan { get; set; } = null!;
        public GMonster Spider { get; set; } = null!;
        public GMonster Squig { get; set; } = null!;
        public GMonster SquigToad { get; set; } = null!;
        public GMonster Stompy { get; set; } = null!;
        public GMonster StoneWorm { get; set; } = null!;
        public GMonster Target { get; set; } = null!;
        [JsonProperty("target_a500")]
        public GMonster TargetA500 { get; set; } = null!;
        [JsonProperty("target_a750")]
        public GMonster TargetA750 { get; set; } = null!;
        [JsonProperty("target_ar500red")]
        public GMonster TargetAr500Red { get; set; } = null!;
        [JsonProperty("target_ar900")]
        public GMonster TargetAr900 { get; set; } = null!;
        [JsonProperty("target_r500")]
        public GMonster TargetR500 { get; set; } = null!;
        [JsonProperty("target_r750")]
        public GMonster TargetR750 { get; set; } = null!;

        public GMonster TinyP { get; set; } = null!;
        public GMonster Tortoise { get; set; } = null!;
        public GMonster VBat { get; set; } = null!;
        public GMonster Wabbit { get; set; } = null!;
        public GMonster WElemental { get; set; } = null!;
        public GMonster Wolf { get; set; } = null!;
        public GMonster Wolfie { get; set; } = null!;
        public GMonster XMageFI { get; set; } = null!;
        public GMonster XMageFZ { get; set; } = null!;
        public GMonster XMageN { get; set; } = null!;
        public GMonster XMageX { get; set; } = null!;
        public GMonster XScorpion { get; set; } = null!;
        public GMonster Zapper0 { get; set; } = null!;
    }
}