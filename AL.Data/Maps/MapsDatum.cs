#region
using System.Linq;
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
    public GMap Abtesting { get; init; } = null!;

    [JsonProperty("arena")]
    public GMap Arena { get; init; } = null!;

    [JsonProperty("bank")]
    public GMap Bank { get; init; } = null!;

    [JsonProperty("bank_b")]
    public GMap BankBasement { get; init; } = null!;

    [JsonProperty("bank_u")]
    public GMap BankUnderground { get; init; } = null!;

    [JsonProperty("batcave")]
    public GMap Batcave { get; init; } = null!;

    [JsonProperty("cave")]
    public GMap Cave { get; init; } = null!;

    [JsonProperty("cgallery")]
    public GMap Cgallery { get; init; } = null!;

    [JsonProperty("crypt")]
    public GMap Crypt { get; init; } = null!;

    [JsonProperty("cyberland")]
    public GMap Cyberland { get; init; } = null!;

    [JsonProperty("d2")]
    public GMap D2 { get; init; } = null!;

    [JsonProperty("d_a1")]
    public GMap DA1 { get; init; } = null!;

    [JsonProperty("d_a2")]
    public GMap DA2 { get; init; } = null!;

    [JsonProperty("d_b1")]
    public GMap DB1 { get; init; } = null!;

    [JsonProperty("d_e")]
    public GMap DE { get; init; } = null!;

    [JsonProperty("desertland")]
    public GMap Desertland { get; init; } = null!;

    [JsonProperty("d_g")]
    public GMap DG { get; init; } = null!;

    [JsonProperty("duelland")]
    public GMap Duelland { get; init; } = null!;

    [JsonProperty("dungeon0")]
    public GMap Dungeon0 { get; init; } = null!;

    [JsonProperty("goobrawl")]
    public GMap Goobrawl { get; init; } = null!;

    [JsonProperty("halloween")]
    public GMap Halloween { get; init; } = null!;

    [JsonProperty("hut")]
    public GMap Hut { get; init; } = null!;

    [JsonProperty("jail")]
    public GMap Jail { get; init; } = null!;

    [JsonProperty("level1")]
    public GMap Level1 { get; init; } = null!;

    [JsonProperty("level2")]
    public GMap Level2 { get; init; } = null!;

    [JsonProperty("level2e")]
    public GMap Level2E { get; init; } = null!;

    [JsonProperty("level2n")]
    public GMap Level2N { get; init; } = null!;

    [JsonProperty("level2s")]
    public GMap Level2S { get; init; } = null!;

    [JsonProperty("level2w")]
    public GMap Level2W { get; init; } = null!;

    [JsonProperty("level3")]
    public GMap Level3 { get; init; } = null!;

    [JsonProperty("level4")]
    public GMap Level4 { get; init; } = null!;

    [JsonProperty("main")]
    public GMap Main { get; init; } = null!;

    [JsonProperty("mansion")]
    public GMap Mansion { get; init; } = null!;

    [JsonProperty("mtunnel")]
    public GMap Mtunnel { get; init; } = null!;

    [JsonProperty("old_bank")]
    public GMap OldBank { get; init; } = null!;

    [JsonProperty("old_main")]
    public GMap OldMain { get; init; } = null!;

    [JsonProperty("original_main")]
    public GMap OriginalMain { get; init; } = null!;

    [JsonProperty("resort")]
    public GMap Resort { get; init; } = null!;

    [JsonProperty("resort_e")]
    public GMap ResortE { get; init; } = null!;

    [JsonProperty("shellsisland")]
    public GMap Shellsisland { get; init; } = null!;

    [JsonProperty("ship0")]
    public GMap Ship0 { get; init; } = null!;

    [JsonProperty("spookytown")]
    public GMap Spookytown { get; init; } = null!;

    [JsonProperty("tavern")]
    public GMap Tavern { get; init; } = null!;

    [JsonProperty("test")]
    public GMap Test { get; init; } = null!;

    [JsonProperty("tomb")]
    public GMap Tomb { get; init; } = null!;

    [JsonProperty("tunnel")]
    public GMap Tunnel { get; init; } = null!;

    [JsonProperty("winter_cave")]
    public GMap WinterCave { get; init; } = null!;

    [JsonProperty("winter_cove")]
    public GMap WinterCove { get; init; } = null!;

    [JsonProperty("winter_inn")]
    public GMap WinterInn { get; init; } = null!;

    [JsonProperty("winter_inn_rooms")]
    public GMap WinterInnRooms { get; init; } = null!;

    [JsonProperty("winter_instance")]
    public GMap WinterInstance { get; init; } = null!;

    [JsonProperty("winterland")]
    public GMap Winterland { get; init; } = null!;

    [JsonProperty("woffice")]
    public GMap Woffice { get; init; } = null!;

    internal override void BuildLookupTable()
    {
        base.BuildLookupTable();

        //map accessors are populated based on the string from the server, not the local copy.
        foreach ((var accessor, var map) in this.Reverse()
                                                .DistinctBy(kvp => kvp.Value.Key))
            map.Accessor = accessor;
    }
}