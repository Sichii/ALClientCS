#region
using System.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
#endregion

namespace AL.Data.Maps;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class MapsDatum : DatumBase<GMap>
{
    [JsonProperty("abtesting")]
    [JsonPropertyName("abtesting")]
    public GMap Abtesting { get; init; } = null!;

    [JsonProperty("arena")]
    [JsonPropertyName("arena")]
    public GMap Arena { get; init; } = null!;

    [JsonProperty("bank")]
    [JsonPropertyName("bank")]
    public GMap Bank { get; init; } = null!;

    [JsonProperty("bank_b")]
    [JsonPropertyName("bank_b")]
    public GMap BankBasement { get; init; } = null!;

    [JsonProperty("bank_u")]
    [JsonPropertyName("bank_u")]
    public GMap BankUnderground { get; init; } = null!;

    [JsonProperty("batcave")]
    [JsonPropertyName("batcave")]
    public GMap Batcave { get; init; } = null!;

    [JsonProperty("cave")]
    [JsonPropertyName("cave")]
    public GMap Cave { get; init; } = null!;

    [JsonProperty("cgallery")]
    [JsonPropertyName("cgallery")]
    public GMap Cgallery { get; init; } = null!;

    [JsonProperty("crypt")]
    [JsonPropertyName("crypt")]
    public GMap Crypt { get; init; } = null!;

    [JsonProperty("cyberland")]
    [JsonPropertyName("cyberland")]
    public GMap Cyberland { get; init; } = null!;

    [JsonProperty("d2")]
    [JsonPropertyName("d2")]
    public GMap D2 { get; init; } = null!;

    [JsonProperty("d_e")]
    [JsonPropertyName("d_e")]
    public GMap DE { get; init; } = null!;

    [JsonProperty("desertland")]
    [JsonPropertyName("desertland")]
    public GMap Desertland { get; init; } = null!;

    [JsonProperty("duelland")]
    [JsonPropertyName("duelland")]
    public GMap Duelland { get; init; } = null!;

    [JsonProperty("dungeon0")]
    [JsonPropertyName("dungeon0")]
    public GMap Dungeon0 { get; init; } = null!;

    [JsonProperty("gateway")]
    [JsonPropertyName("gateway")]
    public GMap Gateway { get; init; } = null!;

    [JsonProperty("goobrawl")]
    [JsonPropertyName("goobrawl")]
    public GMap Goobrawl { get; init; } = null!;

    [JsonProperty("halloween")]
    [JsonPropertyName("halloween")]
    public GMap Halloween { get; init; } = null!;

    [JsonProperty("hut")]
    [JsonPropertyName("hut")]
    public GMap Hut { get; init; } = null!;

    [JsonProperty("jail")]
    [JsonPropertyName("jail")]
    public GMap Jail { get; init; } = null!;

    [JsonProperty("level1")]
    [JsonPropertyName("level1")]
    public GMap Level1 { get; init; } = null!;

    [JsonProperty("level2")]
    [JsonPropertyName("level2")]
    public GMap Level2 { get; init; } = null!;

    [JsonProperty("level2e")]
    [JsonPropertyName("level2e")]
    public GMap Level2E { get; init; } = null!;

    [JsonProperty("level2n")]
    [JsonPropertyName("level2n")]
    public GMap Level2N { get; init; } = null!;

    [JsonProperty("level2s")]
    [JsonPropertyName("level2s")]
    public GMap Level2S { get; init; } = null!;

    [JsonProperty("level2w")]
    [JsonPropertyName("level2w")]
    public GMap Level2W { get; init; } = null!;

    [JsonProperty("level3")]
    [JsonPropertyName("level3")]
    public GMap Level3 { get; init; } = null!;

    [JsonProperty("level4")]
    [JsonPropertyName("level4")]
    public GMap Level4 { get; init; } = null!;

    [JsonProperty("main")]
    [JsonPropertyName("main")]
    public GMap Main { get; init; } = null!;

    [JsonProperty("mansion")]
    [JsonPropertyName("mansion")]
    public GMap Mansion { get; init; } = null!;

    [JsonProperty("mforest")]
    [JsonPropertyName("mforest")]
    public GMap Mforest { get; init; } = null!;

    [JsonProperty("mtunnel")]
    [JsonPropertyName("mtunnel")]
    public GMap Mtunnel { get; init; } = null!;

    [JsonProperty("old_bank")]
    [JsonPropertyName("old_bank")]
    public GMap OldBank { get; init; } = null!;

    [JsonProperty("old_main")]
    [JsonPropertyName("old_main")]
    public GMap OldMain { get; init; } = null!;

    [JsonProperty("original_main")]
    [JsonPropertyName("original_main")]
    public GMap OriginalMain { get; init; } = null!;

    [JsonProperty("resort")]
    [JsonPropertyName("resort")]
    public GMap Resort { get; init; } = null!;

    [JsonProperty("resort_e")]
    [JsonPropertyName("resort_e")]
    public GMap ResortE { get; init; } = null!;

    [JsonProperty("shellsisland")]
    [JsonPropertyName("shellsisland")]
    public GMap Shellsisland { get; init; } = null!;

    [JsonProperty("ship0")]
    [JsonPropertyName("ship0")]
    public GMap Ship0 { get; init; } = null!;

    [JsonProperty("spider_instance")]
    [JsonPropertyName("spider_instance")]
    public GMap SpiderInstance { get; init; } = null!;

    [JsonProperty("spookytown")]
    [JsonPropertyName("spookytown")]
    public GMap Spookytown { get; init; } = null!;

    [JsonProperty("tavern")]
    [JsonPropertyName("tavern")]
    public GMap Tavern { get; init; } = null!;

    [JsonProperty("test")]
    [JsonPropertyName("test")]
    public GMap Test { get; init; } = null!;

    [JsonProperty("tomb")]
    [JsonPropertyName("tomb")]
    public GMap Tomb { get; init; } = null!;

    [JsonProperty("tunnel")]
    [JsonPropertyName("tunnel")]
    public GMap Tunnel { get; init; } = null!;

    [JsonProperty("ucliffs")]
    [JsonPropertyName("ucliffs")]
    public GMap Ucliffs { get; init; } = null!;

    [JsonProperty("uhills")]
    [JsonPropertyName("uhills")]
    public GMap Uhills { get; init; } = null!;

    [JsonProperty("winter_cave")]
    [JsonPropertyName("winter_cave")]
    public GMap WinterCave { get; init; } = null!;

    [JsonProperty("winter_cove")]
    [JsonPropertyName("winter_cove")]
    public GMap WinterCove { get; init; } = null!;

    [JsonProperty("winter_inn")]
    [JsonPropertyName("winter_inn")]
    public GMap WinterInn { get; init; } = null!;

    [JsonProperty("winter_inn_rooms")]
    [JsonPropertyName("winter_inn_rooms")]
    public GMap WinterInnRooms { get; init; } = null!;

    [JsonProperty("winter_instance")]
    [JsonPropertyName("winter_instance")]
    public GMap WinterInstance { get; init; } = null!;

    [JsonProperty("winterland")]
    [JsonPropertyName("winterland")]
    public GMap Winterland { get; init; } = null!;

    [JsonProperty("woffice")]
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