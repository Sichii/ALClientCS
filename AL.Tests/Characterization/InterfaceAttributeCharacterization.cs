#region
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.APIClient.Interfaces;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Data.Conditions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     T12 — Interface-declared attribute census.
/// </summary>
/// <remarks>
///     ~70 wire names used to live only on the six item/attribute/player interfaces, not on the concrete types that
///     implement them. System.Text.Json does not inherit member attributes from interfaces, so it would read none of them;
///     Phase 1 relocated the item keys onto the concrete types for exactly that reason. These tests enumerate the surface
///     by reflection, commit it, and pin that it still binds. All six interfaces are now bare — the census records CLR
///     names only, and any attribute reappearing on one of them breaks the snapshot loudly, which is the point.
/// </remarks>
public class InterfaceAttributeCharacterization
{
    private const string CENSUS_SNAPSHOT = "interface-attribute-census.json";

    /// <summary>
    ///     The six interfaces whose members carry wire names that do not exist on the concrete types.
    /// </summary>
    private static readonly Type[] WireInterfaces =
    [
        typeof(IAttributed),
        typeof(IInventoryItem),
        typeof(ISimplePlayer),
        typeof(ITradeItem),
        typeof(ICommonItem),
        typeof(ISimpleItem)
    ];

    private static string FriendlyTypeName(Type type)
    {
        var nullableUnderlying = Nullable.GetUnderlyingType(type);

        if (nullableUnderlying is not null)
            return $"{FriendlyTypeName(nullableUnderlying)}?";

        if (!type.IsGenericType)
            return type.Name;

        var arguments = string.Join(
            ",",
            type.GetGenericArguments()
                .Select(FriendlyTypeName));
        var baseName = type.Name[..type.Name.IndexOf('`')];

        return $"{baseName}<{arguments}>";
    }

    /// <summary>
    ///     The eight custom-named attribute stats:
    ///     <c>
    ///         firesistance
    ///     </c>
    ///     ,
    ///     <c>
    ///         fzresistance
    ///     </c>
    ///     ,
    ///     <c>
    ///         mp_reduction
    ///     </c>
    ///     ,
    ///     <c>
    ///         potionsm
    ///     </c>
    ///     ,
    ///     <c>
    ///         healm
    ///     </c>
    ///     ,
    ///     <c>
    ///         frequencym
    ///     </c>
    ///     ,
    ///     <c>
    ///         pnresistance
    ///     </c>
    ///     ,
    ///     <c>
    ///         stun
    ///     </c>
    ///     . Their wire names live only on <see cref="IAttributed" />; the concrete <see cref="AttributedRecordBase" />
    ///     re-declares the properties with no attribute. Pins that they currently bind on a real attributed type.
    /// </summary>
    [Test]
    public void T12_AttributedType_InterfaceDeclaredStatNames_CurrentlyBind()
    {
        // GCondition : AttributedRecordBase — a condition entry is the real shape carrying these keys.
        const string CONDITION_DATA = @"{
   ""name"":""synthetic"",
   ""firesistance"":20,
   ""fzresistance"":21,
   ""mp_reduction"":22,
   ""potionsm"":0.5,
   ""healm"":0.25,
   ""frequencym"":0.8,
   ""pnresistance"":23,
   ""stun"":0.15
}";

        var condition = TestJson.Data<GCondition>(CONDITION_DATA);

        condition.Should()
                 .NotBeNull();

        condition.FireResistance
                 .Should()
                 .Be(20f);

        condition.FreezeResistance
                 .Should()
                 .Be(21f);

        condition.MPReduction
                 .Should()
                 .Be(22f);

        condition.PotionsMod
                 .Should()
                 .Be(0.5f);

        condition.HealMod
                 .Should()
                 .Be(0.25f);

        condition.FrequencyMod
                 .Should()
                 .Be(0.8f);

        condition.PoisonResistance
                 .Should()
                 .Be(23f);

