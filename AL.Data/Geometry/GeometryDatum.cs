#region
using System.Linq;
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
    public GGeometry Abtesting { get; init; } = null!;

    [JsonProperty("arena")]
    public GGeometry Arena { get; init; } = null!;

    [JsonProperty("bank")]
    public GGeometry Bank { get; init; } = null!;

    [JsonProperty("bank_b")]
    public GGeometry BankBasement { get; init; } = null!;

    [JsonProperty("bank_u")]
    public GGeometry BankUnderground { get; init; } = null!;

    [JsonProperty("cave")]
    public GGeometry Cave { get; init; } = null!;

    [JsonProperty("cgallery")]
    public GGeometry Cgallery { get; init; } = null!;

    [JsonProperty("crypt")]
    public GGeometry Crypt { get; init; } = null!;

    [JsonProperty("cyberland")]
    public GGeometry Cyberland { get; init; } = null!;

    [JsonProperty("d_a1")]
    public GGeometry DA1 { get; init; } = null!;

    [JsonProperty("d_a2")]
    public GGeometry DA2 { get; init; } = null!;

    [JsonProperty("d_b1")]
    public GGeometry DB1 { get; init; } = null!;

    [JsonProperty("d_e")]
    public GGeometry DE { get; init; } = null!;

    [JsonProperty("desertland")]
    public GGeometry Desertland { get; init; } = null!;

    [JsonProperty("d_g")]
    public GGeometry DG { get; init; } = null!;

    [JsonProperty("duelland")]
    public GGeometry Duelland { get; init; } = null!;

    [JsonProperty("dungeon0")]
    public GGeometry Dungeon0 { get; init; } = null!;

    [JsonProperty("goobrawl")]
    public GGeometry Goobrawl { get; init; } = null!;

    [JsonProperty("halloween")]
    public GGeometry Halloween { get; init; } = null!;

    [JsonProperty("hut")]
    public GGeometry Hut { get; init; } = null!;

    [JsonProperty("jail")]
    public GGeometry Jail { get; init; } = null!;

    [JsonProperty("level1")]
    public GGeometry Level1 { get; init; } = null!;

    [JsonProperty("level2")]
    public GGeometry Level2 { get; init; } = null!;

    [JsonProperty("level2e")]
    public GGeometry Level2E { get; init; } = null!;

    [JsonProperty("level2n")]
    public GGeometry Level2N { get; init; } = null!;

    [JsonProperty("level2s")]
    public GGeometry Level2S { get; init; } = null!;

    [JsonProperty("level2w")]
    public GGeometry Level2W { get; init; } = null!;

    [JsonProperty("level3")]
    public GGeometry Level3 { get; init; } = null!;

    [JsonProperty("level4")]
    public GGeometry Level4 { get; init; } = null!;

    [JsonProperty("main")]
    public GGeometry Main { get; init; } = null!;

    [JsonProperty("mansion")]
    public GGeometry Mansion { get; init; } = null!;

    [JsonProperty("mtunnel")]
    public GGeometry Mtunnel { get; init; } = null!;

    [JsonProperty("resort")]
    public GGeometry Resort { get; init; } = null!;

    [JsonProperty("resort_e")]
    public GGeometry ResortE { get; init; } = null!;

    [JsonProperty("shellsisland")]
    public GGeometry Shellsisland { get; init; } = null!;

    [JsonProperty("ship0")]
    public GGeometry Ship0 { get; init; } = null!;

    [JsonProperty("spookytown")]
    public GGeometry Spookytown { get; init; } = null!;

    [JsonProperty("tavern")]
    public GGeometry Tavern { get; init; } = null!;

    [JsonProperty("test")]
    public GGeometry Test { get; init; } = null!;

    [JsonProperty("tomb")]
    public GGeometry Tomb { get; init; } = null!;

    [JsonProperty("tunnel")]
    public GGeometry Tunnel { get; init; } = null!;

    [JsonProperty("winter_cave")]
    public GGeometry WinterCave { get; init; } = null!;

    [JsonProperty("winter_cove")]
    public GGeometry WinterCove { get; init; } = null!;

    [JsonProperty("winter_inn")]
    public GGeometry WinterInn { get; init; } = null!;

    [JsonProperty("winter_inn_rooms")]
    public GGeometry WinterInnRooms { get; init; } = null!;

    [JsonProperty("winter_instance")]
    public GGeometry WinterInstance { get; init; } = null!;

    [JsonProperty("winterland")]
    public GGeometry Winterland { get; init; } = null!;

    [JsonProperty("woffice")]
    public GGeometry Woffice { get; init; } = null!;

    internal override void BuildLookupTable()
    {
        base.BuildLookupTable();

        //map accessors are populated based on the string from the server, not the local copy.
        foreach ((var accessor, var map) in this.Reverse()
                                                .DistinctBy(kvp => kvp.Value))
            map.Accessor = accessor;
    }
}