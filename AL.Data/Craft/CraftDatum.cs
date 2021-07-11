using Newtonsoft.Json;

namespace AL.Data.Craft
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class CraftDatum : DatumBase<Recipe>
    {
        public Recipe ArmorRing { get; set; } = null!;
        public Recipe BasketOfEggs { get; set; } = null!;
        public Recipe BowOfTheDead { get; set; } = null!;
        public Recipe BronzeIngot { get; set; } = null!;
        public Recipe CandyCaneSword { get; set; } = null!;
        public Recipe CClaw { get; set; } = null!;
        public Recipe Charmer { get; set; } = null!;
        public Recipe Computer { get; set; } = null!;
        public Recipe CTristone { get; set; } = null!;
        public Recipe DaggerOfTheDead { get; set; } = null!;
        public Recipe DartGun { get; set; } = null!;
        public Recipe ElixirDex1 { get; set; } = null!;
        public Recipe ElixirDex2 { get; set; } = null!;

        [JsonProperty("elixirfires")]
        public Recipe ElixirFireRes { get; set; } = null!;

        [JsonProperty("elixirfzres")]
        public Recipe ElixirFreezeRes { get; set; } = null!;

        public Recipe ElixirInt1 { get; set; } = null!;
        public Recipe ElixirInt2 { get; set; } = null!;

        [JsonProperty("elixirpnres")]
        public Recipe ElixirPoisonRes { get; set; } = null!;

        public Recipe ElixirStr1 { get; set; } = null!;
        public Recipe ElixirStr2 { get; set; } = null!;
        public Recipe ElixirVit1 { get; set; } = null!;
        public Recipe ElixirVit2 { get; set; } = null!;
        public Recipe FClaw { get; set; } = null!;
        public Recipe FieryGloves { get; set; } = null!;
        public Recipe FireBlade { get; set; } = null!;
        public Recipe FireBow { get; set; } = null!;
        public Recipe FireStaff { get; set; } = null!;
        public Recipe FireStars { get; set; } = null!;
        public Recipe FrostBow { get; set; } = null!;
        public Recipe FrostStaff { get; set; } = null!;
        public Recipe FSword { get; set; } = null!;
        public Recipe GoldIngot { get; set; } = null!;
        public Recipe GStaff { get; set; } = null!;
        public Recipe HBow { get; set; } = null!;
        public Recipe HeartWood { get; set; } = null!;
        public Recipe LBelt { get; set; } = null!;
        public Recipe MaceOfTheDead { get; set; } = null!;
        public Recipe Merry { get; set; } = null!;
        public Recipe MushroomStaff { get; set; } = null!;
        public Recipe OfferingX { get; set; } = null!;
        public Recipe OrbG { get; set; } = null!;
        public Recipe OrnamentStaff { get; set; } = null!;
        public Recipe Pickaxe { get; set; } = null!;
        public Recipe PlatinumIngot { get; set; } = null!;
        public Recipe PouchBow { get; set; } = null!;
        public Recipe Quiver { get; set; } = null!;
        public Recipe ResistanceRing { get; set; } = null!;
        public Recipe Rod { get; set; } = null!;
        public Recipe SlimeStaff { get; set; } = null!;
        public Recipe SnakeOil { get; set; } = null!;
        public Recipe SpearOfTheDead { get; set; } = null!;
        public Recipe StaffOfTheDead { get; set; } = null!;
        public Recipe StealthCape { get; set; } = null!;
        public Recipe Stinger { get; set; } = null!;
        public Recipe SwordOfTheDead { get; set; } = null!;
        public Recipe WAttire { get; set; } = null!;
        public Recipe WBlade { get; set; } = null!;
        public Recipe WBreeches { get; set; } = null!;
        public Recipe WCap { get; set; } = null!;
        public Recipe WGloves { get; set; } = null!;
        public Recipe WingedBoots { get; set; } = null!;
        public Recipe WShoes { get; set; } = null!;
        public Recipe XBox { get; set; } = null!;
    }
}