        condition.StunChance
                 .Should()
                 .Be(0.15f);
    }

    /// <summary>
    ///     Same three modifier stats, but read from the real committed
    ///     <c>
    ///         conditions.poisoned
    ///     </c>
    ///     entry rather than a synthetic literal, so the values are genuine wire data.
    /// </summary>
    [Test]
    public void T12_AttributedType_RealPoisonedCondition_ModifierStatsBind()
    {
        var poisoned = Fixture.Entry("conditions", "poisoned");

        // Fixture navigation is plain DOM (it is not the oracle); the bind goes through the entry's raw text.
        var condition = TestJson.Data<GCondition>(poisoned.ToJsonString());

        condition.Should()
                 .NotBeNull();

        condition.PotionsMod
                 .Should()
                 .Be(0.5f);

        condition.HealMod
                 .Should()
                 .Be(0.25f);

        condition.FrequencyMod
                 .Should()
                 .Be(0.9f);
    }

    /// <summary>
    ///     Enumerates every interface-declared JSON member and pins the full surface against a committed snapshot. A member
    ///     appearing, disappearing, or acquiring a wire name breaks this loudly — that is the whole point, since STJ silently
    ///     ignores any attribute declared here.
    /// </summary>
    [Test]
    public void T12_InterfaceDeclaredWireNames_Census_MatchesCommittedSnapshot()
    {
        var lines = new List<string>();

        foreach (var wireInterface in WireInterfaces)
        {
            var properties = wireInterface.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var property in properties)
            {
                // Predicate now reads the System.Text.Json attributes, the only flavour production carries.
                // Byte-neutral: all six interfaces are bare, so every line renders as "(clr:Name)" either way.
                var jsonProperty = property.GetCustomAttribute<JsonPropertyNameAttribute>();
                var hasIgnore = property.GetCustomAttribute<JsonIgnoreAttribute>() is not null;
                var hasConverter = property.GetCustomAttribute<JsonConverterAttribute>() is not null;
                var wireName = jsonProperty?.Name ?? $"(clr:{property.Name})";

                lines.Add(
                    $"{wireInterface.Name}.{property.Name} : {FriendlyTypeName(property.PropertyType)} "
                    + $"=> {wireName}"
                    + $"{(jsonProperty is not null ? " [JsonPropertyName]" : string.Empty)}"
                    + $"{(hasIgnore ? " [JsonIgnore]" : string.Empty)}"
                    + $"{(hasConverter ? " [JsonConverter]" : string.Empty)}");
            }
        }

        lines.Sort(StringComparer.Ordinal);

        var generated = JsonSerializer.Serialize(
            lines,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

        Fixture.ShouldMatchCommittedSnapshot(generated, CENSUS_SNAPSHOT);
    }

    /// <summary>
    ///     Pins that all twelve of <see cref="SlotItem" />'s inherited wire keys currently bind:
    ///     <c>
    ///         b
    ///     </c>
    ///     ,
    ///     <c>
    ///         giveaway
    ///     </c>
    ///     ,
    ///     <c>
    ///         list
    ///     </c>
    ///     ,
    ///     <c>
    ///         rid
    ///     </c>
    ///     ,
    ///     <c>
    ///         ach
    ///     </c>
    ///     ,
    ///     <c>
    ///         stat_type
    ///     </c>
    ///     ,
    ///     <c>
    ///         acc
    ///     </c>
    ///     ,
    ///     <c>
    ///         gf
    ///     </c>
    ///     ,
    ///     <c>
    ///         l
    ///     </c>
    ///     ,
    ///     <c>
    ///         ps
    ///     </c>
    ///     ,
    ///     <c>
    ///         v
    ///     </c>
    ///     ,
    ///     <c>
    ///         q
    ///     </c>
    ///     . None of these names exists on <see cref="SlotItem" /> itself.
    /// </summary>
    [Test]
    public void T12_SlotItem_InterfaceDeclaredWireKeys_CurrentlyBind()
    {
        const string SLOT_ITEM_DATA = @"{
   ""name"":""staff"",
   ""b"":true,
   ""giveaway"":12.5,
   ""list"":[""alice"",""bob""],
   ""rid"":""TfCh"",
   ""ach"":""firehazard"",
   ""stat_type"":""vit"",
   ""acc"":0.42,
   ""gf"":""Chonk003"",
   ""l"":""l"",
   ""ps"":[""shiny"",""glitched""],
   ""v"":""2024-01-01T00:00:00Z"",
   ""q"":7
}";

        var slotItem = TestJson.Socket<SlotItem>(SLOT_ITEM_DATA);

        slotItem.Should()
                .NotBeNull();

        slotItem.Name
                .Should()
                .Be("staff");

        slotItem.Buying
                .Should()
                .BeTrue(); // b

        slotItem.GiveawayMins
                .Should()
                .Be(12.5f); // giveaway

        (slotItem.GiveawayParticipants?.ToArray()).Should()
                                                  .Equal("alice", "bob"); // list

        slotItem.Id
                .Should()
                .Be("TfCh"); // rid

        slotItem.AchievementName
                .Should()
                .Be("firehazard"); // ach

        slotItem.StatType
                .Should()
                .Be(ALAttribute.Vit); // stat_type

        slotItem.AchievementProgress
                .Should()
                .Be(0.42f); // acc

        slotItem.GiveawayFrom
                .Should()
                .Be("Chonk003"); // gf

        slotItem.LockType
                .Should()
                .Be(LockType.Locked); // l

        slotItem.PossiblePrefixes
                .ToArray()
                .Should()
                .Equal("shiny", "glitched"); // ps

        slotItem.Volatile
                .Should()
                .Be("2024-01-01T00:00:00Z"); // v

        slotItem.Quantity
                .Should()
                .Be(7); // q
    }
}