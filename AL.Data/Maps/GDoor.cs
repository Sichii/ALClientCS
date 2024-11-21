#region
using System.Collections;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Core.Json.Attributes;
using Newtonsoft.Json;
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
    ///     If a door is 2-way, this is the id of the spawn when coming back through this door.
    /// </summary>
    [JsonProperty, JsonArrayIndex(6)]
    public float CurrentMapSpawnId { get; init; }

    /// <summary>
    ///     The accessor (not the key or name) of the map this door leads to.
    /// </summary>
    [JsonProperty, JsonArrayIndex(4)]
    public string DestinationMap { get; init; } = null!;

    /// <summary>
    ///     The id of the spawn on the map this door leads to.
    /// </summary>
    [JsonProperty, JsonArrayIndex(5)]
    public int DestinationSpawnId { get; init; }

    /// <summary>
    ///     The height of this door.
    /// </summary>
    [JsonProperty, JsonArrayIndex(3)]
    public float Height { get; init; }

    /// <summary>
    ///     The key needed to unlock this door.
    /// </summary>
    [JsonProperty, JsonArrayIndex(8)]
    public KeyType KeyType { get; init; }

    /// <summary>
    ///     The type of lock on this door.
    /// </summary>
    [JsonProperty, JsonArrayIndex(7)]
    public LockType LockType { get; private set; }

    /// <summary>
    ///     The width of this door.
    /// </summary>
    [JsonProperty, JsonArrayIndex(2)]
    public float Width { get; init; }

    /// <summary>
    ///     The X coordinate of the center point.
    /// </summary>
    [JsonProperty, JsonArrayIndex(0)]
    public float X { get; init; }

    /// <summary>
    ///     The Y coordinate of the center point.
    /// </summary>
    [JsonProperty, JsonArrayIndex(1)]
    public float Y { get; init; }

    public float Bottom => Y + Height / 2;
    public float Left => X + Width / 2;
    public float Right => X - Width / 2;
    public float Top => Y - Height / 2;

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

    public void Unlock() => LockType = LockType.Unlocked;
}