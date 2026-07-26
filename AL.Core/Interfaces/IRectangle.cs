#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Core.Interfaces;

/// <summary>
///     Represents a rectangle.
/// </summary>
/// <seealso cref="IPolygon" />
/// <seealso cref="AL.Core.Interfaces.IPoint" />

// forces IRectangle implementers that also implement IEnumerable<IPoint> (e.g. GTile) to bind as objects,
// not arrays. System.Text.Json reads [JsonForcedObject].
[JsonForcedObject]
public interface IRectangle : IPolygon, IPoint
{
    public float Bottom { get; }
    public float Height { get; }
    public float Left { get; }
    public float Right { get; }
    public float Top { get; }
    public float Width { get; }
}