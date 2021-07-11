using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Monsters
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<Monster>))]
    public class MonstersDatum : DatumBase<Monster>
    {
        public Monster A1 { get; set; } = null!;
        public Monster A2 { get; set; } = null!;
        public Monster A3 { get; set; } = null!;
        public Monster A4 { get; set; } = null!;
        public Monster A5 { get; set; } = null!;
        public Monster A6 { get; set; } = null!;
        public Monster A7 { get; set; } = null!;
        public Monster A8 { get; set; } = null!;
        public Monster ArcticBee { get; set; } = null!;
        public Monster Armadillo { get; set; } = null!;
        public Monster Bat { get; set; } = null!;
        public Monster Bbpompom { get; set; } = null!;
        public Monster Bee { get; set; } = null!;
        public Monster BigBird { get; set; } = null!;
        public Monster BlueFairy { get; set; } = null!;
        public Monster Boar { get; set; } = null!;
        public Monster Booboo { get; set; } = null!;
        public Monster BScorpion { get; set; } = null!;
        public Monster Cgoo { get; set; } = null!;
        public Monster Crab { get; set; } = null!;
        public Monster CrabX { get; set; } = null!;
        public Monster Croc { get; set; } = null!;
        public Monster CuteBee { get; set; } = null!;
        public Monster DKnight2 { get; set; } = null!;
        public Monster Dragold { get; set; } = null!;
        [JsonProperty("d_wiz")]
        public Monster DWiz { get; set; } = null!;
        public Monster EElemental { get; set; } = null!;
        public Monster Ent { get; set; } = null!;
        public Monster FElemental { get; set; } = null!;
        public Monster FieldGen0 { get; set; } = null!;
        public Monster FireRoamer { get; set; } = null!;
        public Monster Franky { get; set; } = null!;
        public Monster Frog { get; set; } = null!;
        public Monster FVampire { get; set; } = null!;
        public Monster GBluePro { get; set; } = null!;
        public Monster GGreenPro { get; set; } = null!;
        public Monster Ghost { get; set; } = null!;
        public Monster Goblin { get; set; } = null!;
        public Monster GoldenBat { get; set; } = null!;
        public Monster Goo { get; set; } = null!;
        public Monster GPurplePro { get; set; } = null!;
        public Monster GRedPro { get; set; } = null!;
        public Monster GreenFairy { get; set; } = null!;
        public Monster GreenJr { get; set; } = null!;
        public Monster Grinch { get; set; } = null!;
        public Monster GScorpion { get; set; } = null!;
        public Monster Hen { get; set; } = null!;
        public Monster IceGolem { get; set; } = null!;
        public Monster IceRoamer { get; set; } = null!;
        public Monster Jr { get; set; } = null!;
        public Monster Kitty1 { get; set; } = null!;
        public Monster Kitty2 { get; set; } = null!;
        public Monster Kitty3 { get; set; } = null!;
        public Monster Kitty4 { get; set; } = null!;
        public Monster LigerX { get; set; } = null!;
        public Monster MechaGnome { get; set; } = null!;
        public Monster MiniMush { get; set; } = null!;
        public Monster Mole { get; set; } = null!;
        public Monster MrGreen { get; set; } = null!;
        public Monster MrPumpkin { get; set; } = null!;
        public Monster Mummy { get; set; } = null!;
        public Monster MVampire { get; set; } = null!;
        public Monster NElemental { get; set; } = null!;
        public Monster OneEye { get; set; } = null!;
        public Monster OSnake { get; set; } = null!;
        public Monster Phoenix { get; set; } = null!;
        public Monster PinkGoblin { get; set; } = null!;
        public Monster PinkGoo { get; set; } = null!;
        public Monster Plantoid { get; set; } = null!;
        public Monster Poisio { get; set; } = null!;
        public Monster Porcupine { get; set; } = null!;
        public Monster Pppompom { get; set; } = null!;
        public Monster Prat { get; set; } = null!;
        public Monster Puppy1 { get; set; } = null!;
        public Monster Puppy2 { get; set; } = null!;
        public Monster Puppy3 { get; set; } = null!;
        public Monster Puppy4 { get; set; } = null!;
        public Monster Rat { get; set; } = null!;
        public Monster RedFairy { get; set; } = null!;
        public Monster Rooster { get; set; } = null!;
        public Monster Rudolph { get; set; } = null!;
        public Monster Scorpion { get; set; } = null!;
        public Monster Skeletor { get; set; } = null!;
        public Monster Snake { get; set; } = null!;
        public Monster SnowMan { get; set; } = null!;
        public Monster Spider { get; set; } = null!;
        public Monster Squig { get; set; } = null!;
        public Monster SquigToad { get; set; } = null!;
        public Monster Stompy { get; set; } = null!;
        public Monster StoneWorm { get; set; } = null!;
        public Monster Target { get; set; } = null!;
        [JsonProperty("target_a500")]
        public Monster TargetA500 { get; set; } = null!;
        [JsonProperty("target_a750")]
        public Monster TargetA750 { get; set; } = null!;
        [JsonProperty("target_ar500red")]
        public Monster TargetAr500Red { get; set; } = null!;
        [JsonProperty("target_ar900")]
        public Monster TargetAr900 { get; set; } = null!;
        [JsonProperty("target_r500")]
        public Monster TargetR500 { get; set; } = null!;
        [JsonProperty("target_r750")]
        public Monster TargetR750 { get; set; } = null!;

        public Monster TinyP { get; set; } = null!;
        public Monster Tortoise { get; set; } = null!;
        public Monster VBat { get; set; } = null!;
        public Monster Wabbit { get; set; } = null!;
        public Monster WElemental { get; set; } = null!;
        public Monster Wolf { get; set; } = null!;
        public Monster Wolfie { get; set; } = null!;
        public Monster XMageFI { get; set; } = null!;
        public Monster XMageFZ { get; set; } = null!;
        public Monster XMageN { get; set; } = null!;
        public Monster XMageX { get; set; } = null!;
        public Monster XScorpion { get; set; } = null!;
        public Monster Zapper0 { get; set; } = null!;
    }
}