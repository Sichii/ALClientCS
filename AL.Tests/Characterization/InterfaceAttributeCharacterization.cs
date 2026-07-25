#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using AL.APIClient.Interfaces;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Data.Conditions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonPropertyAttribute = Newtonsoft.Json.JsonPropertyAttribute;
using StjSerializer = System.Text.Json.JsonSerializer;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     T12 — Interface-declared attribute census.
/// </summary>
/// <remarks>
///     ~70 wire names live only on the six item/attribute/player interfaces, not on the concrete types
///     that implement them. Newtonsoft's contract resolver walks interfaces, so these names bind today.
///     System.Text.Json does not inherit member attributes from interfaces, so it would read none of them.
///     These tests enumerate the surface by reflection, commit it, and pin that it currently works.
///     Phase 1 relocated the item keys onto the concrete types, so the value tests now bind through the STJ
///     facade; the reflection census itself still reads the Newtonsoft attributes, which is where they live.
/// </remarks>
[TestClass]
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

    /// <summary>
    ///     Enumerates every interface-declared JSON member and pins the full surface against a committed
    ///     snapshot. A member appearing, disappearing, or changing its wire name breaks this loudly — that
    ///     is the whole point, since STJ silently ignores every one of these.
    /// </summary>
    [TestMethod]
    public void T12_InterfaceDeclaredWireNames_Census_MatchesCommittedSnapshot()
    {
        var lines = new List<string>();

        foreach (Type wireInterface in WireInterfaces)
        {
            PropertyInfo[] properties = wireInterface.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (PropertyInfo property in properties)
            {
                // Predicate stays on the Newtonsoft attributes through Phase 5: no production model carries
                // [JsonPropertyName]/[JsonInclude] yet, and the binding modifier reads these. Byte-neutral either
                // way here (all six interfaces are bare), but flipping it early would just report nothing.
                JsonPropertyAttribute? jsonProperty = property.GetCustomAttribute<JsonPropertyAttribute>();
                bool hasIgnore = property.GetCustomAttribute<JsonIgnoreAttribute>() is not null;
                bool hasConverter = property.GetCustomAttribute<JsonConverterAttribute>() is not null;
                string wireName = jsonProperty?.PropertyName ?? $"(clr:{property.Name})";

                lines.Add(
                    $"{wireInterface.Name}.{property.Name} : {FriendlyTypeName(property.PropertyType)} "
                    + $"=> {wireName}"
                    + $"{(jsonProperty is not null ? " [JsonProperty]" : string.Empty)}"
                    + $"{(hasIgnore ? " [JsonIgnore]" : string.Empty)}"
                    + $"{(hasConverter ? " [JsonConverter]" : string.Empty)}");
            }
        }

        lines.Sort(StringComparer.Ordinal);

        string generated = StjSerializer.Serialize(
            lines,
            new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        // Read the committed copy before writing, otherwise WriteSnapshot masks a genuinely missing snapshot.
        string? committed = Fixture.ReadCommittedSnapshot(CENSUS_SNAPSHOT);

        if (committed is null)
        {
            //sidecar name, never the committed one - see AttributesCensusCharacterization for why
            string generatedPath = Fixture.WriteSnapshot($"{CENSUS_SNAPSHOT}.generated", generated);

            Assert.Fail(
                $"Committed census snapshot '{CENSUS_SNAPSHOT}' is missing. A freshly generated copy was written to "
                + $"'{generatedPath}'; copy it into AL.Tests/Fixtures/snapshots/ and commit it.");
        }

        Assert.AreEqual(
            committed,
            generated,
            "The interface-declared JSON surface changed. If intentional, update the committed census snapshot.");
    }

    /// <summary>
    ///     Independent tripwire on the count, so a snapshot regenerated on a drifted tree still trips.
    /// </summary>
    [TestMethod]
    public void T12_InterfaceDeclaredMemberCount_Is71()
    {
        int total = WireInterfaces.Sum(
            wireInterface => wireInterface
                             .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                             .Length);

        // Reality per reflection: 45 + 8 + 7 + 5 + 4 + 2. The plan's "92" counts something else; see the report.
        Assert.AreEqual(71, total);
    }

    /// <summary>
    ///     The eight custom-named attribute stats: <c>firesistance</c>, <c>fzresistance</c>,
    ///     <c>mp_reduction</c>, <c>potionsm</c>, <c>healm</c>, <c>frequencym</c>, <c>pnresistance</c>,
    ///     <c>stun</c>. Their wire names live only on <see cref="IAttributed" />; the concrete
    ///     <see cref="AttributedRecordBase" /> re-declares the properties with no attribute. Pins that they
    ///     currently bind on a real attributed type.
    /// </summary>
    [TestMethod]
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

        Assert.IsNotNull(condition);
        Assert.AreEqual(20f, condition.FireResistance);
        Assert.AreEqual(21f, condition.FreezeResistance);
        Assert.AreEqual(22f, condition.MPReduction);
        Assert.AreEqual(0.5f, condition.PotionsMod);
        Assert.AreEqual(0.25f, condition.HealMod);
        Assert.AreEqual(0.8f, condition.FrequencyMod);
        Assert.AreEqual(23f, condition.PoisonResistance);
        Assert.AreEqual(0.15f, condition.StunChance);
    }

    /// <summary>
    ///     Same three modifier stats, but read from the real committed <c>conditions.poisoned</c> entry
    ///     rather than a synthetic literal, so the values are genuine wire data.
    /// </summary>
    [TestMethod]
    public void T12_AttributedType_RealPoisonedCondition_ModifierStatsBind()
    {
        JToken poisoned = Fixture.Entry("conditions", "poisoned");

        // Fixture navigation stays on JToken (it is not the oracle); only the bind is re-pointed, via the entry's
        // raw text. The entry carries no date-shaped strings, so JObject.Parse's DateTime normalisation cannot
        // perturb the round-trip.
        var condition = TestJson.Data<GCondition>(poisoned.ToString(Formatting.None));

        Assert.IsNotNull(condition);
        Assert.AreEqual(0.5f, condition.PotionsMod);
        Assert.AreEqual(0.25f, condition.HealMod);
        Assert.AreEqual(0.8f, condition.FrequencyMod);
    }

    /// <summary>
    ///     Phase 1 relocated the item interfaces' member attributes onto the concrete types. Pins the inverse of
    ///     the pre-migration state: <see cref="SlotItem" /> now declares its OWN [JsonProperty] for every wire key
    ///     it used to inherit only through <see cref="ITradeItem" />/<see cref="IInventoryItem" /> and their bases.
    /// </summary>
    [TestMethod]
    public void T12_SlotItem_NowDeclaresItsOwnWireKeys()
    {
        PropertyInfo[] ownProperties = typeof(SlotItem).GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        // Unlike the census above, this predicate is NOT byte-neutral: SlotItem's relocated keys are declared as
        // Newtonsoft [JsonProperty], so reading [JsonPropertyName] here would miss all twelve. Phase 6 flips both.
        List<string> ownWireNames = ownProperties
                                    .Select(p => p.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName)
                                    .Where(name => name is not null)
                                    .Select(name => name!)
                                    .ToList();

        // The twelve keys that used to live only on ITradeItem/IInventoryItem/ICommonItem/ISimpleItem.
        string[] relocated = ["b", "giveaway", "list", "rid", "ach", "stat_type", "acc", "gf", "l", "ps", "v", "q"];

        foreach (string key in relocated)
            Assert.IsTrue(
                ownWireNames.Contains(key),
                $"Phase 1 must have moved the '{key}' wire key onto SlotItem itself, but it is not declared there");
    }

    /// <summary>
    ///     Pins that all twelve of <see cref="SlotItem" />'s inherited wire keys currently bind:
    ///     <c>b</c>, <c>giveaway</c>, <c>list</c>, <c>rid</c>, <c>ach</c>, <c>stat_type</c>, <c>acc</c>,
    ///     <c>gf</c>, <c>l</c>, <c>ps</c>, <c>v</c>, <c>q</c>. None of these names exists on
    ///     <see cref="SlotItem" /> itself.
    /// </summary>
    [TestMethod]
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

        Assert.IsNotNull(slotItem);
        Assert.AreEqual("staff", slotItem.Name);
        Assert.IsTrue(slotItem.Buying);                                    // b
        Assert.AreEqual(12.5f, slotItem.GiveawayMins);                     // giveaway
        CollectionAssert.AreEqual(
            new[] { "alice", "bob" },
            slotItem.GiveawayParticipants?.ToArray());                     // list
        Assert.AreEqual("TfCh", slotItem.Id);                              // rid
        Assert.AreEqual("firehazard", slotItem.AchievementName);           // ach
        Assert.AreEqual(ALAttribute.Vit, slotItem.StatType);              // stat_type
        Assert.AreEqual(0.42f, slotItem.AchievementProgress);             // acc
        Assert.AreEqual("Chonk003", slotItem.GiveawayFrom);               // gf
        Assert.AreEqual(LockType.Locked, slotItem.LockType);             // l
        CollectionAssert.AreEqual(
            new[] { "shiny", "glitched" },
            slotItem.PossiblePrefixes.ToArray());                          // ps
        Assert.AreEqual("2024-01-01T00:00:00Z", slotItem.Volatile);      // v
        Assert.AreEqual(7, slotItem.Quantity);                            // q
    }

    private static string FriendlyTypeName(Type type)
    {
        Type? nullableUnderlying = Nullable.GetUnderlyingType(type);
        if (nullableUnderlying is not null)
            return $"{FriendlyTypeName(nullableUnderlying)}?";

        if (!type.IsGenericType)
            return type.Name;

        string arguments = string.Join(",", type.GetGenericArguments().Select(FriendlyTypeName));
        string baseName = type.Name[..type.Name.IndexOf('`')];

        return $"{baseName}<{arguments}>";
    }
}
