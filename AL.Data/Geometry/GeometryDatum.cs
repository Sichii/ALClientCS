using Newtonsoft.Json;

namespace AL.Data.Geometry
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class GeometryDatum : DatumBase<MapGeometry>
    {
        public MapGeometry ABTesting { get; set; } = null!;
        public MapGeometry Arena { get; set; } = null!;
        public MapGeometry Bank { get; set; } = null!;

        [JsonProperty("bank_b")]
        public MapGeometry BankB { get; set; } = null!;

        [JsonProperty("bank_u")]
        public MapGeometry BankU { get; set; } = null!;

        public MapGeometry Cave { get; set; } = null!;
        public MapGeometry Cgallery { get; set; } = null!;
        public MapGeometry Crypt { get; set; } = null!;
        public MapGeometry Cyberland { get; set; } = null!;

        [JsonProperty("d_a1")]
        public MapGeometry DA1 { get; set; } = null!;

        [JsonProperty("d_a2")]
        public MapGeometry DA2 { get; set; } = null!;

        [JsonProperty("d_b1")]
        public MapGeometry DB1 { get; set; } = null!;

        [JsonProperty("d_e")]
        public MapGeometry DE { get; set; } = null!;

        public MapGeometry Desertland { get; set; } = null!;

        [JsonProperty("d_g")]
        public MapGeometry DG { get; set; } = null!;

        public MapGeometry Duelland { get; set; } = null!;
        public MapGeometry Dungeon0 { get; set; } = null!;
        public MapGeometry Goobrawl { get; set; } = null!;
        public MapGeometry Halloween { get; set; } = null!;
        public MapGeometry Hut { get; set; } = null!;
        public MapGeometry Jail { get; set; } = null!;
        public MapGeometry Level1 { get; set; } = null!;
        public MapGeometry Level2 { get; set; } = null!;
        public MapGeometry Level2E { get; set; } = null!;
        public MapGeometry Level2N { get; set; } = null!;
        public MapGeometry Level2S { get; set; } = null!;
        public MapGeometry Level2W { get; set; } = null!;
        public MapGeometry Level3 { get; set; } = null!;
        public MapGeometry Level4 { get; set; } = null!;
        public MapGeometry Main { get; set; } = null!;
        public MapGeometry Mansion { get; set; } = null!;
        public MapGeometry Mtunnel { get; set; } = null!;
        public MapGeometry Resort { get; set; } = null!;

        [JsonProperty("resort_e")]
        public MapGeometry ResortE { get; set; } = null!;

        public MapGeometry Shellsisland { get; set; } = null!;
        public MapGeometry Ship0 { get; set; } = null!;
        public MapGeometry Spookytown { get; set; } = null!;
        public MapGeometry Tavern { get; set; } = null!;
        public MapGeometry Test { get; set; } = null!;
        public MapGeometry Tomb { get; set; } = null!;
        public MapGeometry Tunnel { get; set; } = null!;

        [JsonProperty("winter_cave")]
        public MapGeometry WinterCave { get; set; } = null!;

        [JsonProperty("winter_inn")]
        public MapGeometry WinterInn { get; set; } = null!;

        [JsonProperty("winter_inn_rooms")]
        public MapGeometry WinterInnRooms { get; set; } = null!;

        [JsonProperty("winter_instance")]
        public MapGeometry WinterInstance { get; set; } = null!;

        public MapGeometry Winterland { get; set; } = null!;
        public MapGeometry Woffice { get; set; } = null!;
    }
}