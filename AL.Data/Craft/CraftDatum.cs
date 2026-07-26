#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Craft;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class CraftDatum : DatumBase<Recipe>
{
    [JsonPropertyName("armorring")]
    public Recipe Armorring { get; init; } = null!;

    [JsonPropertyName("basketofeggs")]
    public Recipe Basketofeggs { get; init; } = null!;

    [JsonPropertyName("bfangamulet")]
    public Recipe Bfangamulet { get; init; } = null!;

    [JsonPropertyName("bowofthedead")]
    public Recipe Bowofthedead { get; init; } = null!;

    [JsonPropertyName("bronzeingot")]
    public Recipe Bronzeingot { get; init; } = null!;

    [JsonPropertyName("cake")]
    public Recipe Cake { get; init; } = null!;

    [JsonPropertyName("candycanesword")]
    public Recipe Candycanesword { get; init; } = null!;

    [JsonPropertyName("carrotsword")]
    public Recipe Carrotsword { get; init; } = null!;

    [JsonPropertyName("cclaw")]
    public Recipe Cclaw { get; init; } = null!;

    [JsonPropertyName("charmer")]
    public Recipe Charmer { get; init; } = null!;

    [JsonPropertyName("cocoon")]
    public Recipe Cocoon { get; init; } = null!;

    [JsonPropertyName("computer")]
    public Recipe Computer { get; init; } = null!;

    [JsonPropertyName("ctristone")]
    public Recipe Ctristone { get; init; } = null!;

    [JsonPropertyName("daggerofthedead")]
    public Recipe Daggerofthedead { get; init; } = null!;

    [JsonPropertyName("dartgun")]
    public Recipe Dartgun { get; init; } = null!;

    [JsonPropertyName("elixirdex1")]
    public Recipe Elixirdex1 { get; init; } = null!;

    [JsonPropertyName("elixirdex2")]
    public Recipe Elixirdex2 { get; init; } = null!;

    [JsonPropertyName("elixirfires")]
    public Recipe Elixirfireres { get; init; } = null!;

    [JsonPropertyName("elixirfzres")]
    public Recipe Elixirfreezeres { get; init; } = null!;

    [JsonPropertyName("elixirint1")]
    public Recipe Elixirint1 { get; init; } = null!;

    [JsonPropertyName("elixirint2")]
    public Recipe Elixirint2 { get; init; } = null!;

    [JsonPropertyName("elixirpnres")]
    public Recipe Elixirpnres { get; init; } = null!;

    [JsonPropertyName("elixirstr1")]
    public Recipe Elixirstr1 { get; init; } = null!;

    [JsonPropertyName("elixirstr2")]
    public Recipe Elixirstr2 { get; init; } = null!;

    [JsonPropertyName("elixirvit1")]
    public Recipe Elixirvit1 { get; init; } = null!;

    [JsonPropertyName("elixirvit2")]
    public Recipe Elixirvit2 { get; init; } = null!;

    [JsonPropertyName("fclaw")]
    public Recipe Fclaw { get; init; } = null!;

    [JsonPropertyName("fierygloves")]
    public Recipe Fierygloves { get; init; } = null!;

    [JsonPropertyName("fireblade")]
    public Recipe Fireblade { get; init; } = null!;

    [JsonPropertyName("firebow")]
    public Recipe Firebow { get; init; } = null!;

    [JsonPropertyName("firestaff")]
    public Recipe Firestaff { get; init; } = null!;

    [JsonPropertyName("firestars")]
    public Recipe Firestars { get; init; } = null!;

    [JsonPropertyName("frostbow")]
    public Recipe Frostbow { get; init; } = null!;

    [JsonPropertyName("froststaff")]
    public Recipe Froststaff { get; init; } = null!;

    [JsonPropertyName("fsword")]
    public Recipe Fsword { get; init; } = null!;

    [JsonPropertyName("goldingot")]
    public Recipe Goldingot { get; init; } = null!;

    [JsonPropertyName("gstaff")]
    public Recipe Gstaff { get; init; } = null!;

    [JsonPropertyName("harpybow")]
    public Recipe Harpybow { get; init; } = null!;

    [JsonPropertyName("hbow")]
    public Recipe Hbow { get; init; } = null!;

    [JsonPropertyName("heartwood")]
    public Recipe Heartwood { get; init; } = null!;

    [JsonPropertyName("lbelt")]
    public Recipe Lbelt { get; init; } = null!;

    [JsonPropertyName("maceofthedead")]
    public Recipe Maceofthedead { get; init; } = null!;

    [JsonPropertyName("merry")]
    public Recipe Merry { get; init; } = null!;

    [JsonPropertyName("mushroomstaff")]
    public Recipe Mushroomstaff { get; init; } = null!;

    [JsonPropertyName("offeringx")]
    public Recipe Offeringx { get; init; } = null!;

    [JsonPropertyName("orba")]
    public Recipe Orba { get; init; } = null!;

    [JsonPropertyName("orbg")]
    public Recipe Orbg { get; init; } = null!;

    [JsonPropertyName("ornamentstaff")]
    public Recipe Ornamentstaff { get; init; } = null!;

    [JsonPropertyName("pickaxe")]
    public Recipe Pickaxe { get; init; } = null!;

    [JsonPropertyName("platinumingot")]
    public Recipe Platinumingot { get; init; } = null!;

    [JsonPropertyName("pmaceofthedead")]
    public Recipe Pmaceofthedead { get; init; } = null!;

    [JsonPropertyName("pouchbow")]
    public Recipe Pouchbow { get; init; } = null!;

    [JsonPropertyName("quiver")]
    public Recipe Quiver { get; init; } = null!;

    [JsonPropertyName("resistancering")]
    public Recipe Resistancering { get; init; } = null!;

    [JsonPropertyName("rod")]
    public Recipe Rod { get; init; } = null!;

    [JsonPropertyName("slimestaff")]
    public Recipe Slimestaff { get; init; } = null!;

    [JsonPropertyName("snakeoil")]
    public Recipe Snakeoil { get; init; } = null!;

    [JsonPropertyName("snowflakes")]
    public Recipe Snowflakes { get; init; } = null!;

    [JsonPropertyName("spearofthedead")]
    public Recipe Spearofthedead { get; init; } = null!;

    [JsonPropertyName("staffofthedead")]
    public Recipe Staffofthedead { get; init; } = null!;

    [JsonPropertyName("stealthcape")]
    public Recipe Stealthcape { get; init; } = null!;

    [JsonPropertyName("stinger")]
    public Recipe Stinger { get; init; } = null!;

    [JsonPropertyName("supercomputer")]
    public Recipe Supercomputer { get; init; } = null!;

    [JsonPropertyName("swordofthedead")]
    public Recipe Swordofthedead { get; init; } = null!;

    [JsonPropertyName("wattire")]
    public Recipe Wattire { get; init; } = null!;

    [JsonPropertyName("wblade")]
    public Recipe Wblade { get; init; } = null!;

    [JsonPropertyName("wbreeches")]
    public Recipe Wbreeches { get; init; } = null!;

    [JsonPropertyName("wcap")]
    public Recipe Wcap { get; init; } = null!;

    [JsonPropertyName("weaver")]
    public Recipe Weaver { get; init; } = null!;

    [JsonPropertyName("wgloves")]
    public Recipe Wgloves { get; init; } = null!;

    [JsonPropertyName("wingedboots")]
    public Recipe Wingedboots { get; init; } = null!;

    [JsonPropertyName("wshoes")]
    public Recipe Wshoes { get; init; } = null!;

    [JsonPropertyName("xbox")]
    public Recipe Xbox { get; init; } = null!;
}