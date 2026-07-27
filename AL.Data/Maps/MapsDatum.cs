#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Maps;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class MapsDatum : DatumBase<GMap>
{
    [JsonPropertyName("abtesting")]
    public GMap Abtesting { get; init; } = null!;

    [JsonPropertyName("arena")]
    public GMap Arena { get; init; } = null!;

    [JsonPropertyName("bank")]
    public GMap Bank { get; init; } = null!;

    [JsonPropertyName("bank_b")]
    public GMap BankBasement { get; init; } = null!;

    [JsonPropertyName("bank_u")]
    public GMap BankUnderground { get; init; } = null!;

    [JsonPropertyName("batcave")]
    public GMap Batcave { get; init; } = null!;

    [JsonPropertyName("cave")]
    public GMap Cave { get; init; } = null!;

    [JsonPropertyName("cgallery")]
    public GMap Cgallery { get; init; } = null!;

    [JsonPropertyName("crypt")]
    public GMap Crypt { get; init; } = null!;

    [JsonPropertyName("cyberland")]
    public GMap Cyberland { get; init; } = null!;

    [JsonPropertyName("d2")]
    public GMap D2 { get; init; } = null!;

    [JsonPropertyName("d_e")]

    // ReSharper disable once InconsistentNaming
    public GMap DE { get; init; } = null!;

    [JsonPropertyName("desertland")]
    public GMap Desertland { get; init; } = null!;

    [JsonPropertyName("duelland")]
    public GMap Duelland { get; init; } = null!;

    [JsonPropertyName("dungeon0")]
    public GMap Dungeon0 { get; init; } = null!;

    [JsonPropertyName("gateway")]
    public GMap Gateway { get; init; } = null!;

    [JsonPropertyName("goobrawl")]
    public GMap Goobrawl { get; init; } = null!;

    [JsonPropertyName("halloween")]
    public GMap Halloween { get; init; } = null!;

    [JsonPropertyName("hut")]
    public GMap Hut { get; init; } = null!;

    [JsonPropertyName("jail")]
    public GMap Jail { get; init; } = null!;

    [JsonPropertyName("level1")]
    public GMap Level1 { get; init; } = null!;

    [JsonPropertyName("level2")]
    public GMap Level2 { get; init; } = null!;

    [JsonPropertyName("level2e")]
    public GMap Level2E { get; init; } = null!;

    [JsonPropertyName("level2n")]
    public GMap Level2N { get; init; } = null!;

    [JsonPropertyName("level2s")]
    public GMap Level2S { get; init; } = null!;

    [JsonPropertyName("level2w")]
    public GMap Level2W { get; init; } = null!;

    [JsonPropertyName("level3")]
    public GMap Level3 { get; init; } = null!;

    [JsonPropertyName("level4")]
    public GMap Level4 { get; init; } = null!;

    [JsonPropertyName("main")]
    public GMap Main { get; init; } = null!;

    [JsonPropertyName("mansion")]
    public GMap Mansion { get; init; } = null!;

    [JsonPropertyName("mforest")]
    public GMap Mforest { get; init; } = null!;

    [JsonPropertyName("mtunnel")]
    public GMap Mtunnel { get; init; } = null!;

    [JsonPropertyName("old_bank")]
    public GMap OldBank { get; init; } = null!;

    [JsonPropertyName("old_main")]
    public GMap OldMain { get; init; } = null!;

    [JsonPropertyName("original_main")]
    public GMap OriginalMain { get; init; } = null!;

    [JsonPropertyName("resort")]
    public GMap Resort { get; init; } = null!;

    [JsonPropertyName("resort_e")]
    public GMap ResortE { get; init; } = null!;

    [JsonPropertyName("shellsisland")]
    public GMap Shellsisland { get; init; } = null!;

    [JsonPropertyName("ship0")]
    public GMap Ship0 { get; init; } = null!;

    [JsonPropertyName("spider_instance")]
    public GMap SpiderInstance { get; init; } = null!;

    [JsonPropertyName("spookytown")]
    public GMap Spookytown { get; init; } = null!;

    [JsonPropertyName("tavern")]
    public GMap Tavern { get; init; } = null!;

    [JsonPropertyName("test")]
    public GMap Test { get; init; } = null!;

    [JsonPropertyName("tomb")]
    public GMap Tomb { get; init; } = null!;

    [JsonPropertyName("tunnel")]
    public GMap Tunnel { get; init; } = null!;

    [JsonPropertyName("ucliffs")]
    public GMap Ucliffs { get; init; } = null!;

    [JsonPropertyName("uhills")]
    public GMap Uhills { get; init; } = null!;

    [JsonPropertyName("winter_cave")]
    public GMap WinterCave { get; init; } = null!;

    [JsonPropertyName("winter_cove")]
    public GMap WinterCove { get; init; } = null!;

    [JsonPropertyName("winter_inn")]
    public GMap WinterInn { get; init; } = null!;

    [JsonPropertyName("winter_inn_rooms")]
    public GMap WinterInnRooms { get; init; } = null!;

    [JsonPropertyName("winter_instance")]
    public GMap WinterInstance { get; init; } = null!;

    [JsonPropertyName("winterland")]
    public GMap Winterland { get; init; } = null!;

    [JsonPropertyName("woffice")]
    public GMap Woffice { get; init; } = null!;

    internal override void BuildLookupTable()
    {
        base.BuildLookupTable();

        //map accessors are populated based on the string from the server, not the local copy.
        foreach ((var accessor, var map) in Entries.Reverse()
                                                   .DistinctBy(kvp => kvp.Value.Key))
            map.Accessor = accessor;
    }
}