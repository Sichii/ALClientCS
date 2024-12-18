#region
using AL.Core.Interfaces;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents a pathfinding destination.
/// </summary>
public sealed record Destination : ILocation, ICircle
{
    public string Map { get; init; } = null!;

    public float Radius { get; init; }

    public float X { get; init; }
    public float Y { get; init; }

    public Destination(ILocation location, float radius)
    {
        Map = location.Map;
        X = location.X;
        Y = location.Y;
        Radius = radius;
    }

    public bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);
    public bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);
    public bool Equals(ICircle? other) => ICircle.Comparer.Equals(this, other);
}