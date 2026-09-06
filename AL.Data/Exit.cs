#region
using AL.Core.Definitions;
using AL.Core.Extensions;
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
    /// <summary>
    ///     Whether this exit is still shut. A field, not a property, and nothing in this library ever sets it - a door's own
    ///     lock lives on <see cref="AL.Data.Maps.GDoor.LockType" />.
    /// </summary>
    public bool Locked;

    /// <summary>
    ///     The accessor of the map this exit is on - the near side, not where it leads.
    /// </summary>
    public string Map { get; init; } = null!;

    /// <summary>
    ///     A single circle about this exit's own position that is wholly inside its reach, for anything that treats an exit as
    ///     a plain <see cref="ICircle" />. Conservative for a door, exact for a transporter.
    /// </summary>
    public float Radius { get; init; }

    /// <summary>
    ///     The rectangle the server measures the character against. For a door, the door's box on its own spawn grown by the
    ///     character's box; for a transporter, a point. See
    ///     <c>
    ///         GameData.DoorReachBand
    ///     </c>
    ///     . A record holding a collection compares it by reference, so do not lean on an <see cref="Exit" />'s synthesized
    ///     equality; the point, location and circle overloads below are the ones to use.
    /// </summary>
    public Rectangle ReachBand { get; init; }

    /// <summary>
    ///     How far outside <see cref="ReachBand" /> the exit still works. The band inflated by this range is the exact region
    ///     the server accepts, and it is what a walk to this exit stops inside.
    /// </summary>
    public float ReachRange { get; init; }

    /// <summary>
    ///     The location this exit leads to.
    /// </summary>
    public ILocation ToLocation { get; init; } = null!;

    /// <summary>
    ///     Which of the destination map's spawns you arrive at, as an index into its spawn list.
    /// </summary>
    public int ToSpawnIndex { get; init; }

    /// <summary>
    ///     The type of exit. (door, npc)
    /// </summary>
    public ExitType Type { get; init; }

    /// <summary>
    ///     The x coordinate of the exit itself - the door's centre, or where the transporter stands.
    /// </summary>
    public float X { get; init; }

    /// <summary>
    ///     The y coordinate of the exit itself - the door's centre, or where the transporter stands.
    /// </summary>
    public float Y { get; init; }

    /// <summary>
    ///     The exit's own position, so it can stand in as the centre of its <see cref="Radius" /> circle.
    /// </summary>
    public ILocation Center => this;

    internal Exit(
        string map,
        IPoint point,
        ILocation toLocation,
        int toSpawnIndex,
        ExitType type,
        Rectangle reachBand,
        float reachRange)
    {
        Map = map;
        X = point.X;
        Y = point.Y;
        ToLocation = toLocation;
        ToSpawnIndex = toSpawnIndex;
        Type = type;
        ReachBand = reachBand;
        ReachRange = reachRange;

        //the range less the exit's own distance to the band is the largest circle about the exit still inside
        //the region; zero where the exit is not inside it at all
        Radius = MathF.Max(0f, reachRange - reachBand.EdgeToCenterDistance(point));
    }

    public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

    public virtual bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);

    public virtual bool Equals(ICircle? other) => ICircle.Comparer.Equals(this, other);

    public override string ToString() => $"{ILocation.ToString(this)} => {ILocation.ToString(ToLocation)}";
}