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
    ///     If a door is 2-way, this is the id of the spawn when coming back through this door. The server measures
    ///     the door's range from that spawn rather than from the door, so a door carrying no such id is one it
    ///     cannot resolve at all - it faults reading the spawn, and the door opens from nowhere.
    ///     <br />
    ///     An absent id deserializes to the same nought a real spawn 0 does, so the two are not distinguishable
    ///     here. Nothing downstream needs them to be: see <c>GameData.DoorReachableRegion</c>.
    /// </summary>
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

    /// <summary>
    ///     The height of this door.
    /// </summary>
    [JsonArrayIndex(3)]
    public float Height { get; init; }

    /// <summary>
    ///     The key item needed to unlock this door. Only a door whose <see cref="LockType" /> is a key carries
    ///     one.
    /// </summary>
    [JsonArrayIndex(8)]
    public KeyType KeyType { get; init; }

    /// <summary>
    ///     What stops you walking through: a key, a gatekeeper monster, or a bank level you have not unlocked
    ///     (node/server.js:5429, :5442).
    /// </summary>
    [JsonArrayIndex(7)]
    [JsonInclude]
    public LockType LockType { get; private set; }

    /// <summary>
    ///     The width of this door.
    /// </summary>
    [JsonArrayIndex(2)]
    public float Width { get; init; }

    /// <summary>
    ///     The X coordinate of the center point.
    /// </summary>
    [JsonArrayIndex(0)]
    public float X { get; init; }

    /// <summary>
    ///     The Y coordinate of the center point.
    /// </summary>
    [JsonArrayIndex(1)]
    public float Y { get; init; }

    /// <summary>
    ///     The y coordinate of the lower edge. Y grows downward, so this is the larger of the two.
    /// </summary>
    public float Bottom => Y + Height / 2;

    /// <summary>
    ///     The x coordinate of the left edge - except it returns X + Width / 2, which is the right one.
    ///     <see cref="Right" /> has the same fault in reverse.
    /// </summary>
    public float Left => X + Width / 2;

    /// <summary>
    ///     The x coordinate of the right edge - except it returns X - Width / 2, which is the left one. See
    ///     <see cref="Left" />.
    /// </summary>
    public float Right => X - Width / 2;

    /// <summary>
    ///     The y coordinate of the upper edge. Y grows downward, so this is the smaller of the two.
    /// </summary>
    public float Top => Y - Height / 2;

    /// <summary>
    ///     The four corners of the door rectangle. Each is built with its y value passed as x and its x value as
    ///     y, so every corner comes back transposed.
    /// </summary>
    public IReadOnlyList<IPoint> Vertices
        =>
        [
            new Point(((IRectangle)this).Top, ((IRectangle)this).Left),
            new Point(((IRectangle)this).Top, ((IRectangle)this).Right),
            new Point(((IRectangle)this).Bottom, ((IRectangle)this).Left),
            new Point(((IRectangle)this).Bottom, ((IRectangle)this).Right)
        ];

    public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);
    public IEnumerator<IPoint> GetEnumerator() => Vertices.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    ///     Records locally that this door has been opened. It sends nothing; the server decides for itself.
    /// </summary>
    public void Unlock() => LockType = LockType.Unlocked;
}