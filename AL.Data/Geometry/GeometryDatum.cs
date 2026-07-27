#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Geometry;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class GeometryDatum : DatumBase<GGeometry>
{
    [JsonPropertyName("abtesting")]
    public GGeometry Abtesting { get; init; } = null!;

    [JsonPropertyName("arena")]
    public GGeometry Arena { get; init; } = null!;

    [JsonPropertyName("bank")]
    public GGeometry Bank { get; init; } = null!;

    [JsonPropertyName("bank_b")]
    public GGeometry BankBasement { get; init; } = null!;

    [JsonPropertyName("bank_u")]
    public GGeometry BankUnderground { get; init; } = null!;

    [JsonPropertyName("cave")]
    public GGeometry Cave { get; init; } = null!;

    [JsonPropertyName("cgallery")]
    public GGeometry Cgallery { get; init; } = null!;

    [JsonPropertyName("crypt")]
    public GGeometry Crypt { get; init; } = null!;

    [JsonPropertyName("cyberland")]
    public GGeometry Cyberland { get; init; } = null!;

    [JsonPropertyName("d_e")]

    // ReSharper disable once InconsistentNaming
    public GGeometry DE { get; init; } = null!;

    [JsonPropertyName("desertland")]
    public GGeometry Desertland { get; init; } = null!;

    [JsonPropertyName("duelland")]
    public GGeometry Duelland { get; init; } = null!;

    [JsonPropertyName("dungeon0")]
    public GGeometry Dungeon0 { get; init; } = null!;

    [JsonPropertyName("gateway")]
    public GGeometry Gateway { get; init; } = null!;

    [JsonPropertyName("goobrawl")]
    public GGeometry Goobrawl { get; init; } = null!;

    [JsonPropertyName("halloween")]
    public GGeometry Halloween { get; init; } = null!;

    [JsonPropertyName("hut")]
    public GGeometry Hut { get; init; } = null!;

    [JsonPropertyName("jail")]
    public GGeometry Jail { get; init; } = null!;

    [JsonPropertyName("level1")]
    public GGeometry Level1 { get; init; } = null!;

    [JsonPropertyName("level2")]
    public GGeometry Level2 { get; init; } = null!;

    [JsonPropertyName("level2e")]
    public GGeometry Level2E { get; init; } = null!;

    [JsonPropertyName("level2n")]
    public GGeometry Level2N { get; init; } = null!;

    [JsonPropertyName("level2s")]
    public GGeometry Level2S { get; init; } = null!;

    [JsonPropertyName("level2w")]
    public GGeometry Level2W { get; init; } = null!;

    [JsonPropertyName("level3")]
    public GGeometry Level3 { get; init; } = null!;

    [JsonPropertyName("level4")]
    public GGeometry Level4 { get; init; } = null!;

    [JsonPropertyName("main")]
    public GGeometry Main { get; init; } = null!;

    [JsonPropertyName("mansion")]
    public GGeometry Mansion { get; init; } = null!;

    [JsonPropertyName("mforest")]
    public GGeometry Mforest { get; init; } = null!;

    [JsonPropertyName("mtunnel")]
    public GGeometry Mtunnel { get; init; } = null!;

    [JsonPropertyName("resort")]
    public GGeometry Resort { get; init; } = null!;

    [JsonPropertyName("resort_e")]
    public GGeometry ResortE { get; init; } = null!;

    [JsonPropertyName("shellsisland")]
    public GGeometry Shellsisland { get; init; } = null!;

    [JsonPropertyName("ship0")]
    public GGeometry Ship0 { get; init; } = null!;

    [JsonPropertyName("spider_instance")]
    public GGeometry SpiderInstance { get; init; } = null!;

    [JsonPropertyName("spookytown")]
    public GGeometry Spookytown { get; init; } = null!;

    [JsonPropertyName("tavern")]
    public GGeometry Tavern { get; init; } = null!;

    [JsonPropertyName("test")]
    public GGeometry Test { get; init; } = null!;

    [JsonPropertyName("tomb")]
    public GGeometry Tomb { get; init; } = null!;

    [JsonPropertyName("tunnel")]
    public GGeometry Tunnel { get; init; } = null!;

    [JsonPropertyName("ucliffs")]
    public GGeometry Ucliffs { get; init; } = null!;

    [JsonPropertyName("uhills")]
    public GGeometry Uhills { get; init; } = null!;

    [JsonPropertyName("winter_cave")]
    public GGeometry WinterCave { get; init; } = null!;

    [JsonPropertyName("winter_cove")]
    public GGeometry WinterCove { get; init; } = null!;

    [JsonPropertyName("winter_inn")]
    public GGeometry WinterInn { get; init; } = null!;

    [JsonPropertyName("winter_inn_rooms")]
    public GGeometry WinterInnRooms { get; init; } = null!;

    [JsonPropertyName("winter_instance")]
    public GGeometry WinterInstance { get; init; } = null!;

    [JsonPropertyName("winterland")]
    public GGeometry Winterland { get; init; } = null!;

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