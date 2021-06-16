using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Maps
{
    public record Trap
    {
        [JsonConverter(typeof(ArrayToPointConverter))]
        public IPoint Position { get; init; }

        public TrapType Type { get; init; }

        [JsonProperty("polygon", ItemConverterType = typeof(ArrayToPointConverter))]
        public IPoint[] Vertices { get; init; }
    }
}