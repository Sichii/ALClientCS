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
    //every member of the movement block, so the wholesale path runs the same merge the gated one does
    private const EntityUpdateField MOVEMENT_FIELDS = EntityUpdateField.X
                                                      | EntityUpdateField.Y
                                                      | EntityUpdateField.GoingX
                                                      | EntityUpdateField.GoingY
                                                      | EntityUpdateField.Angle
                                                      | EntityUpdateField.MoveNum
                                                      | EntityUpdateField.Moving
                                                      | EntityUpdateField.Map
                                                      | EntityUpdateField.In;

    protected BoundingBase BoundingBase = null!;

    /// <summary>
    ///     Serializes every write to the movement block. Three threads write it - the 30Hz delta loop, the socket's
    ///     receive callbacks and the consumer's own movement calls - and each write is a read-compute-write, so a
    ///     correction landing mid-computation was silently clobbered by a step derived from the position before it.
    ///     Readers are deliberately left lock-free; only writers serialize.
    ///     <br />
    ///     Deserialization is the one writer that does not take it: <c>[JsonInclude]</c> drives the narrowed setters
    ///     straight through. That is safe only because a freshly-deserialized entity is thread-local until it is
    ///     published, and it would stop being safe the day a frame is deserialized <i>into</i> a live entity.
    /// </summary>
    private protected readonly Lock MovementLock = new();

    /// <summary>
    ///     TODO: what's this?
    /// </summary>
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
    ///     <see cref="Player" /> name, or <see cref="Monster" /> unique id.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    ///     The map or instance this entity is in.
    /// </summary>
    /// <remarks>
    ///     Only a self 'player' frame carries this - player_to_client (node/server.js:732) lists 'in' in the !stranger
    ///     block alongside 'map'. An entities frame carries it once for the whole frame instead, which is why every
    ///     entity in one is stamped by hand. Left unbound, the shallow merge behind a player frame wrote null over
    ///     whatever the last entities frame stamped, and InSameInstanceAs answers false against a null - so the vision
    ///     sweep evicted every monster and player at once, several times a second.
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

    /// <summary>
    ///     The box this entity's <i>range</i> is measured against, as opposed to the collision foot-print the rest of
    ///     this class presents as its rectangle. It tracks the entity, since it is built over this instance rather
    ///     than over a snapshot of where it was standing.
    /// </summary>
    public IRectangle HitBox { get; private set; } = null!;

    /// <summary>
    ///     Where this entity is and where it is walking, read as one coherent value under the lock every writer takes.
    ///     Use it where two of these have to agree with each other - a position against the destination it was derived
    ///     from, say. Single-property reads stay lock-free and are unaffected.
    /// </summary>
    public MovementBlock Movement
    {
        get
        {
            lock (MovementLock)
                return ReadMovement();
        }
    }

    public void SetHitBox(BoundingBase hitBox) => HitBox = new BoundingRectangle(this, hitBox);

    /// <summary>
    ///     Applies a server frame's movement wholesale, exactly as sent - a key the frame omitted lands as its
    ///     deserialized default rather than keeping what was there. No arbitration either: a frame that disagrees with
    ///     local reckoning still wins, which is what the shallow merge behind a character frame always did.
    /// </summary>
    /// <remarks>
    ///     Wholesale rather than gated on <see cref="PresentFields" /> on purpose, and the difference is real: a
    ///     character frame omits every movement key until the character's first move of the session, so a committed
    ///     capture carries <c>x</c>, <c>y</c>, <c>map</c> and <c>in</c> and none of <c>moving</c>, <c>going_x</c>,
    ///     <c>going_y</c>, <c>angle</c> or <c>move_num</c> (<c>player_to_client</c> sends a key only when the server's
    ///     player object has one, <c>node/server.js:778</c>). Gating would let a reconnect's start frame leave a
    ///     persistent character walking to the destination of a leg on the server it just left.
    /// </remarks>
    /// <param name="frame">
    ///     The freshly-deserialized frame to take movement from.
    /// </param>
    public void AcceptMovement(EntityBase frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        //read the frame's block before taking our own lock, so no two entity locks are ever held at once
        var incoming = frame.Movement;

        lock (MovementLock)
            ApplyMovement(MergeMovement(ReadMovement(), incoming, MOVEMENT_FIELDS));
    }

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

    //the one place the block is read as a group, and the one place any member of it is assigned. Both assume the
    //caller already holds MovementLock - nothing else in this class or Character may touch the members directly
    private protected MovementBlock ReadMovement()
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

    private protected void ApplyMovement(MovementBlock movement)
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

    //the one place a server frame becomes movement, for both paths that apply one. Whichever fields the mask lets
    //through land together; the rest keep what they had. When an arbitration rule arrives - ignore a frame whose
    //move_num is behind, snap past a distance threshold - this is where it goes, and it is written once
    private protected static MovementBlock MergeMovement(MovementBlock current, MovementBlock incoming, EntityUpdateField present)
    {
        if ((present & EntityUpdateField.Angle) != 0)
            current = current with { Angle = incoming.Angle };

        if ((present & EntityUpdateField.GoingX) != 0)
            current = current with { GoingX = incoming.GoingX };

        if ((present & EntityUpdateField.GoingY) != 0)
            current = current with { GoingY = incoming.GoingY };

        if ((present & EntityUpdateField.In) != 0)
            current = current with { In = incoming.In };

        if ((present & EntityUpdateField.Map) != 0)
            current = current with { Map = incoming.Map };

        if ((present & EntityUpdateField.MoveNum) != 0)
            current = current with { MoveNum = incoming.MoveNum };

        if ((present & EntityUpdateField.Moving) != 0)
            current = current with { Moving = incoming.Moving };

        if ((present & EntityUpdateField.X) != 0)
            current = current with { X = incoming.X };

        if ((present & EntityUpdateField.Y) != 0)
            current = current with { Y = incoming.Y };

        return current;
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
        Conditions.TickAndTryRemoveWhere(delta, condition => condition.RemainingMs <= 0);

        //the whole read-compute-write is inside the lock, not just the write. A correction landing between the read
        //and the write is otherwise clobbered by a step derived from the position it just replaced, silently
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

            //steer by where going is from here, rather than by the heading the leg started with. Several paths write
            //x/y between ticks without re-deriving Angle - the server's correction handler, MoveAsync's in-leg repair,
            //the character branch of an entities frame - and a stale heading resumes from the new position along a line
            //parallel to the one it should be on, until the overshoot clamp finally trips
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
        //materialised before the lock: point can be another live entity, and calling into a foreign implementation
        //while holding this entity's lock is how a deadlock gets built later
        var corrected = new Point(point.X, point.Y);

        //one atomic operation, not two - a reckoning tick that landed between them would step from the corrected
        //position and then be compensated from a position it had already left. Both nested calls re-enter the lock
        lock (MovementLock)
        {
            IsCompensated = false;
            UpdateLocation(corrected);
            CompensateOnce(offset);
        }
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

        //taken before our own lock, so no two entity locks are ever held at once
        var incoming = @new.Movement;

        //each member stays individually gated: a partial delta must not overwrite a field the server omitted. What
        //the lock adds is that whichever ones the frame did carry land together
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
    ///     Updates the instanced location of this entity. The instance, the map and the position land together, so no
    ///     reader ever sees the new map at the old position.
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

        //read the argument before the lock: ALClient hands a live Player here, and calling into a foreign
        //implementation while holding this entity's lock is how a deadlock gets built later
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

    /// <summary>
    ///     Updates the point of this entity.
    /// </summary>
    /// <param name="point">
    ///     A coordinate point.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     point
    /// </exception>
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

        lock (MovementLock)
            ApplyMovement(
                ReadMovement() with
                {
                    Map = map,
                    In = @in
                });
    }
}