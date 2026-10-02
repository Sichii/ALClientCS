#region
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Geometry;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Parses a <see cref="MapRectangle" /> from a positional array that is either <c>[x1, y1, x2, y2]</c> or
///     <c>[mapName, x1, y1, x2, y2]</c>.
/// </summary>
public sealed class MapRectangleConverter : JsonConverter<MapRectangle>
{
    /// <summary>
    ///     Whether a JSON null reaches <see cref="Read" />. It does, so that a null yields a zeroed, unnamed rectangle rather
    ///     than a null reference.
    /// </summary>
    public override bool HandleNull => true;

    private static float ReadCoordinate(JsonArray arr, int index, JsonSerializerOptions options)
        => (index < arr.Count) && arr[index] is { } node ? node.Deserialize<float>(options) : 0f;

    public override MapRectangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var arr = JsonNode.Parse(ref reader)
                          ?.AsArray();

        if (arr is null)
            return new MapRectangle(new Point(0, 0), new Point(0, 0));

        //a string coordinate coerces through the options
        var num1 = ReadCoordinate(arr, 1, options);
        var num2 = ReadCoordinate(arr, 2, options);
        var num3 = ReadCoordinate(arr, 3, options);
        var num4 = ReadCoordinate(arr, 4, options);

        //element 0 is the first x (4 elements) or a map name (5 elements); a number or numeric string is a coordinate
        var first = arr[0];

        if (first?.GetValueKind() == JsonValueKind.Number)
            return new MapRectangle(new Point(first.GetValue<float>(), num1), new Point(num2, num3));

        var str = first?.GetValue<string>();

        return float.TryParse(
            str,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var val)
            ? new MapRectangle(new Point(val, num1), new Point(num2, num3))
            : new MapRectangle(new Point(num1, num2), new Point(num3, num4), str);
    }

    public override void Write(Utf8JsonWriter writer, MapRectangle value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}