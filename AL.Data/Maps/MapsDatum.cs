using System.Linq;
using Chaos.Core.Extensions;
using Newtonsoft.Json;

namespace AL.Data.Maps
{
     /// <summary>
     ///     <inheritdoc />
     /// </summary>
     /// <seealso cref="DatumBase{T}" />
     public class MapsDatum : DatumBase<GMap>
    {
        public GMap Abtesting { get; init; } = null!;
        public GMap Arena { get; init; } = null!;
        public GMap Bank { get; init; } = null!;
        [JsonProperty("bank_b")]
        public GMap BankBasement { get; init; } = null!;
        [JsonProperty("bank_u")]
        public GMap BankUnderground { get; init; } = null!;
        public GMap Batcave { get; init; } = null!;
        public GMap Cave { get; init; } = null!;
        public GMap Cgallery { get; init; } = null!;
        public GMap Crypt { get; init; } = null!;
        public GMap Cyberland { get; init; } = null!;
        public GMap D2 { get; init; } = null!;
        [JsonProperty("d_a1")]
        public GMap DA1 { get; init; } = null!;
        [JsonProperty("d_a2")]
        public GMap DA2 { get; init; } = null!;
        [JsonProperty("d_b1")]
        public GMap DB1 { get; init; } = null!;
        [JsonProperty("d_e")]
        public GMap DE { get; init; } = null!;
        public GMap Desertland { get; init; } = null!;
        [JsonProperty("d_g")]
        public GMap DG { get; init; } = null!;
        public GMap Duelland { get; init; } = null!;
        public GMap Dungeon0 { get; init; } = null!;
        public GMap Goobrawl { get; init; } = null!;
        public GMap Halloween { get; init; } = null!;
        public GMap Hut { get; init; } = null!;
        public GMap Jail { get; init; } = null!;
        public GMap Level1 { get; init; } = null!;
        public GMap Level2 { get; init; } = null!;
        public GMap Level2E { get; init; } = null!;
        public GMap Level2N { get; init; } = null!;
        public GMap Level2S { get; init; } = null!;
        public GMap Level2W { get; init; } = null!;
        public GMap Level3 { get; init; } = null!;
        public GMap Level4 { get; init; } = null!;
        public GMap Main { get; init; } = null!;
        public GMap Mansion { get; init; } = null!;
        public GMap Mtunnel { get; init; } = null!;
        [JsonProperty("old_bank")]
        public GMap OldBank { get; init; } = null!;
        [JsonProperty("old_main")]
        public GMap OldMain { get; init; } = null!;
        [JsonProperty("original_main")]
        public GMap OriginalMain { get; init; } = null!;
        public GMap Resort { get; init; } = null!;
        [JsonProperty("resort_e")]
        public GMap ResortE { get; init; } = null!;
        public GMap Shellsisland { get; init; } = null!;
        public GMap Ship0 { get; init; } = null!;
        public GMap Spookytown { get; init; } = null!;
        public GMap Tavern { get; init; } = null!;
        public GMap Test { get; init; } = null!;
        public GMap Tomb { get; init; } = null!;
        public GMap Tunnel { get; init; } = null!;
        [JsonProperty("winter_cave")]
        public GMap WinterCave { get; init; } = null!;
        [JsonProperty("winter_inn")]
        public GMap WinterInn { get; init; } = null!;
        [JsonProperty("winter_inn_rooms")]
        public GMap WinterInnRooms { get; init; } = null!;
        [JsonProperty("winter_instance")]
        public GMap WinterInstance { get; init; } = null!;
        public GMap Winterland { get; init; } = null!;
        public GMap Woffice { get; init; } = null!;


        internal override void BuildLookupTable()
        {
            base.BuildLookupTable();

            //map accessors are populated based on the string from the server, not the local copy.
            foreach ((var accessor, var map) in this.Reverse().DistinctBy(kvp => kvp.Value.Key))
                map.Accessor = accessor;
        }
    }
}