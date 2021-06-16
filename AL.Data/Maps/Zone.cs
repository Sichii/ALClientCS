using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Maps
{
    public record Zone
    {
        public string Drop { get; init; }
        public ZoneType Type { get; init; }

        [JsonProperty("polygon", ItemConverterType = typeof(ArrayToPointConverter))]
        public IPoint[] Vertices { get; init; }
    }
}