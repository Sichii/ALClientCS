#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Core.Interfaces;

/// <summary>Represents a rectangle.</summary>
/// <remarks>
///     <see cref="JsonForcedObjectAttribute" /> makes implementers that also implement <see cref="IEnumerable{IPoint}" />
///     (GTile, for one) bind as objects rather than arrays.
/// </remarks>
/// <seealso cref="IPolygon" />
/// <seealso cref="AL.Core.Interfaces.IPoint" />
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