#region
using System.Text.Json;
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
    ///     If populated, the named effect this item grants while it is worn, such as <c>burn</c>, <c>freeze</c> or
    ///     <c>secondchance</c>.
    /// </summary>
    /// <remarks>
    ///     The item's <c>attr0</c> and <c>attr1</c> are summed across every worn piece naming the ability, and what they mean
    ///     is the ability's own rule.
    /// </remarks>
    public string? Ability { get; init; }

    /// <summary>
    ///     This item's key in the game's item table, such as <c>hpot0</c>. Filled in from that key rather than sent by the
    ///     server.
    /// </summary>
    public string Accessor { get; internal set; } = null!;

    /// <summary>
    ///     If populated, the condition this item holds on every player within 200 of its wearer, as a key into
    ///     <see cref="GameData.Conditions" />. In PvP it reaches only the wearer's own side.
    /// </summary>
    /// <remarks>
    ///     The item's <c>attr0</c> is how much of the condition's one stat the aura hands out: <c>sanguine</c> gives that
    ///     much lifesteal.
    /// </remarks>
    public string? Aura { get; init; }

    /// <summary>
    ///     The stats this item adds on top of its own for one class or on one map, keyed by the class's or the map's key, such
    ///     as <c>rogue</c> or <c>cave</c>. Empty for most items.
    /// </summary>
    /// <remarks>
    ///     The server adds a match for the wearer's class and map into the item before any other stat is worked out.
    /// </remarks>
    [JsonIgnore]
    public IReadOnlyDictionary<string, GItemBonus> Bonuses { get; internal set; } = new Dictionary<string, GItemBonus>();

    /// <summary>
    ///     The item's price in shells if it is a cash-shop item, otherwise zero.
    /// </summary>
    /// <remarks>
    ///     The buy-to-sell discount is not applied to cash items, and the second-hands NPC charges 3x for them rather than 2x.
    /// </remarks>
    public float Cash { get; init; }

    /// <summary>
    ///     The classes allowed to equip this item, or null where any class may.
    /// </summary>
    /// <remarks>
    ///     Several classes may disagree on main stat: <c>fury</c> names four whose <c>MainStat</c> differ.
    /// </remarks>
    [JsonPropertyName("class")]
    public IReadOnlyList<ALClass>? Classes { get; init; }

    /// <summary>
    ///     <b>NULLABLE</b>. If null, this item is not compoundable. If NOT null, the <see cref="ALAttribute" /> gain added
    ///     once per compound level, scaled up past +4: 1.25x at +5, 1.5x at +6, 2x at +7, 3x from +8 on.
    /// </summary>
    [JsonPropertyName("compound")]
    public IReadOnlyDictionary<ALAttribute, float>? CompoundModifiers { get; init; }

    /// <summary>
    ///     If this item is a weapon, this is the damage type of the weapon.
    /// </summary>
    [JsonPropertyName("damage_type")]
    public DamageType DamageType { get; init; }

    /// <summary>
    ///     If this item is a booster, the duration an inactive copy contributes when compounded, in days.
    ///     Activation itself uses a flat 30 days plus two per level instead.
    /// </summary>
    public int? Days { get; init; }

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
    ///     Check <see cref="ExchangeAtNPC" /> for the npc to exchange at, and <see cref="ExchangeRewards" /> for what comes
    ///     back.
    /// </summary>
    [JsonPropertyName("e")]
    public int? ExchangeCount { get; init; }

    /// <summary>
    ///     If populated, what an exchange of this item rolls on, keyed by the level exchanged. An item that neither compounds
    ///     nor upgrades has a single entry at 0, and a level absent from the table cannot be exchanged.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public IReadOnlyDictionary<int, IReadOnlyList<GDrop>>? ExchangeRewards { get; internal set; }

    /// <summary>
    ///     If populated, the line the game writes under this item's name in its own tooltip, such as "Rains fire upon the
    ///     enemy".
    /// </summary>
    /// <remarks>
    ///     Written for a player; the server parses nothing here, so it is not a source of figures.
    /// </remarks>
    public string? Explanation { get; init; }

    /// <summary>
    ///     If populated, what consuming this item gives, as attribute and amount pairs. An amount can be negative, and every
    ///     amount is halved while the character is poisoned.
    /// </summary>
    public IReadOnlyList<(ALAttribute Attribute, float Amount)>? Gives { get; init; }

    /// <summary>
    ///     What an NPC charges for one of this item. Selling one back pays the
    ///     <see cref="AL.Data.Multipliers.GMultipliers.BuyToSell" /> fraction of it.
    /// </summary>
    [JsonPropertyName("g")]
    public float GoldValue { get; init; }

    /// <summary>
    ///     If populated, this item's own fixed grade, as scrolls, offerings and chrysalises carry. A fractional wire value is
    ///     rounded.
    /// </summary>
    public int? Grade { get; set; }

    /// <summary>
    ///     <b>NULLABLE</b>. If populated, this item is compoundable or upgradeable. Four levels: the ones at which the item's
    ///     grade steps to 1, 2, 3 and 4. When absent the server uses 9, 10, 11, 12.
    /// </summary>
    public IReadOnlyList<int>? Grades { get; init; }

    /// <summary>
    ///     If this is true, this is bad/old data that should be ignored.
    /// </summary>
    public bool Ignore { get; init; }

    /// <summary>
    ///     If populated, the key of the NPC tied to this item. The server reads it only to find the NPC on main that redeems a
    ///     token, defaulting to the item's own name plus <c>s</c>.
    ///     <br />
    ///     Check <see cref="ObtainableFromNPC" /> for that NPC's data.
    /// </summary>
    public string? NPC { get; init; }

    /// <summary>
    ///     The item's display name, as the game shows it. <see cref="Accessor" /> is the key it is filed under.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     How <see cref="ObtainableFromNPC" /> was reached: bought from that NPC's shop, crafted from a recipe, exchanged for
    ///     tokens, or handed over for a quest. Unknown when no NPC was found.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public ObtainType ObtainType { get; internal set; }

    /// <summary>
    ///     If populated, one NPC this item can be obtained from. Where several sell it, one placed on a map is preferred.
    ///     <br />
    ///     Check <see cref="ObtainType" /> for the method of obtaining.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public GNPC? ObtainableFromNPC { get; internal set; }

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
    ///     If populated, the recipe that crafts this item. The item's dismantle recipe is not reachable from here.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public Recipe? Recipe { get; internal set; }

    /// <summary>
    ///     If this is a stat scroll, the stat it grants. Equipment sends a number in the same <c>stat</c> slot, and that
    ///     number lands on <see cref="ALAttribute.Stat" /> instead.
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
    ///     Usually the item's own key, but some items borrow another's art, such as every candy re-release.
    /// </remarks>
    public string? Skin { get; init; }

    /// <summary>
    ///     The number of this item that can be placed in a stack.
    /// </summary>
    [JsonPropertyName("s")]
    [JsonConverter(typeof(StjConverters.FalsyStackSizeConverter))]
    public int StackSize { get; init; } = 1;

    /// <summary>
    ///     Whether this item can be thrown at a spot on the ground, consuming one.
    /// </summary>
    /// <remarks>
    ///     Not derived from <see cref="Type" />: <c>whiteegg</c> is a <see cref="ItemType.Material" /> that throws, and
    ///     <c>snowball</c> is an <see cref="ItemType.Throw" /> that does not.
    /// </remarks>
    [JsonPropertyName("throw")]
    public bool Throw { get; init; }

    /// <summary>
    ///     If this is an equipment item, this is the tier of the item. (higher is better)
    /// </summary>
    public float Tier { get; init; }

    /// <summary>
    ///     The item's category. For equipment this is the slot it goes in; otherwise a kind such as elixir or token.
    /// </summary>
    public ItemType Type { get; init; }

    /// <summary>
    ///     <b>NULLABLE</b>. If null, this item is not upgradeable. If NOT null, the <see cref="ALAttribute" /> gain added
    ///     once per upgrade level, scaled up past +6: 1.25x at +7, 1.5x at +8, 2x at +9, 3x at +10, 1.25x at +11 and +12.
    /// </summary>
    [JsonPropertyName("upgrade")]
    public IReadOnlyDictionary<ALAttribute, float>? UpgradeModifiers { get; init; }

    /// <summary>
    ///     If this item is a weapon, the type of weapon. <see cref="AL.Data.Classes.GClass" /> keys its lists of wieldable
    ///     weapons by this.
    /// </summary>
    [JsonPropertyName("wtype")]
    public WeaponType WeaponType { get; init; }

    /// <summary>
    ///     Every key no declared member binds, as it arrived. <see cref="Bonuses" /> is built from it, since the wire files a
    ///     bonus under the class's or the map's own name.
    /// </summary>
    /// <remarks>
    ///     Cleared to null by <see cref="GameData.EnrichItems" /> once <see cref="Bonuses" /> is built.
    /// </remarks>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? WireExtras { get; set; }

    /// <summary>
    ///     Sets <see cref="ScrollStat" /> from the stat name a scroll sends in the numeric <c>stat</c> slot.
    /// </summary>
    /// <param name="statName">
    ///     The stat name as sent. A name that is not an <see cref="ALAttribute" /> is ignored.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     statName
    /// </exception>
    public void RecoverScrollStat(string statName)
    {
        ArgumentNullException.ThrowIfNull(statName);

        if (EnumHelper.TryParse(statName, out ALAttribute attribute))
            ScrollStat = attribute;
    }
}