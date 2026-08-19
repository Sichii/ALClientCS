#region
using System.Text.Json.Serialization;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.SocketClient.SocketModel;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents a character. Contains additional information that the owner of a player would have.
/// </summary>
/// <remarks>
///     Mutated by <see cref="NewMapData" />, <see cref="ILocation" />
/// </remarks>
/// <seealso cref="Player" />
/// <seealso cref="IMutable{TMutator}" />
public class Character : Player, IEquatable<Character>
{
    /// <summary>
    ///     The number of entities currently targeting this player.
    /// </summary>
    [JsonPropertyName("targets")]
    [JsonInclude]
    public int AggroTargets { get; protected set; }

    /// <summary>
    ///     If populated, this character is inside of a bank.
    ///     <br />
    ///     This property only populates while you are inside of a bank, and hold information about gold and items this
    ///     character has in the bank.
    /// </summary>
    [JsonPropertyName("user")]
    [JsonInclude]
    public BankInfo? Bank { get; protected set; }

    /// <summary>
    ///     TODO: unknown
    /// </summary>
    [JsonInclude]
    public int Cache { get; protected set; }

    /// <summary>
    ///     The account's premium currency (shells) balance.
    /// </summary>
    [JsonPropertyName("cash")]
    [JsonInclude]
    public int Cash { get; protected set; }

    /// <summary>
    ///     If populated, this contains all of the code executing for this character.
    /// </summary>
    [JsonInclude]
    public new string? Code { get; protected set; }

    /// <summary>
    ///     What the network calls this character has made are costing it. Not all calls are equal, and this is not a
    ///     count - see <see cref="Definitions.CallCost" /> for the table the server prices them from.
    ///     <br />
    ///     It is a <b>sliding</b> window rather than a resetting counter: the server drops entries older than
    ///     <see cref="Definitions.CallCost.WINDOW" /> off the front before every read, so a burst decays over that
    ///     long instead of clearing on a tick boundary.
    ///     <br />
    ///     <b>
    ///         Above <see cref="Definitions.CallCost.LIMIT" /> the server sends a limitdcreport and disconnects.
    ///     </b>
    /// </summary>
    [JsonPropertyName("cc")]
    [JsonInclude]
    public float CodeCost { get; protected set; }

    /// <summary>
    ///     The number of empty slots in this character's inventory.
    /// </summary>
    [JsonPropertyName("esize")]
    [JsonInclude]
    public int EmptySlots { get; protected set; }

    /// <summary>
    ///     A lag allowance the server spends on your behalf: one budget per character, shared by every skill including
    ///     attack. It starts at 25 and refills 5 per second back to that cap, unconditionally - attacking does not
    ///     hold it back.
    ///     <br />
    ///     A cast lands when the distance is within <c>range + xrange</c>, and one that needed the allowance drains it
    ///     by however much of it was used. A cast past that is refused with <c>too_far</c> and costs nothing, so the
    ///     budget only ever pays for casts that worked. Nothing refuses while it holds, which is why a range check
    ///     measuring against the nominal range alone reads as correct right up until it runs dry.
    /// </summary>
    [JsonPropertyName("xrange")]
    [JsonInclude]
    public float ExtraRange { get; protected set; }

    /// <summary>
    ///     When the number of monsters you are attacking exceeds the courage value for that type of monster, you start to gain
    ///     fear.
    ///     <br />
    ///     This value is the number of monsters you are exceeding your courage by.
    /// </summary>
/// <summary>
    ///     The number of physically-attacking monsters this character can face before gaining fear. Sent on every
    ///     character frame; the server counts physical attackers against this one, magical against
    ///     <see cref="MCourage" /> and pure against <see cref="PCourage" />.
    /// </summary>
    [JsonPropertyName("courage")]
    [JsonInclude]
    public int Courage { get; protected set; }
    [JsonInclude]
    public float Fear { get; protected set; }

    /// <summary>
    ///     The character's gold-find multiplier (1 + xgold/100).
    /// </summary>
    [JsonPropertyName("goldm")]
    [JsonInclude]
    public float GoldMultiplier { get; protected set; }

    /// <summary>
    ///     A percentage bonus applied to incoming damage (0 for most classes).
    /// </summary>
    [JsonPropertyName("incdmgamp")]
    [JsonInclude]
    public int IncomingDamageAmp { get; protected set; }

    /// <summary>
    ///     The character's inventory.
    /// </summary>
    [JsonPropertyName("items")]
    [JsonInclude]
    public Inventory Inventory { get; internal set; } = null!;

    /// <summary>
    ///     The maximum size of this character's inventory.
    /// </summary>
    [JsonPropertyName("isize")]
    [JsonInclude]
    public int InventorySize { get; protected set; }

    /// <summary>
    ///     The character's luck multiplier (1 + xluck/100).
    /// </summary>
    [JsonPropertyName("luckm")]
    [JsonInclude]
    public float LuckMultiplier { get; protected set; }

    /// <summary>
    ///     The number of times this character has changed maps.
    /// </summary>
    [JsonPropertyName("m")]
    [JsonInclude]
    public ulong MapChangeCount { get; protected set; }

