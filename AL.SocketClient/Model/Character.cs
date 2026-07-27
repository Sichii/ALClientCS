#region
using System.Text.Json.Serialization;
using AL.Core.Extensions;
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
    ///     A value indicating the impact of the network calls this character is making.
    ///     <br />
    ///     Not all network calls are equal in value. This number resets to 0 every second.
    ///     <br />
    ///     <b>
    ///         If this number goes above 200, you will be disconnected.
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
    ///     While you are not attacking, your range increases by 5/second, up to a max of 25.
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
    ///     The number of pure/physical monsters this character can face before gaining fear.
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
    /// </param>
    /// <returns>
    /// </returns>
    public void SetMoving(IPoint point)
    {
        GoingX = point.X;
        GoingY = point.Y;
        Angle = point.AngularRelationTo(this);
        Moving = true;
    }

    /// <summary>
    ///     Sets this character as stopped wherever theyre currently standing.
    /// </summary>
    public void StopMoving()
    {
        Moving = false;
        GoingX = X;
        GoingY = Y;
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
        StopMoving();
        UpdateLocation((IInstancedLocation)data);
    }
}