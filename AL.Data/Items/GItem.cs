#region
using System.Text.Json.Serialization;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.Core.Interfaces;
using AL.Data.Drops;
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
    ///     This item's key in the game's item table - <c>hpot0</c>, <c>firebow</c>. Filled in from that key by this
    ///     library rather than sent by the server.
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
    ///     . If null, this item is not compoundable. If NOT null, the <see cref="ALAttribute" /> gain added once per
    ///     compound level, scaled up past +4 - 1.25x at +5, 1.5x at +6, 2x at +7, 3x from +8 on.
    /// </summary>
    [JsonPropertyName("compound")]
    public IReadOnlyDictionary<ALAttribute, float>? CompoundModifiers { get; init; }

    /// <summary>
    ///     If this item is a weapon, this is the damage type of the weapon.
    /// </summary>
    [JsonPropertyName("damage_type")]
    public DamageType DamageType { get; init; }

    /// <summary>
    ///     If this item is an elixir, how long its effect lasts, in hours.
    /// </summary>
    [JsonPropertyName("duration")]
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
    ///     Check <see cref="ExchangeAtNPC" /> for the npc to exchange at, and <see cref="ExchangeRewards" /> for what
    ///     comes back - which depends on the level exchanged when the item compounds or upgrades
    ///     (node/server.js:6067).
    /// </summary>
    [JsonPropertyName("e")]
    public int? ExchangeCount { get; init; }

    /// <summary>
    ///     If populated, what an exchange of this item rolls on, keyed by the level exchanged. An item that neither
    ///     compounds nor upgrades carries a single entry at 0; one that does carries an entry per level the game has
    ///     a table for, and a level absent from it cannot be exchanged at all.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public IReadOnlyDictionary<int, IReadOnlyList<GDrop>>? ExchangeRewards { get; internal set; }

    /// <summary>
    ///     If populated, what consuming this item gives, as attribute and amount pairs. An amount can be negative,
    ///     and every amount is halved while the character is poisoned.
    /// </summary>
    public IReadOnlyList<(ALAttribute Attribute, float Amount)>? Gives { get; init; }

    /// <summary>
    ///     What an NPC charges for one of this item. Selling one back pays 60% of it - see
    ///     <see cref="AL.Data.Multipliers.GMultipliers.BuyToSell" />.
    /// </summary>
    [JsonPropertyName("g")]
    public float GoldValue { get; init; }

    /// <summary>
    ///     If populated, this item's own fixed grade - scrolls, offerings, chrysalises. A fractional wire value is
    ///     rounded, so <c>scroll4</c>'s 3.6 arrives here as 4.
    /// </summary>
    public int? Grade { get; set; }

    /// <summary>
    ///     <b>
    ///         NULLABLE
    ///     </b>
    ///     . If populated, this item is compoundable or upgradeable. Four levels: the ones at which the item's grade
    ///     steps to 1, 2, 3 and 4. When absent the server uses 9, 10, 11, 12.
    /// </summary>
    public IReadOnlyList<int>? Grades { get; init; }

    /// <summary>
    ///     If this is true, this is bad/old data that should be ignored.
    /// </summary>
    public bool Ignore { get; init; }

    /// <summary>
    ///     The item's display name, as the game shows it. <see cref="Accessor" /> is the key it is filed under.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     If populated, the key of the NPC tied to this item. The server reads it only to find the NPC on main that
    ///     redeems a token, defaulting to the item's own name plus "s".
    ///     <br />
    ///     Check <see cref="ObtainableFromNPC" /> for that NPC's data.
    /// </summary>
    public string? NPC { get; init; }

    /// <summary>
    ///     If populated, one NPC this item can be obtained from. Several NPCs may sell the same item; this library
    ///     picks one of them, preferring a seller that is actually placed on a map. The server does not choose it.
    ///     <br />
    ///     Check <see cref="ObtainType" /> for the method of obtaining.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public GNPC? ObtainableFromNPC { get; internal set; }

    /// <summary>
    ///     How <see cref="ObtainableFromNPC" /> was reached: bought from that NPC's shop, crafted from a recipe,
    ///     exchanged for tokens, or handed over for a quest. Unknown when no NPC was found.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public ObtainType ObtainType { get; internal set; }

    /// <summary>
    ///     If this item is a weapon, the key of the projectile it fires when attacking.
    /// </summary>
    public string? Projectile { get; init; }

    /// <summary>
    ///     If populated, this item can be exchanged at an npc with this quest.
    ///     <br />
    ///     Check <see cref="ExchangeAtNPC" /> for that data.
    /// </summary>
    public Quest? Quest { get; init; }

    /// <summary>
    ///     If populated, the recipe that crafts this item. Only the craft table is wired up here - an item's
    ///     dismantle recipe is not reachable from this property.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public Recipe? Recipe { get; internal set; }

    /// <summary>
    ///     If this is a stat scroll, the stat it grants. Equipment sends a number in the same <c>stat</c> slot, and
    ///     that number lands on <see cref="ALAttribute.Stat" /> instead.
    /// </summary>
    [JsonIgnore]
    public ALAttribute ScrollStat { get; private set; }

    /// <summary>
    ///     If this is an equipment item, the armor set it belongs to. Wearing more pieces of one set adds a bonus.
    /// </summary>
    public ArmorSet Set { get; init; }

    /// <summary>
    ///     Which icon this item draws, as a key into <see cref="GameData.Positions" />.
    /// </summary>
    /// <remarks>
    ///     Usually the item's own key, but a couple of dozen items borrow another's art - every candy re-release draws
    ///     the original candy - so this is not a name to guess at.
    /// </remarks>
    public string? Skin { get; init; }

    /// <summary>
    ///     The number of this item that can be placed in a stack.
    /// </summary>
    /// <remarks>
    ///     The design tables spell most stackables <c>"s":true</c>, and the converter here reads a boolean as its falsy
    ///     default of 1 - which would report "not stackable" for the majority of stackable items. That never happens,
    ///     and reading the converter alone genuinely suggests otherwise: the game data's own trailing pass rewrites
    ///     <c>s === true</c> to 9999 as <c>G</c> is built (<c>design/items.js:7441</c>, reached because the server
    ///     evals the file whole), so this property only ever receives a number or no <c>s</c> at all.
    ///     <c>Phase3IntegrationTests</c> pins that against the committed wire snapshot.
    /// </remarks>
    [JsonPropertyName("s")]
    [JsonConverter(typeof(StjConverters.FalsyStackSizeConverter))]
    public int StackSize { get; init; } = 1;

    /// <summary>
    ///     If this is an equipment item, this is the tier of the item. (higher is better)
    /// </summary>
    public float Tier { get; init; }

    /// <summary>
    ///     The item's category. For equipment this is the slot it goes in; otherwise a kind such as elixir or token.
    /// </summary>
    public ItemType Type { get; init; }

    /// <summary>
    ///     <b>
    ///         NULLABLE
    ///     </b>
    ///     . If null, this item is not upgradeable. If NOT null, the <see cref="ALAttribute" /> gain added once per
    ///     upgrade level, scaled up past +6 - 1.25x at +7, 1.5x at +8, 2x at +9, 3x at +10, 1.25x at +11 and +12.
    /// </summary>
    [JsonPropertyName("upgrade")]
    public IReadOnlyDictionary<ALAttribute, float>? UpgradeModifiers { get; init; }

    /// <summary>
    ///     If this item is a weapon, the type of weapon. <see cref="AL.Data.Classes.GClass" /> keys its lists of
    ///     wieldable weapons by this.
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