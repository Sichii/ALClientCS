using Newtonsoft.Json;

namespace AL.Data.Geometry
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class GeometryDatum : DatumBase<GGeometry>
    {
        public GGeometry ABTesting { get; set; } = null!;
        public GGeometry Arena { get; set; } = null!;
        public GGeometry Bank { get; set; } = null!;

        [JsonProperty("bank_b")]
        public GGeometry BankB { get; set; } = null!;

        [JsonProperty("bank_u")]
        public GGeometry BankU { get; set; } = null!;

        public GGeometry Cave { get; set; } = null!;
        public GGeometry Cgallery { get; set; } = null!;
        public GGeometry Crypt { get; set; } = null!;
        public GGeometry Cyberland { get; set; } = null!;

        [JsonProperty("d_a1")]
        public GGeometry DA1 { get; set; } = null!;

        [JsonProperty("d_a2")]
        public GGeometry DA2 { get; set; } = null!;

        [JsonProperty("d_b1")]
        public GGeometry DB1 { get; set; } = null!;

        [JsonProperty("d_e")]
        public GGeometry DE { get; set; } = null!;

        public GGeometry Desertland { get; set; } = null!;

        [JsonProperty("d_g")]
        public GGeometry DG { get; set; } = null!;

        public GGeometry Duelland { get; set; } = null!;
        public GGeometry Dungeon0 { get; set; } = null!;
        public GGeometry Goobrawl { get; set; } = null!;
        public GGeometry Halloween { get; set; } = null!;
        public GGeometry Hut { get; set; } = null!;
        public GGeometry Jail { get; set; } = null!;
        public GGeometry Level1 { get; set; } = null!;
        public GGeometry Level2 { get; set; } = null!;
        public GGeometry Level2E { get; set; } = null!;
        public GGeometry Level2N { get; set; } = null!;
        public GGeometry Level2S { get; set; } = null!;
        public GGeometry Level2W { get; set; } = null!;
        public GGeometry Level3 { get; set; } = null!;
        public GGeometry Level4 { get; set; } = null!;
        public GGeometry Main { get; set; } = null!;
        public GGeometry Mansion { get; set; } = null!;
        public GGeometry Mtunnel { get; set; } = null!;
        public GGeometry Resort { get; set; } = null!;

        [JsonProperty("resort_e")]
        public GGeometry ResortE { get; set; } = null!;

        public GGeometry Shellsisland { get; set; } = null!;
        public GGeometry Ship0 { get; set; } = null!;
        public GGeometry Spookytown { get; set; } = null!;
        public GGeometry Tavern { get; set; } = null!;
        public GGeometry Test { get; set; } = null!;
        public GGeometry Tomb { get; set; } = null!;
        public GGeometry Tunnel { get; set; } = null!;

        [JsonProperty("winter_cave")]
        public GGeometry WinterCave { get; set; } = null!;

        [JsonProperty("winter_inn")]
        public GGeometry WinterInn { get; set; } = null!;

        [JsonProperty("winter_inn_rooms")]
        public GGeometry WinterInnRooms { get; set; } = null!;

        [JsonProperty("winter_instance")]
        public GGeometry WinterInstance { get; set; } = null!;

        public GGeometry Winterland { get; set; } = null!;
        public GGeometry Woffice { get; set; } = null!;
    }
}