using System;
using AL.Core.Geometry;
using Newtonsoft.Json;

namespace AL.Core.Json.Converters
{
    /// <summary>
    ///     Provides conversion logic for <see cref="MapRectangle" /> objects.
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.JsonConverter{T}" />
    public class MapRectangleConverter : JsonConverter<MapRectangle>
    {
        public override MapRectangle ReadJson(
            JsonReader reader,
            Type objectType,
            MapRectangle? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var tupleConverter = new ArrayToTupleConverter<object, int, int, int, int>();

            //first value could be a mapName or the first x vertice (tuple can be 4 or 5 in length)
            (var obj, var num1, var num2, var num3, var num4) = tupleConverter.ReadJson(reader, objectType, default, false, serializer);

            var str = obj?.ToString();

            return float.TryParse(str, out var val)
                ? new MapRectangle(new Point(val, num1), new Point(num2, num3))
                : new MapRectangle(new Point(num1, num2), new Point(num3, num4), str);
        }

        public override void WriteJson(JsonWriter writer, MapRectangle? value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}