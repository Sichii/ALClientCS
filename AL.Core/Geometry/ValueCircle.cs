#region
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Interfaces;
#endregion

namespace AL.Core.Geometry;

/// <summary>
///     <inheritdoc cref="ICircle" />
///     <br />
///     A stack-only circle. Converts implicitly from <see cref="Circle" />.
/// </summary>
public readonly ref struct ValueCircle : ICircle, IEquatable<ValueCircle>
{
    public float X { get; }
    public float Y { get; }
    public float Radius { get; }

    public ValueCircle(float x, float y, float radius)
    {
        X = x;
        Y = y;
        Radius = radius;
    }

    public ValueCircle(Point center, float radius)
        : this(center.X, center.Y, radius) { }

    public static implicit operator ValueCircle(Circle circle) => new(circle.X, circle.Y, circle.Radius);

    public static bool operator ==(ValueCircle left, ValueCircle right) => left.Equals(right);

    public static bool operator !=(ValueCircle left, ValueCircle right) => !left.Equals(right);

    /// <summary>
    ///     Copies any <see cref="ICircle" /> onto the stack.
    /// </summary>
    public static ValueCircle From(ICircle circle) => new(circle.X, circle.Y, circle.Radius);

    public bool Equals(IPoint? other) => other is not null && X.IsNear(other.X, CONSTANTS.EPSILON) && Y.IsNear(other.Y, CONSTANTS.EPSILON);

    public bool Equals(ICircle? other) => other is not null && Radius.Equals(other.Radius) && Equals((IPoint)other);

    public bool Equals(ValueCircle other)
        => Radius.Equals(other.Radius) && X.IsNear(other.X, CONSTANTS.EPSILON) && Y.IsNear(other.Y, CONSTANTS.EPSILON);

    public override bool Equals(object? obj) => obj is ICircle other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Radius.GetHashCode(), HashCode.Combine(Convert.ToInt32(X), Convert.ToInt32(Y)));

    public override string ToString() => $"({Convert.ToInt32(X):N0}, {Convert.ToInt32(Y):N0}) r{Radius:N0}";
}
