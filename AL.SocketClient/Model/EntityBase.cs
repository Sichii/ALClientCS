#region
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json.Serialization;
using AL.Core.Abstractions;
using AL.Core.Attributes;
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
    /// <summary>
    ///     Serializes every read-compute-write of the movement block across the delta loop, the socket and movement calls.
    ///     Readers stay lock-free.
    ///     <br />
    ///     Deserialization writes the setters without it, which holds only while a deserialized entity is not yet published.
    /// </summary>
    protected private readonly Lock MovementLock = new();

    protected BoundingBase BoundingBase = null!;

    /// <summary>TODO: what's this?</summary>
    [JsonInclude]
    public bool ABS { get; protected set; }

    /// <summary>
    ///     If moving, this is the angle they are moving at. (in degrees +/- 180)
    /// </summary>
    [JsonInclude]
    [ShallowMergeIgnore]
    public float Angle { get; private set; }

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
    [ShallowMergeIgnore]
    public float GoingX { get; private set; }

    /// <summary>
    ///     If this entity is moving, this is the Y coordinate they are moving to.
    /// </summary>
    [JsonPropertyName("going_y")]
    [JsonInclude]
    [ShallowMergeIgnore]
    public float GoingY { get; private set; }

    /// <summary>
    ///     The box this entity's <i>range</i> is measured against, as opposed to the collision footprint this class presents
    ///     as its rectangle. It follows the entity as it moves.
    /// </summary>
    public IRectangle HitBox { get; private set; } = null!;

    /// <summary>
    ///     <see cref="Player" /> name, or <see cref="Monster" /> unique id.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    ///     The map or instance this entity is in.
    /// </summary>
    /// <remarks>
    ///     Only a self <c>player</c> frame carries <c>in</c> per entity. An <c>entities</c> frame carries it once for the
    ///     whole frame, so every entity in one is stamped by hand.
    /// </remarks>
    [JsonPropertyName("in")]
    [JsonInclude]
    [ShallowMergeIgnore]
    public string? In { get; private set; }

    public bool IsCompensated { get; private set; }

    [JsonInclude]
    public int Level { get; protected set; }

    [JsonPropertyName("map")]
    [JsonInclude]
    [ShallowMergeIgnore]
    public string Map { get; private set; } = null!;

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
    [ShallowMergeIgnore]
    public ulong MoveNum { get; private set; }

    [JsonInclude]
    [ShallowMergeIgnore]
    public bool Moving { get; private set; }

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
    [ShallowMergeIgnore]
    public float X { get; private set; }

    [JsonInclude]
    [ShallowMergeIgnore]
    public float Y { get; private set; }

    /// <summary>
    ///     Where this entity is and where it is walking, read as one value under the lock every writer takes. Use it where a
    ///     position has to agree with the destination it was derived from.
    /// </summary>
    public MovementBlock Movement
    {
        get
        {
            lock (MovementLock)
                return ReadMovement();
        }
    }

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

    public void CompensateOnce(TimeSpan offset)
    {
        //Update re-enters this lock, which System.Threading.Lock permits
        lock (MovementLock)
        {
            if (IsCompensated)
                throw new InvalidOperationException("Object already compensated.");

            IsCompensated = true;

            Update(offset);
        }
    }

    public virtual bool Equals(EntityBase? other) => other is not null && Id.Equals(other.Id);

    public bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

    public bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);

    public IEnumerator<IPoint> GetEnumerator() => Vertices.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    ///     Records that <paramref name="key" /> was present on the wire. Keys <see cref="Update(EntityBase)" /> does not
    ///     merge are ignored.
    /// </summary>
    /// <param name="key">
    ///     The wire key.
    /// </param>
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
            "in"         => EntityUpdateField.In,
            "level"      => EntityUpdateField.Level,
            "map"        => EntityUpdateField.Map,
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
        //a condition sent without "ms" (the encouragement bonuses) has no timer; it lasts until a frame drops it
        Conditions.UpdateAndTryRemoveWhere(delta, condition => (condition.DurationMs > 0) && (condition.RemainingMs <= 0));

        //the read stays inside the lock so a correction cannot land between it and the write
        lock (MovementLock)
        {
            var movement = ReadMovement();

            //if not moving, or less than 1ms has passed, or already at destination, then dont update
            if (!movement.Moving
                || (movement.X.IsNear(movement.GoingX, CONSTANTS.EPSILON) && movement.Y.IsNear(movement.GoingY, CONSTANTS.EPSILON)))
                return;

            var going = new Point(movement.GoingX, movement.GoingY);
            var distanceDelta = Convert.ToSingle(Speed * delta.TotalSeconds);
            var distance = this.Distance(going);

            if (distance > distanceDelta)
                distance = distanceDelta;
            else
            {
                ApplyMovement(
                    movement with
                    {
                        X = movement.GoingX,
                        Y = movement.GoingY,
                        Moving = false
                    });

                return;
            }

            //steer toward going from here; Angle goes stale when a write moves x/y without re-deriving it
            (var newX, var newY) = this.AngularOffset(going.AngularRelationTo(this), distance);

            ApplyMovement(
                movement with
                {
                    X = newX,
                    Y = newY
                });
        }
    }

    /// <summary>
    ///     Applies a server frame's movement wholesale: a key the frame omitted lands as its deserialized default, and the
    ///     frame wins over local reckoning.
    /// </summary>
    /// <remarks>
    ///     A character frame omits every movement key until the character's first move of the session, so gating on
    ///     <see cref="PresentFields" /> would leave a reconnected character walking a leg from the server it left.
    /// </remarks>
    /// <param name="frame">
    ///     The freshly-deserialized frame to take movement from.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     frame
    /// </exception>
    public void AcceptMovement(EntityBase frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        const EntityUpdateField MOVEMENT_FIELDS = EntityUpdateField.X
                                                  | EntityUpdateField.Y
                                                  | EntityUpdateField.GoingX
                                                  | EntityUpdateField.GoingY
                                                  | EntityUpdateField.Angle
                                                  | EntityUpdateField.MoveNum
                                                  | EntityUpdateField.Moving
                                                  | EntityUpdateField.Map
                                                  | EntityUpdateField.In;

        //read the frame's block before taking our own lock, so no two entity locks are ever held at once
        var incoming = frame.Movement;

        lock (MovementLock)
            ApplyMovement(MergeMovement(ReadMovement(), incoming, MOVEMENT_FIELDS));
    }

    protected private void ApplyMovement(MovementBlock movement)
    {
        Debug.Assert(MovementLock.IsHeldByCurrentThread, "the movement block may only be assigned while holding MovementLock");

        X = movement.X;
        Y = movement.Y;
        GoingX = movement.GoingX;
        GoingY = movement.GoingY;
        Angle = movement.Angle;
        MoveNum = movement.MoveNum;
        Moving = movement.Moving;
        Map = movement.Map!;
        In = movement.In;
    }

    /// <summary>
    ///     Seeds a soft property from its game-data default, unless the frame this entity was deserialized from carried it.
    /// </summary>
    /// <param name="field">
    ///     The soft property.
    /// </param>
    /// <param name="value">
    ///     Its game-data default.
    /// </param>
    /// <remarks>
    ///     The server omits a soft property equal to its default, as the browser's <c>adopt_soft_properties</c> assumes.
    /// </remarks>
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
            case EntityUpdateField.Range:
                Range = value;

                break;
            case EntityUpdateField.Level:
                Level = (int)value;

                break;
        }
    }

    public void CorrectAndCompensate(IPoint point, TimeSpan offset)
    {
        ArgumentNullException.ThrowIfNull(point);

        //copied before the lock, so no foreign IPoint implementation is called while holding it
        var corrected = new Point(point.X, point.Y);

        //held across both so no reckoning tick lands between them; both calls re-enter it
        lock (MovementLock)
        {
            IsCompensated = false;
            UpdateLocation(corrected);
            CompensateOnce(offset);
        }
    }

    public override bool Equals(object? obj) => Equals(obj as EntityBase);

    /// <summary>
    ///     Merges the fields of <paramref name="incoming" /> that <paramref name="present" /> lets through onto
    ///     <paramref name="current" />.
    /// </summary>
    /// <param name="current">
    ///     The movement block this entity holds.
    /// </param>
    /// <param name="incoming">
    ///     The movement block a server frame carried.
    /// </param>
    /// <param name="present">
    ///     The fields to take from <paramref name="incoming" />.
    /// </param>
    /// <returns>
    ///     The merged movement block.
    /// </returns>
    protected private static MovementBlock MergeMovement(MovementBlock current, MovementBlock incoming, EntityUpdateField present)
    {
        if ((present & EntityUpdateField.Angle) != 0)
            current = current with
            {
                Angle = incoming.Angle
            };

        if ((present & EntityUpdateField.GoingX) != 0)
            current = current with
            {
                GoingX = incoming.GoingX
            };

        if ((present & EntityUpdateField.GoingY) != 0)
            current = current with
            {
                GoingY = incoming.GoingY
            };

        if ((present & EntityUpdateField.In) != 0)
            current = current with
            {
                In = incoming.In
            };

        if ((present & EntityUpdateField.Map) != 0)
            current = current with
            {
                Map = incoming.Map
            };

        if ((present & EntityUpdateField.MoveNum) != 0)
            current = current with
            {
                MoveNum = incoming.MoveNum
            };

        if ((present & EntityUpdateField.Moving) != 0)
            current = current with
            {
                Moving = incoming.Moving
            };

        if ((present & EntityUpdateField.X) != 0)
            current = current with
            {
                X = incoming.X
            };

        if ((present & EntityUpdateField.Y) != 0)
            current = current with
            {
                Y = incoming.Y
            };

        return current;
    }

    /// <summary>
    ///     Reads the movement block as one value.
    /// </summary>
    /// <returns>
    ///     The movement block.
    /// </returns>
    /// <remarks>
    ///     The caller must hold <see cref="MovementLock" />, as must any caller of <see cref="ApplyMovement" />.
    /// </remarks>
    protected private MovementBlock ReadMovement()
        => new(
            X,
            Y,
            GoingX,
            GoingY,
            Angle,
            MoveNum,
            Moving,
            Map,
            In);

    /// <summary>Sets the bounding base of the entity.</summary>
    /// <param name="boundingBase">The entity's bounding base.</param>
    public void SetBoundingBase(BoundingBase boundingBase) => BoundingBase = boundingBase;

    public void SetHitBox(BoundingBase hitBox) => HitBox = new BoundingRectangle(this, hitBox);

    /// <summary>
    ///     Merges a freshly-deserialized frame into this live entity, copying only the fields the frame carried.
    /// </summary>
    /// <param name="new">
    ///     The freshly-deserialized frame.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     The frame is for a different entity.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     new
    /// </exception>
    public void Update(EntityBase @new)
    {
        ArgumentNullException.ThrowIfNull(@new);

        if (Id != @new.Id)
            throw new InvalidOperationException($"Attempting to update entity with ID: {Id}, with data for entity with ID: {@new.Id}");

        var present = @new.PresentFields;

        //taken before our own lock, so no two entity locks are ever held at once
        var incoming = @new.Movement;

        //gated per member; the lock only makes the carried movement fields land together
        lock (MovementLock)
            ApplyMovement(MergeMovement(ReadMovement(), incoming, present));

        if ((present & EntityUpdateField.ABS) != 0)
            ABS = @new.ABS;

        if ((present & EntityUpdateField.Armor) != 0)
            Armor = @new.Armor;

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

        if ((present & EntityUpdateField.Speed) != 0)
            Speed = @new.Speed;

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
    ///     Updates the instanced location of this entity. The instance, the map and the position land together, so no reader
    ///     ever sees the new map at the old position.
    /// </summary>
    /// <param name="location">
    ///     An instanced location.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     location
    /// </exception>
    public void UpdateLocation(IInstancedLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);

        //copied before the lock, so no foreign implementation is called while holding it
        var @in = location.In;
        var map = location.Map;
        var x = location.X;
        var y = location.Y;

        lock (MovementLock)
            ApplyMovement(
                ReadMovement() with
                {
                    X = x,
                    Y = y,
                    Map = map,
                    In = @in
                });
    }

    /// <summary>
    ///     Updates the map and position of this entity, as one write.
    /// </summary>
    /// <param name="location">
    ///     A location.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     location
    /// </exception>
    public void UpdateLocation(ILocation location)
    {
        ArgumentNullException.ThrowIfNull(location);

        var map = location.Map;
        var x = location.X;
        var y = location.Y;

        lock (MovementLock)
            ApplyMovement(
                ReadMovement() with
                {
                    X = x,
                    Y = y,
                    Map = map
                });
    }

    /// <summary>Updates the point of this entity.</summary>
    /// <param name="point">A coordinate point.</param>
    /// <exception cref="ArgumentNullException">point</exception>
    public void UpdateLocation(IPoint point)
    {
        ArgumentNullException.ThrowIfNull(point);

        var x = point.X;
        var y = point.Y;

        lock (MovementLock)
            ApplyMovement(
                ReadMovement() with
                {
                    X = x,
                    Y = y
                });
    }

    /// <summary>Updates this entity's instance and map.</summary>
    /// <param name="in">The map or instance this entity is in.</param>
    /// <param name="map">The map this entity is in.</param>
    /// <exception cref="ArgumentNullException">in</exception>
    /// <exception cref="ArgumentNullException">map</exception>
    public void UpdateMap(string @in, string map)
    {
        if (string.IsNullOrEmpty(@in))
            throw new ArgumentNullException(nameof(@in));

        if (string.IsNullOrEmpty(map))
            throw new ArgumentNullException(nameof(map));

        lock (MovementLock)
            ApplyMovement(
                ReadMovement() with
                {
                    Map = map,
                    In = @in
                });
    }
}