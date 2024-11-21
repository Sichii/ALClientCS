#region
using System;
using AL.Core.Geometry;
using Newtonsoft.Json;
#endregion

namespace AL.Core.Json.Converters;

/// <summary>
///     Provides conversion logic for <see cref="Point" />s represented as an array of 2 numbers.
/// </summary>
/// <seealso cref="JsonConverter" />
public sealed class ArrayToPointConverter : JsonConverter<Point>
{
    public override Point ReadJson(
        JsonReader reader,
        Type objectType,
        Point existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var arr = serializer.Deserialize<float[]>(reader);

        if (arr == null)
            throw new InvalidOperationException("Failed to deserialize point values.");

        return new Point(arr[0], arr[1]);
    }

    public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer) => throw new NotImplementedException();
}