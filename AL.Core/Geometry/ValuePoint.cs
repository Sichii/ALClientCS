#region
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Interfaces;
#endregion

namespace AL.Core.Geometry;

/// <summary>
///     <inheritdoc cref="IPoint" />
///     <br />
///     A stack-only point. It cannot be boxed or captured, so any method that accepts one is allocation-free by
///     construction. Converts implicitly to and from <see cref="Point" />.
/// </summary>
public readonly ref struct ValuePoint : IPoint, IEquatable<ValuePoint>
{
    public float X { get; }
    public float Y { get; }

    public ValuePoint(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static implicit operator ValuePoint(Point point) => new(point.X, point.Y);

    public static implicit operator Point(ValuePoint point) => new(point.X, point.Y);

    public static bool operator ==(ValuePoint left, ValuePoint right) => left.Equals(right);

    public static bool operator !=(ValuePoint left, ValuePoint right) => !left.Equals(right);

    /// <summary>
    ///     Copies any <see cref="IPoint" /> onto the stack.
    /// </summary>
    public static ValuePoint From(IPoint point) => new(point.X, point.Y);

    public void Deconstruct(out float x, out float y)
    {
        x = X;
        y = Y;
    }

    //the same tolerance PointEqualityComparer applies, inlined because a ref struct cannot be handed to it
    public bool Equals(IPoint? other) => other is not null && X.IsNear(other.X, CONSTANTS.EPSILON) && Y.IsNear(other.Y, CONSTANTS.EPSILON);

    public bool Equals(ValuePoint other) => X.IsNear(other.X, CONSTANTS.EPSILON) && Y.IsNear(other.Y, CONSTANTS.EPSILON);

    public override bool Equals(object? obj) => obj is IPoint other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Convert.ToInt32(X), Convert.ToInt32(Y));

    public override string ToString() => $"({Convert.ToInt32(X):N0}, {Convert.ToInt32(Y):N0})";
}
