#region
using System;
using System.Collections.Generic;
using AL.Core.Extensions;
using AL.Core.Interfaces;
using AL.SocketClient.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;
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
[JsonConverter(typeof(CharacterConverter<Character>))]
public class Character : Player, IEquatable<Character>
{
    /// <summary>
    ///     The number of entities currently targeting this player.
    /// </summary>
    [JsonProperty("targets")]
    public int AggroTargets { get; protected set; }

    /// <summary>
    ///     If populated, this character is inside of a bank.
    ///     <br />
    ///     This property only populates while you are inside of a bank, and hold information about gold and items this
    ///     character has in the bank.
    /// </summary>
    [JsonProperty("user"), JsonConverter(typeof(BankDataConverter))]
    public BankInfo? Bank { get; protected set; }

    /// <summary>
    ///     TODO: unknown
    /// </summary>
    [JsonProperty]
    public int Cache { get; protected set; }

    /// <summary>
    ///     If populated, this contains all of the code executing for this character.
    /// </summary>
    [JsonProperty]
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
    [JsonProperty("cc")]
    public float CodeCost { get; protected set; }

    /// <summary>
    ///     The number of empty slots in this character's inventory.
    /// </summary>
    [JsonProperty("esize")]
    public int EmptySlots { get; protected set; }

    /// <summary>
    ///     While you are not attacking, your range increases by 5/second, up to a max of 25.
    /// </summary>
    [JsonProperty("xrange")]
    public float ExtraRange { get; protected set; }

    /// <summary>
    ///     When the number of monsters you are attacking exceeds the courage value for that type of monster, you start to gain
    ///     fear.
    ///     <br />
    ///     This value is the number of monsters you are exceeding your courage by.
    /// </summary>
    [JsonProperty]
    public float Fear { get; protected set; }

    /// <summary>
    ///     The character's inventory.
    /// </summary>
    [JsonProperty("items")]
    public Inventory Inventory { get; internal set; } = null!;

    /// <summary>
    ///     The maximum size of this character's inventory.
    /// </summary>
    [JsonProperty("isize")]
    public int InventorySize { get; protected set; }

    /// <summary>
    ///     The number of times this character has changed maps.
    /// </summary>
    [JsonProperty("m")]
    public ulong MapChangeCount { get; protected set; }

    /// <summary>
    ///     The mp cost of the character's basic attack.
    /// </summary>
    [JsonProperty("mp_cost")]
    public new int MPCost { get; protected set; }

    /// <summary>
    ///     When you buy or sell an item, there is a fraction of that item's value you pay in taxes.
    ///     <br />
    ///     For merchants, this tax is converted into earned xp.
    /// </summary>
    [JsonProperty]
    public float Tax { get; protected set; }

    /// <summary>
    ///     The total xp required to reach the next level. (large enough to overflow <see cref="int" /> at high level)
    /// </summary>
    [JsonProperty("max_xp")]
    public long MaxXP { get; protected set; }

    /// <summary>
    ///     The character's gold-find multiplier (1 + xgold/100).
    /// </summary>
    [JsonProperty("goldm")]
    public float GoldMultiplier { get; protected set; }

    /// <summary>
    ///     The character's xp multiplier (1 + xxp/100).
    /// </summary>
    [JsonProperty("xpm")]
    public float XPMultiplier { get; protected set; }

    /// <summary>
    ///     The character's luck multiplier (1 + xluck/100).
    /// </summary>
    [JsonProperty("luckm")]
    public float LuckMultiplier { get; protected set; }

    /// <summary>
    ///     The account's premium currency (shells) balance.
    /// </summary>
    [JsonProperty("cash")]
    public int Cash { get; protected set; }

    /// <summary>
    ///     A percentage bonus applied to incoming damage (0 for most classes).
    /// </summary>
    [JsonProperty("incdmgamp")]
    public int IncomingDamageAmp { get; protected set; }

    /// <summary>
    ///     The number of magical monsters this character can face before gaining fear.
    /// </summary>
    [JsonProperty("mcourage")]
    public int MCourage { get; protected set; }

    /// <summary>
    ///     The number of pure/physical monsters this character can face before gaining fear.
    /// </summary>
    [JsonProperty("pcourage")]
    public int PCourage { get; protected set; }

    public virtual bool Equals(Character? other) => base.Equals(other);

    public override bool Equals(object? obj) => Equals(obj as Character);

    public override int GetHashCode() => base.GetHashCode();

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

    /// <summary>
    ///     System.Text.Json post-deserialize step: after the base slot fill, size the inventory to
    ///     <see cref="InventorySize" /> (bound by this point). Reproduces the Newtonsoft CharacterConverter enrich.
    /// </summary>
    /// <remarks>
    ///     A frame that carries no <c>items</c> key leaves <see cref="Inventory" /> null, so the sizing is skipped
    ///     rather than dereferencing it — there is no inventory to size, and the alternative is a
    ///     <see cref="NullReferenceException" /> that kills the whole frame.
    /// </remarks>
    public override void OnDeserialized()
    {
        base.OnDeserialized();
        Inventory?.SetCapacity(InventorySize);
    }
}