#region
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.Core.Geometry;
using AL.Core.Interfaces;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Converts a 2-element numeric array
///     <c>
///         [x, y]
///     </c>
///     to a <see cref="Point" />. The System.Text.Json replacement for the Newtonsoft
///     <c>
///         ArrayToPointConverter
///     </c>
///     .
/// </summary>
public sealed class ArrayToPointConverter : JsonConverter<Point>
{
    public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var arr = JsonSerializer.Deserialize<float[]>(ref reader, options);

        if (arr is null)
            throw new InvalidOperationException("Failed to deserialize point values.");

        return new Point(arr[0], arr[1]);
    }

    public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options) => throw new NotSupportedException();
}

/// <summary>
///     Converts an array of
///     <c>
///         [x, y]
///     </c>
///     pairs to a <see cref="Polygon" />. Newtonsoft applied <see cref="ArrayToPointConverter" /> per element via
///     <c>
///         ItemConverterType
///     </c>
///     ; System.Text.Json cannot, because a <see cref="Polygon" /> is an
///     <c>
///         IEnumerable&lt;IPoint&gt;
///     </c>
///     with no add-path and its element type is the interface
///     <c>
///         IPoint
///     </c>
///     (which the point converter, a
///     <c>
///         JsonConverter&lt;Point&gt;
///     </c>
///     , does not match). So the whole polygon is claimed here: each element resolves through the registered
///     <see cref="ArrayToPointConverter" />.
/// </summary>
public sealed class PolygonConverter : JsonConverter<Polygon>
{
    //a JSON null (or any non-array token) yields null, matching Newtonsoft's item-converter short-circuit
    public override bool HandleNull => true;

    public override Polygon? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            reader.Skip();

            return null;
        }

        var vertices = JsonSerializer.Deserialize<List<Point>>(ref reader, options) ?? new List<Point>();

        //Point is a value type, so List<Point> is not covariant to IEnumerable<IPoint>; box each vertex explicitly
        return new Polygon(vertices.Cast<IPoint>());
    }

    public override void Write(Utf8JsonWriter writer, Polygon value, JsonSerializerOptions options) => throw new NotSupportedException();
}