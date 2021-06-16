using Newtonsoft.Json;

namespace AL.Data.Craft
{
    public class CraftDatum : DatumBase<Recipe>
    {
        public Recipe ArmorRing { get; set; }
        public Recipe BasketOfEggs { get; set; }
        public Recipe BowOfTheDead { get; set; }
        public Recipe BronzeIngot { get; set; }
        public Recipe CandyCaneSword { get; set; }
        public Recipe CClaw { get; set; }
        public Recipe Charmer { get; set; }
        public Recipe Computer { get; set; }
        public Recipe CTristone { get; set; }
        public Recipe DaggerOfTheDead { get; set; }
        public Recipe DartGun { get; set; }
        public Recipe ElixirDex1 { get; set; }
        public Recipe ElixirDex2 { get; set; }

        [JsonProperty("elixirfires")]
        public Recipe ElixirFireRes { get; set; }

        [JsonProperty("elixirfzres")]
        public Recipe ElixirFreezeRes { get; set; }

        public Recipe ElixirInt1 { get; set; }
        public Recipe ElixirInt2 { get; set; }

        [JsonProperty("elixirpnres")]
        public Recipe ElixirPoisonRes { get; set; }

        public Recipe ElixirStr1 { get; set; }
        public Recipe ElixirStr2 { get; set; }
        public Recipe ElixirVit1 { get; set; }
        public Recipe ElixirVit2 { get; set; }
        public Recipe FClaw { get; set; }
        public Recipe FieryGloves { get; set; }
        public Recipe FireBlade { get; set; }
        public Recipe FireBow { get; set; }
        public Recipe FireStaff { get; set; }
        public Recipe FireStars { get; set; }
        public Recipe FrostBow { get; set; }
        public Recipe FrostStaff { get; set; }
        public Recipe FSword { get; set; }
        public Recipe GoldIngot { get; set; }
        public Recipe GStaff { get; set; }
        public Recipe HBow { get; set; }
        public Recipe HeartWood { get; set; }
        public Recipe LBelt { get; set; }
        public Recipe MaceOfTheDead { get; set; }
        public Recipe Merry { get; set; }
        public Recipe MushroomStaff { get; set; }
        public Recipe OfferingX { get; set; }
        public Recipe OrbG { get; set; }
        public Recipe OrnamentStaff { get; set; }
        public Recipe Pickaxe { get; set; }
        public Recipe PlatinumIngot { get; set; }
        public Recipe PouchBow { get; set; }
        public Recipe Quiver { get; set; }
        public Recipe ResistanceRing { get; set; }
        public Recipe Rod { get; set; }
        public Recipe SlimeStaff { get; set; }
        public Recipe SnakeOil { get; set; }
        public Recipe SpearOfTheDead { get; set; }
        public Recipe StaffOfTheDead { get; set; }
        public Recipe StealthCape { get; set; }
        public Recipe Stinger { get; set; }
        public Recipe SwordOfTheDead { get; set; }
        public Recipe WAttire { get; set; }
        public Recipe WBlade { get; set; }
        public Recipe WBreeches { get; set; }
        public Recipe WCap { get; set; }
        public Recipe WGloves { get; set; }
        public Recipe WingedBoots { get; set; }
        public Recipe WShoes { get; set; }
        public Recipe XBox { get; set; }
    }
}