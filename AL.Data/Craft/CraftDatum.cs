using Newtonsoft.Json;

namespace AL.Data.Craft
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class CraftDatum : DatumBase<Recipe>
    {
        public Recipe Armorring { get; init; } = null!;
        public Recipe Basketofeggs { get; init; } = null!;
        public Recipe Bowofthedead { get; init; } = null!;
        public Recipe Bronzeingot { get; init; } = null!;
        public Recipe Candycanesword { get; init; } = null!;
        public Recipe Cclaw { get; init; } = null!;
        public Recipe Charmer { get; init; } = null!;
        public Recipe Computer { get; init; } = null!;
        public Recipe Ctristone { get; init; } = null!;
        public Recipe Daggerofthedead { get; init; } = null!;
        public Recipe Dartgun { get; init; } = null!;
        public Recipe Elixirdex1 { get; init; } = null!;
        public Recipe Elixirdex2 { get; init; } = null!;
        [JsonProperty("elixirfires")]
        public Recipe Elixirfireres { get; init; } = null!;
        [JsonProperty("elixirfzres")]
        public Recipe Elixirfreezeres { get; init; } = null!;
        public Recipe Elixirint1 { get; init; } = null!;
        public Recipe Elixirint2 { get; init; } = null!;
        public Recipe Elixirpnres { get; init; } = null!;
        public Recipe Elixirstr1 { get; init; } = null!;
        public Recipe Elixirstr2 { get; init; } = null!;
        public Recipe Elixirvit1 { get; init; } = null!;
        public Recipe Elixirvit2 { get; init; } = null!;
        public Recipe Fclaw { get; init; } = null!;
        public Recipe Fierygloves { get; init; } = null!;
        public Recipe Fireblade { get; init; } = null!;
        public Recipe Firebow { get; init; } = null!;
        public Recipe Firestaff { get; init; } = null!;
        public Recipe Firestars { get; init; } = null!;
        public Recipe Frostbow { get; init; } = null!;
        public Recipe Froststaff { get; init; } = null!;
        public Recipe Fsword { get; init; } = null!;
        public Recipe Goldingot { get; init; } = null!;
        public Recipe Gstaff { get; init; } = null!;
        public Recipe Hbow { get; init; } = null!;
        public Recipe Heartwood { get; init; } = null!;
        public Recipe Lbelt { get; init; } = null!;
        public Recipe Maceofthedead { get; init; } = null!;
        public Recipe Merry { get; init; } = null!;
        public Recipe Mushroomstaff { get; init; } = null!;
        public Recipe Offeringx { get; init; } = null!;
        public Recipe Orbg { get; init; } = null!;
        public Recipe Ornamentstaff { get; init; } = null!;
        public Recipe Pickaxe { get; init; } = null!;
        public Recipe Platinumingot { get; init; } = null!;
        public Recipe Pouchbow { get; init; } = null!;
        public Recipe Quiver { get; init; } = null!;
        public Recipe Resistancering { get; init; } = null!;
        public Recipe Rod { get; init; } = null!;
        public Recipe Slimestaff { get; init; } = null!;
        public Recipe Snakeoil { get; init; } = null!;
        public Recipe Spearofthedead { get; init; } = null!;
        public Recipe Staffofthedead { get; init; } = null!;
        public Recipe Stealthcape { get; init; } = null!;
        public Recipe Stinger { get; init; } = null!;
        public Recipe Swordofthedead { get; init; } = null!;
        public Recipe Wattire { get; init; } = null!;
        public Recipe Wblade { get; init; } = null!;
        public Recipe Wbreeches { get; init; } = null!;
        public Recipe Wcap { get; init; } = null!;
        public Recipe Wgloves { get; init; } = null!;
        public Recipe Wingedboots { get; init; } = null!;
        public Recipe Wshoes { get; init; } = null!;
        public Recipe Xbox { get; init; } = null!;
    }
}