#region
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Core.Model;
using AL.SocketClient.Interfaces;
using Chaos.Time.Abstractions;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Provides a base for <see cref="Player" />s and <see cref="Monster" />s.
/// </summary>
/// <seealso cref="AttributedRecordBase" />
/// <seealso cref="IBounding" />
/// <seealso cref="IRectangle" />
/// <seealso cref="IInstancedLocation" />
/// <seealso cref="IDeltaUpdateable" />
/// <seealso cref="IPingCompensated" />
/// <seealso cref="IMutable{TMutator}" />
/// <seealso cref="IEquatable{T}" />
public abstract class EntityBase : AttributedObjectBase,
                                   IBounding,
                                   IRectangle,
                                   IInstancedLocation,
                                   IDeltaUpdatable,
                                   IPingCompensated,
                                   IMutable<Mutation>,
                                   IEquatable<EntityBase>
{
    protected BoundingBase BoundingBase = null!;

    /// <summary>
    ///     TODO: what's this?
    /// </summary>
    [JsonProperty]
    public bool ABS { get; protected set; }

    /// <summary>
    ///     If moving, this is the angle they are moving at. (in degrees +/- 180)
    /// </summary>
    [JsonProperty]
    public float Angle { get; protected set; }

    /// <summary>
    ///     The conditions this entity has.
    ///     <br />
    ///     <b>
    ///         THIS COLLECTION IS SYNCHRONIZED, DO NOT DO LONG RUNNING OPERATIONS WHILE ITERATING IT.
    ///     </b>
    /// </summary>
    [JsonProperty("s")]
    public ConcurrentDictionary<Core.Definitions.Condition, Condition> Conditions { get; protected set; } = new();

    /// <summary>
    ///     A unique ID for this version of the entity's data.
    ///     <br />
    ///     This number starts at 0 and iterates by 1 every time a new version of this entity's data is sent.
    /// </summary>
    [JsonProperty("cid")]
    public int ContinuousId { get; init; }

    /// <summary>
    ///     If this entity is moving, this is the X coordinate they are moving to.
    /// </summary>
    [JsonProperty("going_x")]
    public float GoingX { get; protected set; }

    /// <summary>
    ///     If this entity is moving, this is the Y coordinate they are moving to.
    /// </summary>
    [JsonProperty("going_y")]
    public float GoingY { get; protected set; }

    /// <summary>
    ///     <see cref="Player" /> name, or <see cref="Monster" /> unique id.
    /// </summary>
    [JsonProperty]
    public string Id { get; init; } = null!;

    public string? In { get; protected set; }

    public bool IsCompensated { get; private set; }

    [JsonProperty]
    public int Level { get; protected set; }

    [JsonProperty("map")]
    public string Map { get; protected set; } = null!;

    [JsonProperty("max_hp")]
    public float MaxHP { get; protected set; }

    /// <summary>
    ///     The number of individual movements this entity has done.
    /// </summary>
    [JsonProperty("move_num")]
    public ulong MoveNum { get; protected set; }

    [JsonProperty]
    public bool Moving { get; protected set; }

    /// <summary>
    ///     If populated, the <see cref="Id" /> of this entity's target.
    /// </summary>
    [JsonProperty]
    public string? Target { get; protected set; }

    [JsonProperty]
    public float X { get; protected set; }

    [JsonProperty]
    public float Y { get; protected set; }

    public float Bottom => Y + VerticalNotNorth;
    public float HalfWidth => BoundingBase.HalfWidth;
    public float Height => VerticalNorth + VerticalNotNorth;
    public float Left => X - HalfWidth;
    public float Right => X + HalfWidth;
    public float Top => Y - VerticalNorth;
    public float VerticalNorth => BoundingBase.VerticalNorth;
    public float VerticalNotNorth => BoundingBase.VerticalNotNorth;

    public IReadOnlyList<IPoint> Vertices
        =>
        [
            new Point(Left, Top),
            new Point(Right, Top),
            new Point(Right, Bottom),
            new Point(Left, Bottom)
        ];

    public float Width => HalfWidth * 2;

    public void CompensateOnce(TimeSpan minimumOffset)
    {
        if (IsCompensated)
            throw new InvalidOperationException("Object already compensated.");

        IsCompensated = true;

        Update(minimumOffset);
    }

    public virtual bool Equals(EntityBase? other) => other is not null && Id.Equals(other.Id);

    public bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

    public bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);

    public IEnumerator<IPoint> GetEnumerator() => Vertices.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override int GetHashCode() => Id.GetHashCode();

    public void Mutate(Mutation mutator)
    {
        if (mutator.Attribute == ALAttribute.Hp)
            HP += Convert.ToInt32(mutator.Mutator);
    }

    public void Update(TimeSpan delta)
    {
        foreach (var kvp in Conditions.ToList())
        {
            kvp.Value.Update(delta);

            if (kvp.Value.RemainingMs <= 0)
                Conditions.Remove(kvp.Key, out _);
        }

        //if not moving, or less than 1ms has passed, or already at destination, then dont update
        if (!Moving || (X.IsNear(GoingX, CONSTANTS.EPSILON) && Y.IsNear(GoingY, CONSTANTS.EPSILON)))
            return;

        var going = new Point(GoingX, GoingY);
        var distanceDelta = Convert.ToSingle(Speed / 1000 * delta.TotalMilliseconds);
        var distance = this.Distance(going);

        if (distance > distanceDelta)
            distance = distanceDelta;
        else
        {
            Moving = false;
            X = GoingX;
            Y = GoingY;

            return;
        }

        (var newX, var newY) = this.AngularOffset(Angle, distance);
        X = newX;
        Y = newY;
    }

    public void CorrectAndCompensate(IPoint point, TimeSpan minimumOffset)
    {
        IsCompensated = false;
        UpdateLocation(point);
        CompensateOnce(minimumOffset);
    }

    public override bool Equals(object? obj) => Equals(obj as EntityBase);

    /// <summary>
    ///     Sets the bounding base of the entity.
    /// </summary>
    /// <param name="boundingBase">
    ///     The entitie's bounding base.
    /// </param>
    public void SetBoundingBase(BoundingBase boundingBase) => BoundingBase = boundingBase;

    public void Update(EntityBase @new)
    {
        if (Id != @new.Id)
            throw new InvalidOperationException($"Attempting to update entity with ID: {Id}, with data for entity with ID: {@new.Id}");

        ABS = @new.ABS;
        Angle = @new.Angle;
        Armor = @new.Armor;
        GoingX = @new.GoingX;
        GoingY = @new.GoingY;
        HP = @new.HP;
        MaxHP = @new.MaxHP;
        Level = @new.Level;
        MoveNum = @new.MoveNum;
        Moving = @new.Moving;
        Speed = @new.Speed;
        X = @new.X;
        Y = @new.Y;
        XP = @new.XP;
        Attack = @new.Attack;
        Frequency = @new.Frequency;
        MP = @new.MP;
        Resistance = @new.Resistance;
        Target = @new.Target;
        Conditions = @new.Conditions;
    }

    /// <summary>
    ///     Updates the instanced location of this entity.
    /// </summary>
    /// <param name="location">
    ///     An instanced location.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Invalid argument.
    /// </exception>
    public void UpdateLocation(IInstancedLocation location)
    {
        if (location.Equals(default))
            throw new ArgumentException("Invalid argument.", nameof(location));

        In = location.In;
        UpdateLocation((ILocation)location);
    }

    /// <summary>
    ///     Updates the location of this entity.
    /// </summary>
    /// <param name="location">
    ///     A location.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Invalid argument.
    /// </exception>
    public void UpdateLocation(ILocation location)
    {
        if (location.Equals(default))
            throw new ArgumentException("Invalid argument.", nameof(location));

        Map = location.Map;
        UpdateLocation((IPoint)location);
    }

    /// <summary>
    ///     Updates the point of this entity.
    /// </summary>
    /// <param name="point">
    ///     A coordinate point.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Invalid argument.
    /// </exception>
    public void UpdateLocation(IPoint point)
    {
        if (point.Equals(default))
            throw new ArgumentException("Invalid argument.", nameof(point));

        X = point.X;
        Y = point.Y;
    }

    /// <summary>
    ///     Updates this entity's instance and map.
    /// </summary>
    /// <param name="in">
    ///     The map or instance this entity is in.
    /// </param>
    /// <param name="map">
    ///     The map this entity is in.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     in
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     map
    /// </exception>
    public void UpdateMap(string @in, string map)
    {
        if (string.IsNullOrEmpty(@in))
            throw new ArgumentNullException(nameof(@in));

        if (string.IsNullOrEmpty(map))
            throw new ArgumentNullException(nameof(map));

        In = @in;
        Map = map;
    }
}