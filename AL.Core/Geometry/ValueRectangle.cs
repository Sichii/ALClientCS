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
/// </summary>
/// <remarks>
///     <see cref="Vertices" /> is computed on demand and allocates.
/// </remarks>
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
    ///     Initializes a new instance of the <see cref="ValueRectangle" /> struct from its centre and size.
    /// </summary>
    /// <param name="x">The centre's x coordinate.</param>
    /// <param name="y">The centre's y coordinate.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
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

    /// <summary>Creates a rectangle from two opposing corners.</summary>
    /// <param name="x1">The first corner's x coordinate.</param>
    /// <param name="y1">The first corner's y coordinate.</param>
    /// <param name="x2">The opposing corner's x coordinate.</param>
    /// <param name="y2">The opposing corner's y coordinate.</param>
    /// <returns>The rectangle spanning both corners.</returns>
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
    /// <param name="rectangle">The rectangle to copy.</param>
    /// <returns>
    ///     A <see cref="ValueRectangle" /> with the same values.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">rectangle</exception>
    public static ValueRectangle From(IRectangle rectangle)
    {
        ArgumentNullException.ThrowIfNull(rectangle);

        return new ValueRectangle(
            rectangle.X,
            rectangle.Y,
            rectangle.Width,
            rectangle.Height);
    }

    public bool Equals(IPoint? other) => other is not null && X.IsNear(other.X, CONSTANTS.EPSILON) && Y.IsNear(other.Y, CONSTANTS.EPSILON);

    public override bool Equals(object? obj) => obj is IPoint other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Convert.ToInt32(X), Convert.ToInt32(Y));

    public IEnumerator<IPoint> GetEnumerator() => Vertices.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"({Convert.ToInt32(X):N0}, {Convert.ToInt32(Y):N0}) {Width:N0}x{Height:N0}";
}