    /// <summary>
    ///     The total xp required to reach the next level. (large enough to overflow <see cref="int" /> at high level)
    /// </summary>
    [JsonPropertyName("max_xp")]
    [JsonInclude]
    public long MaxXP { get; protected set; }

    /// <summary>
    ///     The number of magical monsters this character can face before gaining fear.
    /// </summary>
    [JsonPropertyName("mcourage")]
    [JsonInclude]
    public int MCourage { get; protected set; }

    /// <summary>
    ///     The mp cost of the character's basic attack.
    /// </summary>
    [JsonPropertyName("mp_cost")]
    [JsonInclude]
    public new int MPCost { get; protected set; }

    /// <summary>
    ///     The number of pure-damage monsters this character can face before gaining fear. Pure only - a physical
    ///     attacker counts against <see cref="Courage" /> instead.
    /// </summary>
    [JsonPropertyName("pcourage")]
    [JsonInclude]
    public int PCourage { get; protected set; }

    /// <summary>
    ///     When you buy or sell an item, there is a fraction of that item's value you pay in taxes.
    ///     <br />
    ///     For merchants, this tax is converted into earned xp.
    /// </summary>
    [JsonInclude]
    public float Tax { get; protected set; }

    /// <summary>
    ///     The character's xp multiplier (1 + xxp/100).
    /// </summary>
    [JsonPropertyName("xpm")]
    [JsonInclude]
    public float XPMultiplier { get; protected set; }

    public virtual bool Equals(Character? other) => base.Equals(other);

    public override bool Equals(object? obj) => Equals(obj as Character);

    public override int GetHashCode() => base.GetHashCode();

    /// <summary>
    ///     System.Text.Json post-deserialize step: after the base slot fill, size the inventory to
    ///     <see cref="InventorySize" /> (bound by this point). Reproduces the Newtonsoft CharacterConverter enrich.
    /// </summary>
    /// <remarks>
    ///     A frame that carries no
    ///     <c>
    ///         items
    ///     </c>
    ///     key leaves <see cref="Inventory" /> null, so the sizing is skipped rather than dereferencing it — there is no
    ///     inventory to size, and the alternative is a <see cref="NullReferenceException" /> that kills the whole frame.
    /// </remarks>
    public override void OnDeserialized()
    {
        base.OnDeserialized();
        Inventory?.SetCapacity(InventorySize);
    }

    /// <summary>
    ///     Sets this character as moving to a point.
    /// </summary>
    /// <param name="point">
    ///     Where to walk to.
    /// </param>
    /// <returns>
    ///     The movement block this left the character holding. Returned so a caller needing the move number this
    ///     walk was started with reads it from the same write rather than from a second, later look at the character.
    /// </returns>
    public MovementBlock SetMoving(IPoint point)
    {
        //materialised before the lock: ALClient hands a live Player here, and calling into a foreign implementation
        //while holding this character's lock is how a deadlock gets built later
        var going = new Point(point.X, point.Y);

        lock (MovementLock)
        {
            var movement = ReadMovement() with
            {
                GoingX = going.X,
                GoingY = going.Y,
                Angle = going.AngularRelationTo(this),
                Moving = true
            };

            ApplyMovement(movement);

            return movement;
        }
    }

    /// <summary>
    ///     Sets this character as stopped wherever theyre currently standing.
    /// </summary>
    public void StopMoving()
    {
        lock (MovementLock)
        {
            var movement = ReadMovement();

            ApplyMovement(
                movement with
                {
                    GoingX = movement.X,
                    GoingY = movement.Y,
                    Moving = false
                });
        }
    }

    /// <summary>
    ///     Updates the map information of this entity.
    /// </summary>
    /// <param name="data">
    ///     The new map information.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     data
    /// </exception>
    public void UpdateLocation(NewMapData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        MapChangeCount = data.MapChangeCount;

        //one atomic operation, not two - a reckoning tick landing between them would walk the character away from
        //the arrival spot the stop just pinned. Both nested calls re-enter the lock
        lock (MovementLock)
        {
            StopMoving();
            UpdateLocation((IInstancedLocation)data);
        }
    }

    /// <summary>
    ///     Folds an in-progress upgrade or compound's detail onto the inventory slot it belongs to.
    /// </summary>
    /// <remarks>
    ///     The server sends this on <c>q_data</c> and only there - it names the slot and carries the operation's
    ///     prediction, and it never restates the inventory. So a consumer that only merges character frames sees the
    ///     placeholder but never the detail underneath it, which is where the roll's digits live and where they are
    ///     revealed one at a time as the animation counts down.
    /// </remarks>
    /// <param name="inventorySlot">
    ///     The slot holding the operation's placeholder.
    /// </param>
    /// <param name="prediction">
    ///     The operation's current detail.
    /// </param>
    public void Update(int inventorySlot, Prediction prediction)
    {
        ArgumentNullException.ThrowIfNull(prediction);

        //null before the first character frame lands, which a q_data cannot precede in practice but costs nothing
        //to allow for
        Inventory?.SetPrediction(inventorySlot, prediction);
    }
}