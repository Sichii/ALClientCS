#region
using System.Text.Json.Serialization;
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
        [JsonPropertyName("armorring")]
        public Recipe Armorring { get; init; } = null!;

        [JsonProperty("basketofeggs")]
        [JsonPropertyName("basketofeggs")]
        public Recipe Basketofeggs { get; init; } = null!;

        [JsonProperty("bfangamulet")]
        [JsonPropertyName("bfangamulet")]
        public Recipe Bfangamulet { get; init; } = null!;

        [JsonProperty("bowofthedead")]
        [JsonPropertyName("bowofthedead")]
        public Recipe Bowofthedead { get; init; } = null!;

        [JsonProperty("bronzeingot")]
        [JsonPropertyName("bronzeingot")]
        public Recipe Bronzeingot { get; init; } = null!;

        [JsonProperty("cake")]
        [JsonPropertyName("cake")]
        public Recipe Cake { get; init; } = null!;

        [JsonProperty("candycanesword")]
        [JsonPropertyName("candycanesword")]
        public Recipe Candycanesword { get; init; } = null!;

        [JsonProperty("carrotsword")]
        [JsonPropertyName("carrotsword")]
        public Recipe Carrotsword { get; init; } = null!;

        [JsonProperty("cclaw")]
        [JsonPropertyName("cclaw")]
        public Recipe Cclaw { get; init; } = null!;

        [JsonProperty("charmer")]
        [JsonPropertyName("charmer")]
        public Recipe Charmer { get; init; } = null!;

        [JsonProperty("cocoon")]
        [JsonPropertyName("cocoon")]
        public Recipe Cocoon { get; init; } = null!;

        [JsonProperty("computer")]
        [JsonPropertyName("computer")]
        public Recipe Computer { get; init; } = null!;

        [JsonProperty("ctristone")]
        [JsonPropertyName("ctristone")]
        public Recipe Ctristone { get; init; } = null!;

        [JsonProperty("daggerofthedead")]
        [JsonPropertyName("daggerofthedead")]
        public Recipe Daggerofthedead { get; init; } = null!;

        [JsonProperty("dartgun")]
        [JsonPropertyName("dartgun")]
        public Recipe Dartgun { get; init; } = null!;

        [JsonProperty("elixirdex1")]
        [JsonPropertyName("elixirdex1")]
        public Recipe Elixirdex1 { get; init; } = null!;

        [JsonProperty("elixirdex2")]
        [JsonPropertyName("elixirdex2")]
        public Recipe Elixirdex2 { get; init; } = null!;

        [JsonProperty("elixirfires")]
        [JsonPropertyName("elixirfires")]
        public Recipe Elixirfireres { get; init; } = null!;

        [JsonProperty("elixirfzres")]
        [JsonPropertyName("elixirfzres")]
        public Recipe Elixirfreezeres { get; init; } = null!;

        [JsonProperty("elixirint1")]
        [JsonPropertyName("elixirint1")]
        public Recipe Elixirint1 { get; init; } = null!;

        [JsonProperty("elixirint2")]
        [JsonPropertyName("elixirint2")]
        public Recipe Elixirint2 { get; init; } = null!;

        [JsonProperty("elixirpnres")]
        [JsonPropertyName("elixirpnres")]
        public Recipe Elixirpnres { get; init; } = null!;

        [JsonProperty("elixirstr1")]
        [JsonPropertyName("elixirstr1")]
        public Recipe Elixirstr1 { get; init; } = null!;

        [JsonProperty("elixirstr2")]
        [JsonPropertyName("elixirstr2")]
        public Recipe Elixirstr2 { get; init; } = null!;

        [JsonProperty("elixirvit1")]
        [JsonPropertyName("elixirvit1")]
        public Recipe Elixirvit1 { get; init; } = null!;

        [JsonProperty("elixirvit2")]
        [JsonPropertyName("elixirvit2")]
        public Recipe Elixirvit2 { get; init; } = null!;

        [JsonProperty("fclaw")]
        [JsonPropertyName("fclaw")]
        public Recipe Fclaw { get; init; } = null!;

        [JsonProperty("fierygloves")]
        [JsonPropertyName("fierygloves")]
        public Recipe Fierygloves { get; init; } = null!;

        [JsonProperty("fireblade")]
        [JsonPropertyName("fireblade")]
        public Recipe Fireblade { get; init; } = null!;

        [JsonProperty("firebow")]
        [JsonPropertyName("firebow")]
        public Recipe Firebow { get; init; } = null!;

        [JsonProperty("firestaff")]
        [JsonPropertyName("firestaff")]
        public Recipe Firestaff { get; init; } = null!;

        [JsonProperty("firestars")]
        [JsonPropertyName("firestars")]
        public Recipe Firestars { get; init; } = null!;

        [JsonProperty("frostbow")]
        [JsonPropertyName("frostbow")]
        public Recipe Frostbow { get; init; } = null!;

        [JsonProperty("froststaff")]
        [JsonPropertyName("froststaff")]
        public Recipe Froststaff { get; init; } = null!;

        [JsonProperty("fsword")]
        [JsonPropertyName("fsword")]
        public Recipe Fsword { get; init; } = null!;

        [JsonProperty("goldingot")]
        [JsonPropertyName("goldingot")]
        public Recipe Goldingot { get; init; } = null!;

        [JsonProperty("gstaff")]
        [JsonPropertyName("gstaff")]
        public Recipe Gstaff { get; init; } = null!;

        [JsonProperty("harpybow")]
        [JsonPropertyName("harpybow")]
        public Recipe Harpybow { get; init; } = null!;

        [JsonProperty("hbow")]
        [JsonPropertyName("hbow")]
        public Recipe Hbow { get; init; } = null!;

        [JsonProperty("heartwood")]
        [JsonPropertyName("heartwood")]
        public Recipe Heartwood { get; init; } = null!;

        [JsonProperty("lbelt")]
        [JsonPropertyName("lbelt")]
        public Recipe Lbelt { get; init; } = null!;

        [JsonProperty("maceofthedead")]
        [JsonPropertyName("maceofthedead")]
        public Recipe Maceofthedead { get; init; } = null!;

        [JsonProperty("merry")]
        [JsonPropertyName("merry")]
        public Recipe Merry { get; init; } = null!;

        [JsonProperty("mushroomstaff")]
        [JsonPropertyName("mushroomstaff")]
        public Recipe Mushroomstaff { get; init; } = null!;

        [JsonProperty("offeringx")]
        [JsonPropertyName("offeringx")]
        public Recipe Offeringx { get; init; } = null!;

        [JsonProperty("orba")]
        [JsonPropertyName("orba")]
        public Recipe Orba { get; init; } = null!;

        [JsonProperty("orbg")]
        [JsonPropertyName("orbg")]
        public Recipe Orbg { get; init; } = null!;

        [JsonProperty("ornamentstaff")]
        [JsonPropertyName("ornamentstaff")]
        public Recipe Ornamentstaff { get; init; } = null!;

        [JsonProperty("pickaxe")]
        [JsonPropertyName("pickaxe")]
        public Recipe Pickaxe { get; init; } = null!;

        [JsonProperty("platinumingot")]
        [JsonPropertyName("platinumingot")]
        public Recipe Platinumingot { get; init; } = null!;

        [JsonProperty("pmaceofthedead")]
        [JsonPropertyName("pmaceofthedead")]
        public Recipe Pmaceofthedead { get; init; } = null!;

        [JsonProperty("pouchbow")]
        [JsonPropertyName("pouchbow")]
        public Recipe Pouchbow { get; init; } = null!;

        [JsonProperty("quiver")]
        [JsonPropertyName("quiver")]
        public Recipe Quiver { get; init; } = null!;

        [JsonProperty("resistancering")]
        [JsonPropertyName("resistancering")]
        public Recipe Resistancering { get; init; } = null!;

        [JsonProperty("rod")]
        [JsonPropertyName("rod")]
        public Recipe Rod { get; init; } = null!;

        [JsonProperty("slimestaff")]
        [JsonPropertyName("slimestaff")]
        public Recipe Slimestaff { get; init; } = null!;

        [JsonProperty("snakeoil")]
        [JsonPropertyName("snakeoil")]
        public Recipe Snakeoil { get; init; } = null!;

        [JsonProperty("snowflakes")]
        [JsonPropertyName("snowflakes")]
        public Recipe Snowflakes { get; init; } = null!;

        [JsonProperty("spearofthedead")]
        [JsonPropertyName("spearofthedead")]
        public Recipe Spearofthedead { get; init; } = null!;

        [JsonProperty("staffofthedead")]
        [JsonPropertyName("staffofthedead")]
        public Recipe Staffofthedead { get; init; } = null!;

        [JsonProperty("stealthcape")]
        [JsonPropertyName("stealthcape")]
        public Recipe Stealthcape { get; init; } = null!;

        [JsonProperty("stinger")]
        [JsonPropertyName("stinger")]
        public Recipe Stinger { get; init; } = null!;

        [JsonProperty("supercomputer")]
        [JsonPropertyName("supercomputer")]
        public Recipe Supercomputer { get; init; } = null!;

        [JsonProperty("swordofthedead")]
        [JsonPropertyName("swordofthedead")]
        public Recipe Swordofthedead { get; init; } = null!;

        [JsonProperty("wattire")]
        [JsonPropertyName("wattire")]
        public Recipe Wattire { get; init; } = null!;

        [JsonProperty("wblade")]
        [JsonPropertyName("wblade")]
        public Recipe Wblade { get; init; } = null!;

        [JsonProperty("wbreeches")]
        [JsonPropertyName("wbreeches")]
        public Recipe Wbreeches { get; init; } = null!;

        [JsonProperty("wcap")]
        [JsonPropertyName("wcap")]
        public Recipe Wcap { get; init; } = null!;

        [JsonProperty("weaver")]
        [JsonPropertyName("weaver")]
        public Recipe Weaver { get; init; } = null!;

        [JsonProperty("wgloves")]
        [JsonPropertyName("wgloves")]
        public Recipe Wgloves { get; init; } = null!;

        [JsonProperty("wingedboots")]
        [JsonPropertyName("wingedboots")]
        public Recipe Wingedboots { get; init; } = null!;

        [JsonProperty("wshoes")]
        [JsonPropertyName("wshoes")]
        public Recipe Wshoes { get; init; } = null!;

        [JsonProperty("xbox")]
        [JsonPropertyName("xbox")]
        public Recipe Xbox { get; init; } = null!;
    }
}