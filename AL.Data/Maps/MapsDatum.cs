using Newtonsoft.Json;

namespace AL.Data.Maps
{
    public class MapsDatum : DatumBase<Map>
    {
        public Map ABTesting { get; set; }
        public Map Arena { get; set; }
        public Map Bank { get; set; }

        [JsonProperty("bank_b")]
        public Map BankB { get; set; }

        [JsonProperty("bank_u")]
        public Map BankU { get; set; }

        public Map BatCave { get; set; }
        public Map Cave { get; set; }
        public Map CGallery { get; set; }
        public Map Crypt { get; set; }
        public Map CyberLand { get; set; }
        public Map D2 { get; set; }

        [JsonProperty("d_a1")]
        public Map DA1 { get; set; }

        [JsonProperty("d_a2")]
        public Map DA2 { get; set; }

        [JsonProperty("d_b1")]
        public Map DB1 { get; set; }

        [JsonProperty("d_e")]
        public Map DE { get; set; }

        public Map Desertland { get; set; }

        [JsonProperty("d_g")]
        public Map DG { get; set; }

        public Map DuelLand { get; set; }
        public Map Dungeon0 { get; set; }
        public Map Goobrawl { get; set; }
        public Map Halloween { get; set; }
        public Map Hut { get; set; }
        public Map Jail { get; set; }
        public Map Level1 { get; set; }
        public Map Level2 { get; set; }
        public Map Level2E { get; set; }
        public Map Level2N { get; set; }
        public Map Level2S { get; set; }
        public Map Level2W { get; set; }
        public Map Level3 { get; set; }
        public Map Level4 { get; set; }
        public Map Main { get; set; }
        public Map Mansion { get; set; }
        public Map Mtunnel { get; set; }

        [JsonProperty("old_bank")]
        public Map OldBank { get; set; }

        [JsonProperty("old_main")]
        public Map OldMain { get; set; }

        [JsonProperty("original_main")]
        public Map OriginalMain { get; set; }

        public Map Resort { get; set; }

        [JsonProperty("resort_e")]
        public Map ResortE { get; set; }

        public Map ShellsIsland { get; set; }
        public Map Ship0 { get; set; }
        public Map SpookyTown { get; set; }
        public Map Tavern { get; set; }
        public Map Test { get; set; }
        public Map Tomb { get; set; }
        public Map Tunnel { get; set; }

        [JsonProperty("winter_cave")]
        public Map WinterCave { get; set; }

        [JsonProperty("winter_inn")]
        public Map WinterInn { get; set; }

        [JsonProperty("winter_inn_rooms")]
        public Map WinterInnRooms { get; set; }

        [JsonProperty("winter_instance")]
        public Map WinterInstance { get; set; }

        public Map WinterLand { get; set; }
        public Map Woffice { get; set; }
    }
}