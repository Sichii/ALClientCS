using Newtonsoft.Json;

namespace AL.Data.Geometry
{
    public class GeometryDatum : DatumBase<MapGeometry>
    {
        public MapGeometry ABTesting { get; set; }
        public MapGeometry Arena { get; set; }
        public MapGeometry Bank { get; set; }

        [JsonProperty("bank_b")]
        public MapGeometry BankB { get; set; }

        [JsonProperty("bank_u")]
        public MapGeometry BankU { get; set; }

        public MapGeometry Cave { get; set; }
        public MapGeometry Cgallery { get; set; }
        public MapGeometry Crypt { get; set; }
        public MapGeometry Cyberland { get; set; }

        [JsonProperty("d_a1")]
        public MapGeometry DA1 { get; set; }

        [JsonProperty("d_a2")]
        public MapGeometry DA2 { get; set; }

        [JsonProperty("d_b1")]
        public MapGeometry DB1 { get; set; }

        [JsonProperty("d_e")]
        public MapGeometry DE { get; set; }

        public MapGeometry Desertland { get; set; }

        [JsonProperty("d_g")]
        public MapGeometry DG { get; set; }

        public MapGeometry Duelland { get; set; }
        public MapGeometry Dungeon0 { get; set; }
        public MapGeometry Goobrawl { get; set; }
        public MapGeometry Halloween { get; set; }
        public MapGeometry Hut { get; set; }
        public MapGeometry Jail { get; set; }
        public MapGeometry Level1 { get; set; }
        public MapGeometry Level2 { get; set; }
        public MapGeometry Level2E { get; set; }
        public MapGeometry Level2N { get; set; }
        public MapGeometry Level2S { get; set; }
        public MapGeometry Level2W { get; set; }
        public MapGeometry Level3 { get; set; }
        public MapGeometry Level4 { get; set; }
        public MapGeometry Main { get; set; }
        public MapGeometry Mansion { get; set; }
        public MapGeometry Mtunnel { get; set; }
        public MapGeometry Resort { get; set; }

        [JsonProperty("resort_e")]
        public MapGeometry ResortE { get; set; }

        public MapGeometry Shellsisland { get; set; }
        public MapGeometry Ship0 { get; set; }
        public MapGeometry Spookytown { get; set; }
        public MapGeometry Tavern { get; set; }
        public MapGeometry Test { get; set; }
        public MapGeometry Tomb { get; set; }
        public MapGeometry Tunnel { get; set; }

        [JsonProperty("winter_cave")]
        public MapGeometry WinterCave { get; set; }

        [JsonProperty("winter_inn")]
        public MapGeometry WinterInn { get; set; }

        [JsonProperty("winter_inn_rooms")]
        public MapGeometry WinterInnRooms { get; set; }

        [JsonProperty("winter_instance")]
        public MapGeometry WinterInstance { get; set; }

        public MapGeometry Winterland { get; set; }
        public MapGeometry Woffice { get; set; }
    }
}