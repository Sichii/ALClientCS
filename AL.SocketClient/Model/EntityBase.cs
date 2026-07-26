#region
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Core.Model;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using Chaos.Time.Abstractions;
using StjConverters = AL.Core.Json.SystemTextJson;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Provides a base for <see cref="Player" />s and <see cref="Monster" />s.
/// </summary>
/// <seealso cref="AttributedRecordBase" />
/// <seealso cref="IBounding" />
/// <seealso cref="IRectangle" />
/// <seealso cref="IInstancedLocation" />
/// <seealso cref="IDeltaUpdatable" />
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
                                   IKeyPresenceCapturable,
                                   IEquatable<EntityBase>
{
    protected BoundingBase BoundingBase = null!;

    /// <summary>
    ///     TODO: what's this?
    /// </summary>
    [JsonInclude]
    public bool ABS { get; protected set; }

    /// <summary>
    ///     If moving, this is the angle they are moving at. (in degrees +/- 180)
    /// </summary>
    [JsonInclude]
    public float Angle { get; protected set; }

    /// <summary>
    ///     The conditions this entity has.
    ///     <br />
    ///     <b>
    ///         THIS COLLECTION IS SYNCHRONIZED, DO NOT DO LONG RUNNING OPERATIONS WHILE ITERATING IT.
    ///     </b>
    /// </summary>
    [JsonPropertyName("s")]
    [JsonInclude]
    [JsonConverter(typeof(StjConverters.TolerantEnumKeyDictionaryConverter<Core.Definitions.Condition, Condition>))]
    public ConcurrentDictionary<Core.Definitions.Condition, Condition> Conditions { get; protected set; } = new();

    /// <summary>
    ///     A unique ID for this version of the entity's data.
    ///     <br />
    ///     This number starts at 0 and iterates by 1 every time a new version of this entity's data is sent.
    /// </summary>
    [JsonPropertyName("cid")]
    public int ContinuousId { get; init; }

    /// <summary>
    ///     If populated, the <see cref="Id" /> of the entity this one is focused on.
    /// </summary>
    [JsonPropertyName("focus")]
    [JsonInclude]
    public string? Focus { get; protected set; }

    /// <summary>
    ///     If this entity is moving, this is the X coordinate they are moving to.
    /// </summary>
    [JsonPropertyName("going_x")]
    [JsonInclude]
    public float GoingX { get; protected set; }

    /// <summary>
    ///     If this entity is moving, this is the Y coordinate they are moving to.
    /// </summary>
    [JsonPropertyName("going_y")]
    [JsonInclude]
    public float GoingY { get; protected set; }

    /// <summary>
    ///     <see cref="Player" /> name, or <see cref="Monster" /> unique id.
    /// </summary>
    public string Id { get; init; } = null!;

    public string? In { get; protected set; }

    public bool IsCompensated { get; private set; }

    [JsonInclude]
    public int Level { get; protected set; }

    [JsonPropertyName("map")]
    [JsonInclude]
    public string Map { get; protected set; } = null!;

    [JsonPropertyName("max_hp")]
    [JsonInclude]
    public float MaxHP { get; protected set; }

    [JsonPropertyName("max_mp")]
    [JsonInclude]
    public float MaxMP { get; protected set; }

    /// <summary>
    ///     The number of individual movements this entity has done.
    /// </summary>
    [JsonPropertyName("move_num")]
    [JsonInclude]
    public ulong MoveNum { get; protected set; }

    [JsonInclude]
    public bool Moving { get; protected set; }

    /// <summary>
    ///     Which wire keys the frame this entity was deserialized from actually carried. Set by <see cref="MarkPresent" />
    ///     during deserialization; consumed by <see cref="Update(EntityBase)" /> so a partial delta never overwrites a field
    ///     the server omitted.
    /// </summary>
    [JsonIgnore]
    public EntityUpdateField PresentFields { get; private set; }

    /// <summary>
    ///     If populated, the <see cref="Id" /> of this entity's target.
    /// </summary>
    [JsonInclude]
    public string? Target { get; protected set; }

    [JsonInclude]
    public float X { get; protected set; }

    [JsonInclude]
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

    /// <summary>
    ///     Records that <paramref name="key" /> was present on the wire. Allocation-free: it only ORs a flag. Unlisted keys
    ///     (bank data, cosmetics, etc.) are not mergeable through <see cref="Update(EntityBase)" /> and are ignored here.
    /// </summary>
    public void MarkPresent(string key)
        => PresentFields |= key switch
        {
            "abs"        => EntityUpdateField.ABS,
            "angle"      => EntityUpdateField.Angle,
            "armor"      => EntityUpdateField.Armor,
            "attack"     => EntityUpdateField.Attack,
            "s"          => EntityUpdateField.Conditions,
            "focus"      => EntityUpdateField.Focus,
            "frequency"  => EntityUpdateField.Frequency,
            "going_x"    => EntityUpdateField.GoingX,
            "going_y"    => EntityUpdateField.GoingY,
            "hp"         => EntityUpdateField.HP,
            "level"      => EntityUpdateField.Level,
            "max_hp"     => EntityUpdateField.MaxHP,
            "max_mp"     => EntityUpdateField.MaxMP,
            "move_num"   => EntityUpdateField.MoveNum,
            "moving"     => EntityUpdateField.Moving,
            "mp"         => EntityUpdateField.MP,
            "resistance" => EntityUpdateField.Resistance,
            "speed"      => EntityUpdateField.Speed,
            "target"     => EntityUpdateField.Target,
            "x"          => EntityUpdateField.X,
            "xp"         => EntityUpdateField.XP,
            "y"          => EntityUpdateField.Y,
            _            => EntityUpdateField.None
        };

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

    /// <summary>
    ///     Seeds a soft property from its G default, but only if the frame this entity was deserialized from did not already
    ///     carry it. The server omits a soft property that equals the G default, so a freshly-sighted monster reports 0 for
    ///     those until they are backfilled - mirrors the browser's
    ///     <c>
    ///         adopt_soft_properties
    ///     </c>
    ///     (
    ///     <c>
    ///         js/game.js:766-771
    ///     </c>
    ///     ). Only the numeric soft properties the encoder can omit are handled.
    /// </summary>
    public void BackfillSoftDefault(EntityUpdateField field, float value)
    {
        //the frame carried a real value for this field; never override it with the def
        if ((PresentFields & field) != 0)
            return;

        switch (field)
        {
            case EntityUpdateField.HP:
                HP = value;

                break;
            case EntityUpdateField.MaxHP:
                MaxHP = value;

                break;
            case EntityUpdateField.MP:
                MP = value;

                break;
            case EntityUpdateField.MaxMP:
                MaxMP = value;

                break;
            case EntityUpdateField.Attack:
                Attack = value;

                break;
            case EntityUpdateField.Speed:
                Speed = value;

                break;
            case EntityUpdateField.XP:
                XP = value;

                break;
            case EntityUpdateField.Frequency:
                Frequency = value;

                break;
            case EntityUpdateField.Armor:
                Armor = value;

                break;
            case EntityUpdateField.Resistance:
                Resistance = value;

                break;
            case EntityUpdateField.Level:
                Level = (int)value;

                break;
        }
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

    /// <summary>
    ///     Merges a freshly-deserialized frame into this live entity, copying only the fields the frame actually carried.
    ///     Mirrors the browser's received-key merge (
    ///     <c>
    ///         js/game.js:786
    ///     </c>
    ///     ) - a bare
    ///     <c>
    ///         {id,x,y}
    ///     </c>
    ///     delta leaves hp/speed/etc. untouched instead of zeroing them.
    /// </summary>
    public void Update(EntityBase @new)
    {
        if (Id != @new.Id)
            throw new InvalidOperationException($"Attempting to update entity with ID: {Id}, with data for entity with ID: {@new.Id}");

        var present = @new.PresentFields;

        if ((present & EntityUpdateField.ABS) != 0)
            ABS = @new.ABS;

        if ((present & EntityUpdateField.Angle) != 0)
            Angle = @new.Angle;

        if ((present & EntityUpdateField.Armor) != 0)
            Armor = @new.Armor;

        if ((present & EntityUpdateField.GoingX) != 0)
            GoingX = @new.GoingX;

        if ((present & EntityUpdateField.GoingY) != 0)
            GoingY = @new.GoingY;

        if ((present & EntityUpdateField.HP) != 0)
            HP = @new.HP;

        if ((present & EntityUpdateField.MaxHP) != 0)
            MaxHP = @new.MaxHP;

        if ((present & EntityUpdateField.MaxMP) != 0)
            MaxMP = @new.MaxMP;

        if ((present & EntityUpdateField.Focus) != 0)
            Focus = @new.Focus;

        if ((present & EntityUpdateField.Level) != 0)
            Level = @new.Level;

        if ((present & EntityUpdateField.MoveNum) != 0)
            MoveNum = @new.MoveNum;

        if ((present & EntityUpdateField.Moving) != 0)
            Moving = @new.Moving;

        if ((present & EntityUpdateField.Speed) != 0)
            Speed = @new.Speed;

        if ((present & EntityUpdateField.X) != 0)
            X = @new.X;

        if ((present & EntityUpdateField.Y) != 0)
            Y = @new.Y;

        if ((present & EntityUpdateField.XP) != 0)
            XP = @new.XP;

        if ((present & EntityUpdateField.Attack) != 0)
            Attack = @new.Attack;

        if ((present & EntityUpdateField.Frequency) != 0)
            Frequency = @new.Frequency;

        if ((present & EntityUpdateField.MP) != 0)
            MP = @new.MP;

        if ((present & EntityUpdateField.Resistance) != 0)
            Resistance = @new.Resistance;

        if ((present & EntityUpdateField.Target) != 0)
            Target = @new.Target;

        if ((present & EntityUpdateField.Conditions) != 0)
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