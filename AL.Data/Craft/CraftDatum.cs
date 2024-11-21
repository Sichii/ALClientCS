#region
using Newtonsoft.Json;
#endregion

namespace AL.Data.Craft
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class CraftDatum : DatumBase<Recipe>
    {
        [JsonProperty("armorring")]
        public Recipe Armorring { get; init; } = null!;

        [JsonProperty("basketofeggs")]
        public Recipe Basketofeggs { get; init; } = null!;

        [JsonProperty("bfangamulet")]
        public Recipe Bfangamulet { get; init; } = null!;

        [JsonProperty("bowofthedead")]
        public Recipe Bowofthedead { get; init; } = null!;

        [JsonProperty("bronzeingot")]
        public Recipe Bronzeingot { get; init; } = null!;

        [JsonProperty("cake")]
        public Recipe Cake { get; init; } = null!;

        [JsonProperty("candycanesword")]
        public Recipe Candycanesword { get; init; } = null!;

        [JsonProperty("carrotsword")]
        public Recipe Carrotsword { get; init; } = null!;

        [JsonProperty("cclaw")]
        public Recipe Cclaw { get; init; } = null!;

        [JsonProperty("charmer")]
        public Recipe Charmer { get; init; } = null!;

        [JsonProperty("computer")]
        public Recipe Computer { get; init; } = null!;

        [JsonProperty("ctristone")]
        public Recipe Ctristone { get; init; } = null!;

        [JsonProperty("daggerofthedead")]
        public Recipe Daggerofthedead { get; init; } = null!;

        [JsonProperty("dartgun")]
        public Recipe Dartgun { get; init; } = null!;

        [JsonProperty("elixirdex1")]
        public Recipe Elixirdex1 { get; init; } = null!;

        [JsonProperty("elixirdex2")]
        public Recipe Elixirdex2 { get; init; } = null!;

        [JsonProperty("elixirfires")]
        public Recipe Elixirfireres { get; init; } = null!;

        [JsonProperty("elixirfzres")]
        public Recipe Elixirfreezeres { get; init; } = null!;

        [JsonProperty("elixirint1")]
        public Recipe Elixirint1 { get; init; } = null!;

        [JsonProperty("elixirint2")]
        public Recipe Elixirint2 { get; init; } = null!;

        [JsonProperty("elixirpnres")]
        public Recipe Elixirpnres { get; init; } = null!;

        [JsonProperty("elixirstr1")]
        public Recipe Elixirstr1 { get; init; } = null!;

        [JsonProperty("elixirstr2")]
        public Recipe Elixirstr2 { get; init; } = null!;

        [JsonProperty("elixirvit1")]
        public Recipe Elixirvit1 { get; init; } = null!;

        [JsonProperty("elixirvit2")]
        public Recipe Elixirvit2 { get; init; } = null!;

        [JsonProperty("fclaw")]
        public Recipe Fclaw { get; init; } = null!;

        [JsonProperty("fierygloves")]
        public Recipe Fierygloves { get; init; } = null!;

        [JsonProperty("fireblade")]
        public Recipe Fireblade { get; init; } = null!;

        [JsonProperty("firebow")]
        public Recipe Firebow { get; init; } = null!;

        [JsonProperty("firestaff")]
        public Recipe Firestaff { get; init; } = null!;

        [JsonProperty("firestars")]
        public Recipe Firestars { get; init; } = null!;

        [JsonProperty("frostbow")]
        public Recipe Frostbow { get; init; } = null!;

        [JsonProperty("froststaff")]
        public Recipe Froststaff { get; init; } = null!;

        [JsonProperty("fsword")]
        public Recipe Fsword { get; init; } = null!;

        [JsonProperty("goldingot")]
        public Recipe Goldingot { get; init; } = null!;

        [JsonProperty("gstaff")]
        public Recipe Gstaff { get; init; } = null!;

        [JsonProperty("harpybow")]
        public Recipe Harpybow { get; init; } = null!;

        [JsonProperty("hbow")]
        public Recipe Hbow { get; init; } = null!;

        [JsonProperty("heartwood")]
        public Recipe Heartwood { get; init; } = null!;

        [JsonProperty("lbelt")]
        public Recipe Lbelt { get; init; } = null!;

        [JsonProperty("maceofthedead")]
        public Recipe Maceofthedead { get; init; } = null!;

        [JsonProperty("merry")]
        public Recipe Merry { get; init; } = null!;

        [JsonProperty("mushroomstaff")]
        public Recipe Mushroomstaff { get; init; } = null!;

        [JsonProperty("offeringx")]
        public Recipe Offeringx { get; init; } = null!;

        [JsonProperty("orbg")]
        public Recipe Orbg { get; init; } = null!;

        [JsonProperty("ornamentstaff")]
        public Recipe Ornamentstaff { get; init; } = null!;

        [JsonProperty("pickaxe")]
        public Recipe Pickaxe { get; init; } = null!;

        [JsonProperty("platinumingot")]
        public Recipe Platinumingot { get; init; } = null!;

        [JsonProperty("pmaceofthedead")]
        public Recipe Pmaceofthedead { get; init; } = null!;

        [JsonProperty("pouchbow")]
        public Recipe Pouchbow { get; init; } = null!;

        [JsonProperty("quiver")]
        public Recipe Quiver { get; init; } = null!;

        [JsonProperty("resistancering")]
        public Recipe Resistancering { get; init; } = null!;

        [JsonProperty("rod")]
        public Recipe Rod { get; init; } = null!;

        [JsonProperty("slimestaff")]
        public Recipe Slimestaff { get; init; } = null!;

        [JsonProperty("snakeoil")]
        public Recipe Snakeoil { get; init; } = null!;

        [JsonProperty("snowflakes")]
        public Recipe Snowflakes { get; init; } = null!;

        [JsonProperty("spearofthedead")]
        public Recipe Spearofthedead { get; init; } = null!;

        [JsonProperty("staffofthedead")]
        public Recipe Staffofthedead { get; init; } = null!;

        [JsonProperty("stealthcape")]
        public Recipe Stealthcape { get; init; } = null!;

        [JsonProperty("stinger")]
        public Recipe Stinger { get; init; } = null!;

        [JsonProperty("supercomputer")]
        public Recipe Supercomputer { get; init; } = null!;

        [JsonProperty("swordofthedead")]
        public Recipe Swordofthedead { get; init; } = null!;

        [JsonProperty("wattire")]
        public Recipe Wattire { get; init; } = null!;

        [JsonProperty("wblade")]
        public Recipe Wblade { get; init; } = null!;

        [JsonProperty("wbreeches")]
        public Recipe Wbreeches { get; init; } = null!;

        [JsonProperty("wcap")]
        public Recipe Wcap { get; init; } = null!;

        [JsonProperty("wgloves")]
        public Recipe Wgloves { get; init; } = null!;

        [JsonProperty("wingedboots")]
        public Recipe Wingedboots { get; init; } = null!;

        [JsonProperty("wshoes")]
        public Recipe Wshoes { get; init; } = null!;

        [JsonProperty("xbox")]
        public Recipe Xbox { get; init; } = null!;
    }
}