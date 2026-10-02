#region
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.Core.Geometry;
using AL.Core.Interfaces;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Converts a 2-element numeric array <c>[x, y]</c> to a <see cref="Point" />.
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
///     Converts an array of <c>[x, y]</c> pairs to a <see cref="Polygon" />, reading each pair through
///     <see cref="ArrayToPointConverter" />.
/// </summary>
/// <remarks>
///     A per-element converter cannot reach the vertices: <see cref="Polygon" /> has no add path, and its element type is
///     <see cref="IPoint" /> rather than <see cref="Point" />.
/// </remarks>
public sealed class PolygonConverter : JsonConverter<Polygon>
{
    /// <summary>
    ///     Whether a JSON null reaches <see cref="Read" />. It does, so that a null, like any non-array token, yields null.
    /// </summary>
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