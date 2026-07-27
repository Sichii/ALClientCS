#region
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.SocketClient.Interfaces;
using StjConverters = AL.Core.Json.SystemTextJson;
using IJsonOnDeserialized = System.Text.Json.Serialization.IJsonOnDeserialized;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents a player. (ymyself or others)
/// </summary>
/// <seealso cref="EntityBase" />
/// <seealso cref="ISimplePlayer" />
/// <seealso cref="IEquatable{T}" />
public class Player : EntityBase, ISimplePlayer, IEquatable<Player>, IJsonOnDeserialized
{
    [JsonInclude]
    [JsonConverter(typeof(StjConverters.AfkConverter))]
    public bool AFK { get; protected set; }

    [JsonInclude]
    public int Age { get; protected set; }

    /// <summary>
    ///     <b>
    ///         NULLABLE.
    ///     </b>
    ///     If populated, you are channeling.
    ///     <br />
    ///     Some channeling abilities will be canceled if you move. Check the skill's
    ///     <c>
    ///         CanMove
    ///     </c>
    ///     property.
    /// </summary>
    [JsonPropertyName("c")]
    [JsonInclude]
    public IReadOnlyDictionary<string, ChannelingInfo>? Channeling { get; protected set; }

    /// <summary>
    ///     The class of the player.
    /// </summary>
    [JsonPropertyName("ctype")]
    [JsonInclude]
    public ALClass Class { get; protected set; }

    /// <summary>
    ///     Whether or not the player is running code.
    /// </summary>
    [JsonInclude]
    public bool Code { get; protected set; }

    /// <summary>
    ///     If populated, this character is using the steam client's frame-in-frame multi logging.
    ///     <br />
    ///     This is the name of the character who's iframe is hosting this character.
    /// </summary>
    [JsonInclude]
    public string? Controller { get; protected set; }

    /// <summary>
    ///     Appearanc information about the character.
    /// </summary>
    [JsonPropertyName("cx")]
    [JsonInclude]
    public CosmeticInfo Cosmetics { get; protected set; } = null!;

    /// <summary>
    ///     If populated, this player is actually an NPC.
    ///     <br />
    ///     You can also check <see cref="IsNPC" />.
    /// </summary>
    [JsonPropertyName("npc")]
    [JsonInclude]
    public string? NPCName { get; protected set; }

    /// <summary>
    ///     If populated, this player is an actual person.
    ///     <br />
    ///     This is the ID of the account this player belongs to. This
    /// </summary>
    [JsonInclude]
    public string? Owner { get; protected set; }

    [JsonPropertyName("party")]
    [JsonInclude]
    public string? PartyLeader { get; protected set; }

    /// <summary>
    ///     This is a value indicating the overall contribution a player is making towards his party. (higher is better)
    /// </summary>
    [JsonInclude]
    public float PDPS { get; protected set; }

    /// <summary>
    ///     <b>
    ///         NULLABLE.
    ///     </b>
    ///     If populated, this player is performing a queued action.
    ///     <br />
    ///     Queued actions are actions that take some time to complete. This object lets you keep track of their progress.
    /// </summary>
    [JsonPropertyName("q")]
    [JsonInclude]
    public QueuedActionInfo? QueuedActions { get; protected set; }

    /// <summary>
    ///     Whether or not you are dead.
    /// </summary>
    /// <remarks>
    ///     The server sends
    ///     <c>
    ///         true
    ///     </c>
    ///     or, once a gravestone cosmetic is chosen, the cosmetic's name - which is the same true-or-string shape AFK has, so
    ///     it reuses that converter. The cosmetic name itself is of no use to a headless client.
    /// </remarks>
    [JsonInclude]
    [JsonConverter(typeof(StjConverters.AfkConverter))]
    public bool RIP { get; protected set; }

    /// <summary>
    ///     If populated, this is the skin applied to the character. (appearance stuff)
    /// </summary>
    [JsonInclude]
    public string? Skin { get; protected set; }

    /// <summary>
    ///     A collection of equipment the player is wearing, and items they are selling/buying.
    ///     <br />
    ///     All slots should have keys in the dictionary, but the items may be null.
    /// </summary>
    /// <remarks>
    ///     Enriched by converter
    /// </remarks>
    [JsonInclude]
    public IReadOnlyDictionary<Slot, SlotItem?> Slots { get; protected set; } = new Dictionary<Slot, SlotItem?>();

    /// <summary>
    ///     The type of stand this player is using.
    /// </summary>
    [JsonInclude]
    public Stand Stand { get; protected set; }

    /// <summary>
    ///     Whether or not this player is currently teleporting.
    /// </summary>
    [JsonPropertyName("tp")]
    [JsonInclude]
    public bool Teleporting { get; protected set; }

    /// <summary>
    ///     Checks if this player is actually an NPC.
    /// </summary>
    [JsonIgnore]
    public bool IsNPC => (NPCName != null) || (Class == ALClass.NPC);

    /// <summary>
    ///     The name of the player.
    /// </summary>
    [JsonIgnore]
    public string Name => Id;

    public virtual bool Equals(Player? other) => other is not null && base.Equals(other);

    /// <summary>
    ///     System.Text.Json post-deserialize step: the wire only carries the equipment slots that are populated, so fill every
    ///     missing <see cref="Slot" /> with null. This reproduces the Newtonsoft PlayerConverter enrich; unlike that converter
    ///     it also runs for a bare <see cref="Player" />.
    /// </summary>
    public virtual void OnDeserialized()
    {
        var playerSlots = (Dictionary<Slot, SlotItem?>)Slots;

        foreach (var slot in Enum.GetValues<Slot>())
            playerSlots.TryAdd(slot, null);
    }

    public override bool Equals(object? obj) => Equals(obj as Player);

    public override int GetHashCode() => base.GetHashCode();

    public void Update(QueuedActionInfo queuedActionInfo) => QueuedActions = queuedActionInfo;

    public void Update(Player other)
    {
        if (Id != other.Id)
            throw new InvalidOperationException($"Attempting to update player with ID: {Id}, with data for entity with ID: {other.Id}");

        Channeling = other.Channeling;
        Cosmetics = other.Cosmetics;
        QueuedActions = other.QueuedActions;
        Skin = other.Skin;

        if (!IsNPC)
        {
            Range = other.Range;
            AFK = other.AFK;
            Age = other.Age;
            Code = other.Code;
            PDPS = other.PDPS;
            RIP = other.RIP;
            Slots = other.Slots;
            Stand = other.Stand;
        }

        base.Update(other);
    }
}