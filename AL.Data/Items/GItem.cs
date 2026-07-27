#region
using System.Text.Json.Serialization;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.Core.Interfaces;
using AL.Data.NPCs;
using StjConverters = AL.Core.Json.SystemTextJson;
#endregion

namespace AL.Data.Items;

/// <summary>
///     <inheritdoc cref="AttributedRecordBase" />
///     <br />
///     Represents the static data for an item.
/// </summary>
/// <seealso cref="AttributedRecordBase" />
public sealed record GItem : AttributedRecordBase, IScrollStatRecoverable
{
    /// <summary>
    ///     The unique accessor for this item.
    /// </summary>
    public string Accessor { get; internal set; } = null!;

    /// <summary>
    ///     The item's price in shells if it is a cash-shop item, otherwise zero.
    /// </summary>
    /// <remarks>
    ///     A nonzero value also changes how the item is valued: the buy-to-sell discount is not applied to cash items, and the
    ///     second-hands NPC charges a 3x multiplier rather than 2x.
    /// </remarks>
    public float Cash { get; init; }

    /// <summary>
    ///     <b>
    ///         NULLABLE
    ///     </b>
    ///     . If null, this item is not compoundable. If NOT null, this dictionary contains the <see cref="ALAttribute" />
    ///     modifications per compound level.
    /// </summary>
    [JsonPropertyName("compound")]
    public IReadOnlyDictionary<ALAttribute, float>? CompoundModifiers { get; init; }

    /// <summary>
    ///     If this item is a weapon, this is the damage type of the weapon.
    /// </summary>
    [JsonPropertyName("damage_type")]
    public DamageType DamageType { get; init; }

    /// <summary>
    ///     If this item is an elixir, this is the duration of the elixir's effect in hours.
    /// </summary>
    public float? DurationHrs { get; init; }

    /// <summary>
    ///     If populated, this item can be exchanged at this NPC.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public GNPC? ExchangeAtNPC { get; internal set; }

    /// <summary>
    ///     If populated, this item can be exchanged.
    ///     <br />
    ///     This is the amount of the item that is exchanged at once.
    ///     <br />
    ///     Check <see cref="ExchangeAtNPC" /> for the npc to exchange at.
    /// </summary>
    [JsonPropertyName("e")]
    public int? ExchangeCount { get; init; }

    /// <summary>
    ///     If populated, this item gives some attribute when consumed.
    ///     <br />
    ///     These are the attributes it gives.
    /// </summary>
    public IReadOnlyList<(ALAttribute Attribute, float Amount)>? Gives { get; init; }

    /// <summary>
    ///     The default gold value of this item if selling to an NPC merchant.
    /// </summary>
    [JsonPropertyName("g")]
    public float GoldValue { get; init; }

    /// <summary>
    ///     If populated, this is the grade of the item. (scrolls, and various other items)
    /// </summary>
    public int? Grade { get; set; }

    /// <summary>
    ///     <b>
    ///         NULLABLE
    ///     </b>
    ///     . If populated, this item is compoundable or upgradeable. This list contains the levels at which the grade
    ///     increases.
    /// </summary>
    public IReadOnlyList<int>? Grades { get; init; }

    /// <summary>
    ///     If this is true, this is bad/old data that should be ignored.
    /// </summary>
    public bool Ignore { get; init; }

    /// <summary>
    ///     The name of the item.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     If populated, this item is obtainable from this NPC via a quest.
    ///     <br />
    ///     Check <see cref="ObtainableFromNPC" /> for that data.
    /// </summary>
    public string? NPC { get; init; }

    /// <summary>
    ///     If populated, this item can be obtained from this NPC.
    ///     <br />
    ///     Check <see cref="ObtainType" /> for the method of obtaining.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public GNPC? ObtainableFromNPC { get; internal set; }

    /// <summary>
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public ObtainType ObtainType { get; internal set; }

    /// <summary>
    ///     If this item is a weapon, this is the name of the projectile it Emits when attacking.
    /// </summary>
    public string? Projectile { get; init; }

    /// <summary>
    ///     If populated, this item can be exchanged at an npc with this quest.
    ///     <br />
    ///     Check <see cref="ExchangeAtNPC" /> for that data.
    /// </summary>
    public Quest? Quest { get; init; }

    /// <summary>
    ///     If populated. this item is craftable.
    ///     <br />
    ///     This is the recipe to craft this item.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public Recipe? Recipe { get; internal set; }

    /// <summary>
    ///     If this is an equipment item, this is the number of stats this item will give at level 0.
    /// </summary>
    [JsonIgnore]
    public ALAttribute ScrollStat { get; private set; }

    /// <summary>
    ///     If this is an equipment item, the set this item belongs to.
    /// </summary>
    public ArmorSet Set { get; init; }

    /// <summary>
    ///     The number of this item that can be placed in a stack.
    /// </summary>
    [JsonPropertyName("s")]
    [JsonConverter(typeof(StjConverters.FalsyStackSizeConverter))]
    public int StackSize { get; init; } = 1;

    /// <summary>
    ///     If this is an equipment item, this is the tier of the item. (higher is better)
    /// </summary>
    public float Tier { get; init; }

    /// <summary>
    ///     The type of item.
    /// </summary>
    public ItemType Type { get; init; }

    /// <summary>
    ///     <b>
    ///         NULLABLE
    ///     </b>
    ///     . If null, this item is not upgradeable. If NOT null, this dictionary contains the <see cref="ALAttribute" />
    ///     modifications per upgrade level.
    /// </summary>
    [JsonPropertyName("upgrade")]
    public IReadOnlyDictionary<ALAttribute, float>? UpgradeModifiers { get; init; }

    /// <summary>
    ///     If this item is a weapon, this is the type of weapon.
    /// </summary>
    [JsonPropertyName("wtype")]
    public WeaponType WeaponType { get; init; }

    /// <summary>
    ///     Recovers the scroll's target stat when the server sends its name in the numeric
    ///     <c>
    ///         stat
    ///     </c>
    ///     slot. The System.Text.Json attributed-object converter strips that key before binding (the float
    ///     <see cref="ALAttribute.Stat" /> attribute would otherwise abort the item) and calls this. It replaces a Newtonsoft
    ///     <c>
    ///         [OnError]
    ///     </c>
    ///     handler that let the bind throw and then scraped the stat name out of the exception message.
    /// </summary>
    public void RecoverScrollStat(string statName)
    {
        if (EnumHelper.TryParse(statName, out ALAttribute attribute))
            ScrollStat = attribute;
    }
}