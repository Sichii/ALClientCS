#region
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
#endregion

namespace AL.Data;

/// <summary>
///     Represents a point on a map that will teleport you to another map or point.
/// </summary>
/// <seealso cref="ICircle" />
/// <seealso cref="ILocation" />
public record Exit : ICircle, ILocation
{
    public bool Locked;
    public string Map { get; init; } = null!;

    /// <summary>
    ///     A single circle about this exit's own position that is wholly inside <see cref="ReachableFrom" />, for
    ///     anything that treats an exit as a plain <see cref="ICircle" />. Conservative for a door, exact for a
    ///     transporter.
    /// </summary>
    public float Radius { get; init; }

    /// <summary>
    ///     Where you have to stand for the server to let you through, as the union of these circles. One for a
    ///     transporter, which the server measures centre to centre; four for a door, which it does not. See
    ///     <c>GameData.DoorReachableRegion</c> for how a door's are derived and what they cover.
    ///     <br />
    ///     Compared by reference, as any collection on a record is - do not lean on an <see cref="Exit" />'s
    ///     synthesized equality. The hand-written overloads below compare position, which is what callers want.
    /// </summary>
    public IReadOnlyList<ICircle> ReachableFrom { get; init; }

    /// <summary>
    ///     The location this exit leads to.
    /// </summary>
    public ILocation ToLocation { get; init; } = null!;

    public int ToSpawnIndex { get; init; }

    /// <summary>
    ///     The type of exit. (door, npc)
    /// </summary>
    public ExitType Type { get; init; }

    public float X { get; init; }
    public float Y { get; init; }

    public ILocation Center => this;

    internal Exit(
        string map,
        IPoint point,
        ILocation toLocation,
        int toSpawnIndex,
        ExitType type,
        float radius,
        IReadOnlyList<ICircle>? reachableFrom = null)
    {
        Map = map;
        X = point.X;
        Y = point.Y;
        ToLocation = toLocation;
        ToSpawnIndex = toSpawnIndex;
        Type = type;
        Radius = radius;

        //a transporter is measured centre to centre from where it stands, so its own circle is the whole region
        ReachableFrom = reachableFrom ?? [new Circle(point, radius)];
    }

    public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

    public virtual bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);

    public virtual bool Equals(ICircle? other) => ICircle.Comparer.Equals(this, other);

    public override string ToString() => $"{ILocation.ToString(this)} => {ILocation.ToString(ToLocation)}";
}