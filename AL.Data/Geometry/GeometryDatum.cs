#region
using System.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
#endregion

namespace AL.Data.Geometry;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class GeometryDatum : DatumBase<GGeometry>
{
    [JsonProperty("abtesting")]
    [JsonPropertyName("abtesting")]
    public GGeometry Abtesting { get; init; } = null!;

    [JsonProperty("arena")]
    [JsonPropertyName("arena")]
    public GGeometry Arena { get; init; } = null!;

    [JsonProperty("bank")]
    [JsonPropertyName("bank")]
    public GGeometry Bank { get; init; } = null!;

    [JsonProperty("bank_b")]
    [JsonPropertyName("bank_b")]
    public GGeometry BankBasement { get; init; } = null!;

    [JsonProperty("bank_u")]
    [JsonPropertyName("bank_u")]
    public GGeometry BankUnderground { get; init; } = null!;

    [JsonProperty("cave")]
    [JsonPropertyName("cave")]
    public GGeometry Cave { get; init; } = null!;

    [JsonProperty("cgallery")]
    [JsonPropertyName("cgallery")]
    public GGeometry Cgallery { get; init; } = null!;

    [JsonProperty("crypt")]
    [JsonPropertyName("crypt")]
    public GGeometry Crypt { get; init; } = null!;

    [JsonProperty("cyberland")]
    [JsonPropertyName("cyberland")]
    public GGeometry Cyberland { get; init; } = null!;

    [JsonProperty("d_e")]
    [JsonPropertyName("d_e")]
    public GGeometry DE { get; init; } = null!;

    [JsonProperty("desertland")]
    [JsonPropertyName("desertland")]
    public GGeometry Desertland { get; init; } = null!;

    [JsonProperty("duelland")]
    [JsonPropertyName("duelland")]
    public GGeometry Duelland { get; init; } = null!;

    [JsonProperty("dungeon0")]
    [JsonPropertyName("dungeon0")]
    public GGeometry Dungeon0 { get; init; } = null!;

    [JsonProperty("gateway")]
    [JsonPropertyName("gateway")]
    public GGeometry Gateway { get; init; } = null!;

    [JsonProperty("goobrawl")]
    [JsonPropertyName("goobrawl")]
    public GGeometry Goobrawl { get; init; } = null!;

    [JsonProperty("halloween")]
    [JsonPropertyName("halloween")]
    public GGeometry Halloween { get; init; } = null!;

    [JsonProperty("hut")]
    [JsonPropertyName("hut")]
    public GGeometry Hut { get; init; } = null!;

    [JsonProperty("jail")]
    [JsonPropertyName("jail")]
    public GGeometry Jail { get; init; } = null!;

    [JsonProperty("level1")]
    [JsonPropertyName("level1")]
    public GGeometry Level1 { get; init; } = null!;

    [JsonProperty("level2")]
    [JsonPropertyName("level2")]
    public GGeometry Level2 { get; init; } = null!;

    [JsonProperty("level2e")]
    [JsonPropertyName("level2e")]
    public GGeometry Level2E { get; init; } = null!;

    [JsonProperty("level2n")]
    [JsonPropertyName("level2n")]
    public GGeometry Level2N { get; init; } = null!;

    [JsonProperty("level2s")]
    [JsonPropertyName("level2s")]
    public GGeometry Level2S { get; init; } = null!;

    [JsonProperty("level2w")]
    [JsonPropertyName("level2w")]
    public GGeometry Level2W { get; init; } = null!;

    [JsonProperty("level3")]
    [JsonPropertyName("level3")]
    public GGeometry Level3 { get; init; } = null!;

    [JsonProperty("level4")]
    [JsonPropertyName("level4")]
    public GGeometry Level4 { get; init; } = null!;

    [JsonProperty("main")]
    [JsonPropertyName("main")]
    public GGeometry Main { get; init; } = null!;

    [JsonProperty("mansion")]
    [JsonPropertyName("mansion")]
    public GGeometry Mansion { get; init; } = null!;

    [JsonProperty("mforest")]
    [JsonPropertyName("mforest")]
    public GGeometry Mforest { get; init; } = null!;

    [JsonProperty("mtunnel")]
    [JsonPropertyName("mtunnel")]
    public GGeometry Mtunnel { get; init; } = null!;

    [JsonProperty("resort")]
    [JsonPropertyName("resort")]
    public GGeometry Resort { get; init; } = null!;

    [JsonProperty("resort_e")]
    [JsonPropertyName("resort_e")]
    public GGeometry ResortE { get; init; } = null!;

    [JsonProperty("shellsisland")]
    [JsonPropertyName("shellsisland")]
    public GGeometry Shellsisland { get; init; } = null!;

    [JsonProperty("ship0")]
    [JsonPropertyName("ship0")]
    public GGeometry Ship0 { get; init; } = null!;

    [JsonProperty("spider_instance")]
    [JsonPropertyName("spider_instance")]
    public GGeometry SpiderInstance { get; init; } = null!;

    [JsonProperty("spookytown")]
    [JsonPropertyName("spookytown")]
    public GGeometry Spookytown { get; init; } = null!;

    [JsonProperty("tavern")]
    [JsonPropertyName("tavern")]
    public GGeometry Tavern { get; init; } = null!;

    [JsonProperty("test")]
    [JsonPropertyName("test")]
    public GGeometry Test { get; init; } = null!;

    [JsonProperty("tomb")]
    [JsonPropertyName("tomb")]
    public GGeometry Tomb { get; init; } = null!;

    [JsonProperty("tunnel")]
    [JsonPropertyName("tunnel")]
    public GGeometry Tunnel { get; init; } = null!;

    [JsonProperty("ucliffs")]
    [JsonPropertyName("ucliffs")]
    public GGeometry Ucliffs { get; init; } = null!;

    [JsonProperty("uhills")]
    [JsonPropertyName("uhills")]
    public GGeometry Uhills { get; init; } = null!;

    [JsonProperty("winter_cave")]
    [JsonPropertyName("winter_cave")]
    public GGeometry WinterCave { get; init; } = null!;

    [JsonProperty("winter_cove")]
    [JsonPropertyName("winter_cove")]
    public GGeometry WinterCove { get; init; } = null!;

    [JsonProperty("winter_inn")]
    [JsonPropertyName("winter_inn")]
    public GGeometry WinterInn { get; init; } = null!;

    [JsonProperty("winter_inn_rooms")]
    [JsonPropertyName("winter_inn_rooms")]
    public GGeometry WinterInnRooms { get; init; } = null!;

    [JsonProperty("winter_instance")]
    [JsonPropertyName("winter_instance")]
    public GGeometry WinterInstance { get; init; } = null!;

    [JsonProperty("winterland")]
    [JsonPropertyName("winterland")]
    public GGeometry Winterland { get; init; } = null!;

    [JsonProperty("woffice")]
    [JsonPropertyName("woffice")]
    public GGeometry Woffice { get; init; } = null!;

    internal override void BuildLookupTable()
    {
        base.BuildLookupTable();

        //map accessors are populated based on the string from the server, not the local copy.
        foreach ((var accessor, var map) in Entries.Reverse()
                                                   .DistinctBy(kvp => kvp.Value))
            map.Accessor = accessor;
    }
}