using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Monsters
{
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<Monster>))]
    public class MonstersDatum : DatumBase<Monster>
    {
        public Monster A1 { get; set; }
        public Monster A2 { get; set; }
        public Monster A3 { get; set; }
        public Monster A4 { get; set; }
        public Monster A5 { get; set; }
        public Monster A6 { get; set; }
        public Monster A7 { get; set; }
        public Monster A8 { get; set; }
        public Monster ArcticBee { get; set; }
        public Monster Armadillo { get; set; }
        public Monster Bat { get; set; }
        public Monster Bbpompom { get; set; }
        public Monster Bee { get; set; }
        public Monster BigBird { get; set; }
        public Monster BlueFairy { get; set; }
        public Monster Boar { get; set; }
        public Monster Booboo { get; set; }
        public Monster BScorpion { get; set; }
        public Monster Cgoo { get; set; }
        public Monster Crab { get; set; }
        public Monster CrabX { get; set; }
        public Monster Croc { get; set; }
        public Monster CuteBee { get; set; }
        public Monster DKnight2 { get; set; }
        public Monster Dragold { get; set; }
        public Monster Ent { get; set; }
        public Monster FireRoamer { get; set; }
        public Monster Franky { get; set; }
        public Monster Frog { get; set; }
        public Monster FVampire { get; set; }
        public Monster Ghost { get; set; }
        public Monster GoldenBat { get; set; }
        public Monster Goo { get; set; }
        public Monster GreenFairy { get; set; }
        public Monster GreenJr { get; set; }
        public Monster Grinch { get; set; }
        public Monster GScorpion { get; set; }
        public Monster Hen { get; set; }
        public Monster IceGolem { get; set; }
        public Monster IceRoamer { get; set; }
        public Monster Jr { get; set; }
        public Monster MechaGnome { get; set; }
        public Monster MiniMush { get; set; }
        public Monster Mole { get; set; }
        public Monster MrGreen { get; set; }
        public Monster MrPumpkin { get; set; }
        public Monster Mummy { get; set; }
        public Monster MVampire { get; set; }
        public Monster OneEye { get; set; }
        public Monster OSnake { get; set; }
        public Monster Phoenix { get; set; }
        public Monster PinkGoblin { get; set; }
        public Monster PinkGoo { get; set; }
        public Monster Plantoid { get; set; }
        public Monster Poisio { get; set; }
        public Monster Porcupine { get; set; }
        public Monster Pppompom { get; set; }
        public Monster Prat { get; set; }
        public Monster Rat { get; set; }
        public Monster RedFairy { get; set; }
        public Monster Rooster { get; set; }
        public Monster Rudolph { get; set; }
        public Monster Scorpion { get; set; }
        public Monster Skeletor { get; set; }
        public Monster Snake { get; set; }
        public Monster SnowMan { get; set; }
        public Monster Spider { get; set; }
        public Monster Squig { get; set; }
        public Monster SquigToad { get; set; }
        public Monster Stompy { get; set; }
        public Monster StoneWorm { get; set; }

        [JsonProperty("target_ar500red")]
        public Monster TargetAr500Red { get; set; }

        public Monster TinyP { get; set; }
        public Monster Tortoise { get; set; }
        public Monster VBat { get; set; }
        public Monster Wabbit { get; set; }
        public Monster Wolf { get; set; }
        public Monster Wolfie { get; set; }
        public Monster Xmagex { get; set; }
        public Monster XScorpion { get; set; }
    }
}