#region
using System.Collections;
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Maps;

/// <summary>
///     <inheritdoc cref="IRectangle" />
///     <br />
///     Represents a door on the map.
/// </summary>
/// <seealso cref="IRectangle" />
public record GDoor : IRectangle
{
    /// <summary>
    ///     If a door is 2-way, this is the id of the spawn when coming back through this door. The server measures the door's
    ///     range from that spawn rather than from the door.
    /// </summary>
    /// <remarks>
    ///     An absent id reads as 0, the same as a real spawn 0.
    /// </remarks>
    [JsonArrayIndex(6)]
    public float CurrentMapSpawnId { get; init; }

    /// <summary>
    ///     The accessor (not the key or name) of the map this door leads to.
    /// </summary>
    [JsonArrayIndex(4)]
    public string DestinationMap { get; init; } = null!;

    /// <summary>
    ///     The id of the spawn on the map this door leads to.
    /// </summary>
    [JsonArrayIndex(5)]
    public int DestinationSpawnId { get; init; }

    /// <summary>The height of this door.</summary>
    [JsonArrayIndex(3)]
    public float Height { get; init; }

    /// <summary>
    ///     The key item needed to unlock this door. Only a door whose <see cref="LockType" /> is a key carries one.
    /// </summary>
    [JsonArrayIndex(8)]
    public KeyType KeyType { get; init; }

    /// <summary>
    ///     What stops you walking through: a key, a gatekeeper monster, or a bank level you have not unlocked.
    /// </summary>
    [JsonArrayIndex(7)]
    [JsonInclude]
    public DoorLockType LockType { get; private set; }

    /// <summary>The width of this door.</summary>
    [JsonArrayIndex(2)]
    public float Width { get; init; }

    /// <summary>The X coordinate of the center point.</summary>
    [JsonArrayIndex(0)]
    public float X { get; init; }

    /// <summary>The Y coordinate of the center point.</summary>
    [JsonArrayIndex(1)]
    public float Y { get; init; }

    /// <summary>The y coordinate of the lower edge.</summary>
    public float Bottom => Y + Height / 2;

    /// <summary>The x coordinate of the left edge.</summary>
    public float Left => X - Width / 2;

    /// <summary>The x coordinate of the right edge.</summary>
    public float Right => X + Width / 2;

    /// <summary>The y coordinate of the upper edge.</summary>
    public float Top => Y - Height / 2;

    /// <summary>
    ///     The four corners of the door rectangle, clockwise from the top left.
    /// </summary>
    public IReadOnlyList<IPoint> Vertices
        =>
        [
            new Point(Left, Top),
            new Point(Right, Top),
            new Point(Right, Bottom),
            new Point(Left, Bottom)
        ];

    public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);
    public IEnumerator<IPoint> GetEnumerator() => Vertices.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    ///     Marks this door as unlocked locally. Nothing is sent to the server.
    /// </summary>
    public void Unlock() => LockType = DoorLockType.Unlocked;
}