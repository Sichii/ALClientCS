using Newtonsoft.Json;

namespace AL.Data.Maps
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class MapsDatum : DatumBase<Map>
    {
        public Map ABTesting { get; set; } = null!;
        public Map Arena { get; set; } = null!;
        public Map Bank { get; set; } = null!;

        [JsonProperty("bank_b")]
        public Map BankB { get; set; } = null!;

        [JsonProperty("bank_u")]
        public Map BankU { get; set; } = null!;

        public Map BatCave { get; set; } = null!;
        public Map Cave { get; set; } = null!;
        public Map CGallery { get; set; } = null!;
        public Map Crypt { get; set; } = null!;
        public Map CyberLand { get; set; } = null!;
        public Map D2 { get; set; } = null!;

        [JsonProperty("d_a1")]
        public Map DA1 { get; set; } = null!;

        [JsonProperty("d_a2")]
        public Map DA2 { get; set; } = null!;

        [JsonProperty("d_b1")]
        public Map DB1 { get; set; } = null!;

        [JsonProperty("d_e")]
        public Map DE { get; set; } = null!;

        public Map Desertland { get; set; } = null!;

        [JsonProperty("d_g")]
        public Map DG { get; set; } = null!;

        public Map DuelLand { get; set; } = null!;
        public Map Dungeon0 { get; set; } = null!;
        public Map Goobrawl { get; set; } = null!;
        public Map Halloween { get; set; } = null!;
        public Map Hut { get; set; } = null!;
        public Map Jail { get; set; } = null!;
        public Map Level1 { get; set; } = null!;
        public Map Level2 { get; set; } = null!;
        public Map Level2E { get; set; } = null!;
        public Map Level2N { get; set; } = null!;
        public Map Level2S { get; set; } = null!;
        public Map Level2W { get; set; } = null!;
        public Map Level3 { get; set; } = null!;
        public Map Level4 { get; set; } = null!;
        public Map Main { get; set; } = null!;
        public Map Mansion { get; set; } = null!;
        public Map Mtunnel { get; set; } = null!;

        [JsonProperty("old_bank")]
        public Map OldBank { get; set; } = null!;

        [JsonProperty("old_main")]
        public Map OldMain { get; set; } = null!;

        [JsonProperty("original_main")]
        public Map OriginalMain { get; set; } = null!;

        public Map Resort { get; set; } = null!;

        [JsonProperty("resort_e")]
        public Map ResortE { get; set; } = null!;

        public Map ShellsIsland { get; set; } = null!;
        public Map Ship0 { get; set; } = null!;
        public Map SpookyTown { get; set; } = null!;
        public Map Tavern { get; set; } = null!;
        public Map Test { get; set; } = null!;
        public Map Tomb { get; set; } = null!;
        public Map Tunnel { get; set; } = null!;

        [JsonProperty("winter_cave")]
        public Map WinterCave { get; set; } = null!;

        [JsonProperty("winter_inn")]
        public Map WinterInn { get; set; } = null!;

        [JsonProperty("winter_inn_rooms")]
        public Map WinterInnRooms { get; set; } = null!;

        [JsonProperty("winter_instance")]
        public Map WinterInstance { get; set; } = null!;

        public Map WinterLand { get; set; } = null!;
        public Map Woffice { get; set; } = null!;

        internal override void ConstructCache()
        {
            base.ConstructCache();

            foreach ((var accessor, var map) in this)
                map.Accessor = accessor;
        }
    }
}