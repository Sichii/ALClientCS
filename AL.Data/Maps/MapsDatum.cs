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
        public GMap ABTesting { get; set; } = null!;
        public GMap Arena { get; set; } = null!;
        public GMap Bank { get; set; } = null!;

        [JsonProperty("bank_b")]
        public GMap BankB { get; set; } = null!;

        [JsonProperty("bank_u")]
        public GMap BankU { get; set; } = null!;

        public GMap BatCave { get; set; } = null!;
        public GMap Cave { get; set; } = null!;
        public GMap CGallery { get; set; } = null!;
        public GMap Crypt { get; set; } = null!;
        public GMap CyberLand { get; set; } = null!;
        public GMap D2 { get; set; } = null!;

        [JsonProperty("d_a1")]
        public GMap DA1 { get; set; } = null!;

        [JsonProperty("d_a2")]
        public GMap DA2 { get; set; } = null!;

        [JsonProperty("d_b1")]
        public GMap DB1 { get; set; } = null!;

        [JsonProperty("d_e")]
        public GMap DE { get; set; } = null!;

        public GMap Desertland { get; set; } = null!;

        [JsonProperty("d_g")]
        public GMap DG { get; set; } = null!;

        public GMap DuelLand { get; set; } = null!;
        public GMap Dungeon0 { get; set; } = null!;
        public GMap Goobrawl { get; set; } = null!;
        public GMap Halloween { get; set; } = null!;
        public GMap Hut { get; set; } = null!;
        public GMap Jail { get; set; } = null!;
        public GMap Level1 { get; set; } = null!;
        public GMap Level2 { get; set; } = null!;
        public GMap Level2E { get; set; } = null!;
        public GMap Level2N { get; set; } = null!;
        public GMap Level2S { get; set; } = null!;
        public GMap Level2W { get; set; } = null!;
        public GMap Level3 { get; set; } = null!;
        public GMap Level4 { get; set; } = null!;
        public GMap Main { get; set; } = null!;
        public GMap Mansion { get; set; } = null!;
        public GMap Mtunnel { get; set; } = null!;

        [JsonProperty("old_bank")]
        public GMap OldBank { get; set; } = null!;

        [JsonProperty("old_main")]
        public GMap OldMain { get; set; } = null!;

        [JsonProperty("original_main")]
        public GMap OriginalMain { get; set; } = null!;

        public GMap Resort { get; set; } = null!;

        [JsonProperty("resort_e")]
        public GMap ResortE { get; set; } = null!;

        public GMap ShellsIsland { get; set; } = null!;
        public GMap Ship0 { get; set; } = null!;
        public GMap SpookyTown { get; set; } = null!;
        public GMap Tavern { get; set; } = null!;
        public GMap Test { get; set; } = null!;
        public GMap Tomb { get; set; } = null!;
        public GMap Tunnel { get; set; } = null!;

        [JsonProperty("winter_cave")]
        public GMap WinterCave { get; set; } = null!;

        [JsonProperty("winter_inn")]
        public GMap WinterInn { get; set; } = null!;

        [JsonProperty("winter_inn_rooms")]
        public GMap WinterInnRooms { get; set; } = null!;

        [JsonProperty("winter_instance")]
        public GMap WinterInstance { get; set; } = null!;

        public GMap WinterLand { get; set; } = null!;
        public GMap Woffice { get; set; } = null!;

        internal override void ConstructCache()
        {
            base.ConstructCache();

            //map accessors are populated based on the string from the server, not the local copy.
            foreach ((var accessor, var map) in this.Reverse().DistinctBy(kvp => kvp.Value.Key))
                map.Accessor = accessor;
        }
    }
}