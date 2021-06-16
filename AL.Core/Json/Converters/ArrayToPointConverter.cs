using System;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.Core.Json.Converters
{
    public class ArrayToPointConverter : JsonConverter<IPoint>
    {
        public override IPoint ReadJson(JsonReader reader, Type objectType, IPoint existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var tupleConverter = new ArrayToTupleConverter<float, float>();

            (var x, var y) = tupleConverter.ReadJson(reader, objectType, default, false, serializer);

            return new Point(x, y);
        }

        public override void WriteJson(JsonWriter writer, IPoint value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}