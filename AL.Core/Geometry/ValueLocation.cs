#region
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Interfaces;
using Chaos.Extensions.Common;
#endregion

namespace AL.Core.Geometry;

/// <summary>
///     <inheritdoc cref="ILocation" />
///     <br />
///     A stack-only location. Converts implicitly from <see cref="Location" />.
/// </summary>
public readonly ref struct ValueLocation : ILocation, IEquatable<ValueLocation>
{
    public string Map { get; }
    public float X { get; }
    public float Y { get; }

    public ValueLocation(string map, float x, float y)
    {
        Map = map;
        X = x;
        Y = y;
    }

    public ValueLocation(string map, Point point)
        : this(map, point.X, point.Y) { }

    public static implicit operator ValueLocation(Location location) => new(location.Map, location.X, location.Y);

    public static bool operator ==(ValueLocation left, ValueLocation right) => left.Equals(right);

    public static bool operator !=(ValueLocation left, ValueLocation right) => !left.Equals(right);

    /// <summary>
    ///     Copies any <see cref="ILocation" /> onto the stack.
    /// </summary>
    public static ValueLocation From(ILocation location) => new(location.Map, location.X, location.Y);

    public void Deconstruct(out string map, out float x, out float y)
    {
        map = Map;
        x = X;
        y = Y;
    }

    public bool Equals(IPoint? other) => other is not null && X.IsNear(other.X, CONSTANTS.EPSILON) && Y.IsNear(other.Y, CONSTANTS.EPSILON);

    public bool Equals(ILocation? other) => other is not null && Map is not null && other.Map is not null && Map.EqualsI(other.Map) && Equals((IPoint)other);

    public bool Equals(ValueLocation other)
        => Map is not null
            && other.Map is not null
            && Map.EqualsI(other.Map)
            && X.IsNear(other.X, CONSTANTS.EPSILON)
            && Y.IsNear(other.Y, CONSTANTS.EPSILON);

    public override bool Equals(object? obj) => obj is ILocation other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Map, HashCode.Combine(Convert.ToInt32(X), Convert.ToInt32(Y)));

    public override string ToString() => $"{Map}:({Convert.ToInt32(X):N0}, {Convert.ToInt32(Y):N0})";
}
