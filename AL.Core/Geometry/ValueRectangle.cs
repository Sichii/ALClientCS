#region
using System.Collections;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Interfaces;
#endregion

namespace AL.Core.Geometry;

/// <summary>
///     <inheritdoc cref="IRectangle" />
///     <br />
///     A stack-only rectangle defined by its centre and size. Converts implicitly from <see cref="Rectangle" />.
///     <see cref="Vertices" /> is computed on demand and is the one member that allocates.
/// </summary>
public readonly ref struct ValueRectangle : IRectangle
{
    public float X { get; }
    public float Y { get; }
    public float Width { get; }
    public float Height { get; }
    public float Left => X - Width / 2;
    public float Right => X + Width / 2;
    public float Top => Y - Height / 2;
    public float Bottom => Y + Height / 2;

    public IReadOnlyList<IPoint> Vertices
        =>
        [
            new Point(Left, Top),
            new Point(Right, Top),
            new Point(Right, Bottom),
            new Point(Left, Bottom)
        ];

    /// <summary>
    ///     Initializes a rectangle from its centre and size, the same shape <see cref="Rectangle" />'s primary constructor
    ///     takes.
    /// </summary>
    public ValueRectangle(
        float x,
        float y,
        float width,
        float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public static implicit operator ValueRectangle(Rectangle rectangle)
        => new(
            rectangle.X,
            rectangle.Y,
            rectangle.Width,
            rectangle.Height);

    /// <summary>
    ///     Initializes a rectangle from two opposing corners.
    /// </summary>
    public static ValueRectangle FromCorners(
        float x1,
        float y1,
        float x2,
        float y2)
        => new(
            (x1 + x2) / 2,
            (y1 + y2) / 2,
            Math.Abs(x1 - x2),
            Math.Abs(y1 - y2));

    /// <summary>
    ///     Copies any <see cref="IRectangle" /> onto the stack.
    /// </summary>
    public static ValueRectangle From(IRectangle rectangle)
        => new(
            rectangle.X,
            rectangle.Y,
            rectangle.Width,
            rectangle.Height);

    public bool Equals(IPoint? other) => other is not null && X.IsNear(other.X, CONSTANTS.EPSILON) && Y.IsNear(other.Y, CONSTANTS.EPSILON);

    public override bool Equals(object? obj) => obj is IPoint other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Convert.ToInt32(X), Convert.ToInt32(Y));

    public IEnumerator<IPoint> GetEnumerator() => Vertices.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"({Convert.ToInt32(X):N0}, {Convert.ToInt32(Y):N0}) {Width:N0}x{Height:N0}";
}