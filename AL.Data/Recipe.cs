using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data
{
    public record Recipe
    {
        public int Cost { get; init; }

        [JsonProperty(ItemConverterType = typeof(ArrayToTupleConverter<float, string, int>))]
        public (float Quantity, string ItemName, int Level)[] Items { get; init; }

        public string Quest { get; init; }
    }
}