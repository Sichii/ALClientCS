using System.Linq;
using Chaos.Core.Extensions;
using Newtonsoft.Json;

namespace AL.Data.Geometry
{
       /// <summary>
       ///     <inheritdoc />
       /// </summary>
       /// <seealso cref="DatumBase{T}" />
       public class GeometryDatum : DatumBase<GGeometry>
    {
        public GGeometry Abtesting { get; init; } = null!;
        public GGeometry Arena { get; init; } = null!;
        public GGeometry Bank { get; init; } = null!;
        [JsonProperty("bank_b")]
        public GGeometry BankBasement { get; init; } = null!;
        [JsonProperty("bank_u")]
        public GGeometry BankUnderground { get; init; } = null!;
        public GGeometry Cave { get; init; } = null!;
        public GGeometry Cgallery { get; init; } = null!;
        public GGeometry Crypt { get; init; } = null!;
        public GGeometry Cyberland { get; init; } = null!;
        [JsonProperty("d_a1")]
        public GGeometry DA1 { get; init; } = null!;
        [JsonProperty("d_a2")]
        public GGeometry DA2 { get; init; } = null!;
        [JsonProperty("d_b1")]
        public GGeometry DB1 { get; init; } = null!;
        [JsonProperty("d_e")]
        public GGeometry DE { get; init; } = null!;
        public GGeometry Desertland { get; init; } = null!;
        [JsonProperty("d_g")]
        public GGeometry DG { get; init; } = null!;
        public GGeometry Duelland { get; init; } = null!;
        public GGeometry Dungeon0 { get; init; } = null!;
        public GGeometry Goobrawl { get; init; } = null!;
        public GGeometry Halloween { get; init; } = null!;
        public GGeometry Hut { get; init; } = null!;
        public GGeometry Jail { get; init; } = null!;
        public GGeometry Level1 { get; init; } = null!;
        public GGeometry Level2 { get; init; } = null!;
        public GGeometry Level2E { get; init; } = null!;
        public GGeometry Level2N { get; init; } = null!;
        public GGeometry Level2S { get; init; } = null!;
        public GGeometry Level2W { get; init; } = null!;
        public GGeometry Level3 { get; init; } = null!;
        public GGeometry Level4 { get; init; } = null!;
        public GGeometry Main { get; init; } = null!;
        public GGeometry Mansion { get; init; } = null!;
        public GGeometry Mtunnel { get; init; } = null!;
        public GGeometry Resort { get; init; } = null!;
        [JsonProperty("resort_e")]
        public GGeometry ResortE { get; init; } = null!;
        public GGeometry Shellsisland { get; init; } = null!;
        public GGeometry Ship0 { get; init; } = null!;
        public GGeometry Spookytown { get; init; } = null!;
        public GGeometry Tavern { get; init; } = null!;
        public GGeometry Test { get; init; } = null!;
        public GGeometry Tomb { get; init; } = null!;
        public GGeometry Tunnel { get; init; } = null!;
        [JsonProperty("winter_cave")]
        public GGeometry WinterCave { get; init; } = null!;
        [JsonProperty("winter_inn")]
        public GGeometry WinterInn { get; init; } = null!;
        [JsonProperty("winter_inn_rooms")]
        public GGeometry WinterInnRooms { get; init; } = null!;
        [JsonProperty("winter_instance")]
        public GGeometry WinterInstance { get; init; } = null!;
        public GGeometry Winterland { get; init; } = null!;
        public GGeometry Woffice { get; init; } = null!;


        internal override void BuildLookupTable()
        {
            base.BuildLookupTable();

            //map accessors are populated based on the string from the server, not the local copy.
            foreach ((var accessor, var map) in this.Reverse().DistinctBy(kvp => kvp.Value))
                map.Accessor = accessor;
        }
    }
}