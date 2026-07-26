#region
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using AL.Core.Definitions;
using AL.Data.Items;
using FluentAssertions;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins the
///     <c>
///         stat
///     </c>
///     recovery on <see cref="GItem" />. When
///     <c>
///         stat
///     </c>
///     arrives as a string rather than a number the numeric bind cannot take it; the string is resolved to an
///     <see cref="ALAttribute" />, assigned to <see cref="GItem.ScrollStat" />, and binding continues with the remaining
///     wire members. System.Text.Json strips the offending
///     <c>
///         stat
///     </c>
///     key before binding and recovers it afterwards. Every expectation below comes from the wire itself or from the
///     hardcoded <see cref="ExpectedScrollStat" /> table, so the pins hold without a second engine to compare against.
/// </summary>
public class ItemStatRecoveryCharacterization
{
    /// <summary>
    ///     The wire
    ///     <c>
    ///         stat
    ///     </c>
    ///     string of each recovering item mapped to the attribute the handler resolves it to. Hardcoded as an independent
    ///     source of truth so the assertion does not merely re-run the parser it pins.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, ALAttribute> ExpectedScrollStat = new Dictionary<string, ALAttribute>
    {
        ["str"] = ALAttribute.Str,
        ["int"] = ALAttribute.Int,
        ["dex"] = ALAttribute.Dex,
        ["vit"] = ALAttribute.Vit,
        ["for"] = ALAttribute.For,
        ["evasion"] = ALAttribute.Evasion,
        ["reflection"] = ALAttribute.Reflection,
        ["gold"] = ALAttribute.Gold,
        ["luck"] = ALAttribute.Luck,
        ["xp"] = ALAttribute.XP,
        ["armor"] = ALAttribute.Armor,
        ["resistance"] = ALAttribute.Resistance,
        ["speed"] = ALAttribute.Speed,
        ["lifesteal"] = ALAttribute.Lifesteal,
        ["manasteal"] = ALAttribute.ManaSteal,
        ["rpiercing"] = ALAttribute.RPiercing,
        ["apiercing"] = ALAttribute.APiercing,
        ["crit"] = ALAttribute.Crit,
        ["dreturn"] = ALAttribute.DReturn,
        ["frequency"] = ALAttribute.Frequency,
        ["mp_cost"] = ALAttribute.MpCost,
        ["output"] = ALAttribute.Output
    };

    /// <summary>
    ///     The item's wire JSON through
    ///     <c>
    ///         ALJson.Options
    ///     </c>
    ///     , which resolves the item converter from the attributed factory exactly as GameData binds items.
    /// </summary>
    private static GItem Deserialize(JsonObject wire) => TestJson.Data<GItem>(wire.ToJsonString())!;

    /// <summary>
    ///     Every item in the snapshot whose
    ///     <c>
    ///         stat
    ///     </c>
    ///     value is a JSON string, discovered from the raw wire so the count is independent of any deserialization the tests
    ///     also exercise.
    /// </summary>
    private static IEnumerable<(string Accessor, JsonObject Wire)> StringStatItems()
        => Fixture.Section("items")
                  .AsObject()
                  .Select(property => (Accessor: property.Key, Wire: property.Value as JsonObject))
                  .Where(item => item.Wire?["stat"]
                                     ?.GetValueKind()
                                 == JsonValueKind.String)
                  .Select(item => (item.Accessor, item.Wire!));

    [Test]
    public void T3_StringStat_GradeIsNull_AbsentFromWire()
    {
        // the plan lists Grade as a resume canary, but grade is absent from all 22 scrolls' wire; it stays null.
        // pinned as-is: the real resume canaries are Name / GoldValue / StackSize (see the sibling test).
        foreach ((var accessor, var wire) in StringStatItems())
        {
            wire.ContainsKey("grade")
                .Should()
                .BeFalse("grade is absent from the wire for {0}", accessor);

            Deserialize(wire)
                .Grade
                .Should()
                .BeNull("grade is absent from the wire for {0}, so Grade cannot serve as a resume canary", accessor);
        }
    }

    [Test]
    public void T3_StringStat_LeavesNumericStatUnbound()
    {
        // the string value never reaches the numeric Stat bind; recovery fills ScrollStat and Stat keeps its default
        foreach ((var accessor, var wire) in StringStatItems())
            Deserialize(wire)
                .Stat
                .Should()
                .Be(0f, "item {0}'s stat was a string, so the numeric Stat property never bound", accessor);
    }

    [Test]
    public void T3_StringStat_RecoversScrollStat()
    {
        foreach ((var accessor, var wire) in StringStatItems())
        {
            var statString = wire["stat"]!.GetValue<string>();

            Deserialize(wire)
                .ScrollStat
                .Should()
                .Be(
                    ExpectedScrollStat[statString],
                    "item {0} declares stat=\"{1}\"",
                    accessor,
                    statString);
        }
    }

    [Test]
    public void T3_StringStat_ResumesBindingLaterWireMembers()
    {
        // name, s and g all follow stat in the wire object; if recovery abandoned the object instead of
        // binding the rest, these would hold their defaults (null / 1 / 0)
        foreach ((var accessor, var wire) in StringStatItems())
        {
            var item = Deserialize(wire);

            item.Name
                .Should()
                .Be(wire["name"]!.GetValue<string>(), "Name follows stat in the wire and must bind after recovery for {0}", accessor);

            item.GoldValue
                .Should()
                .Be(wire["g"]!.GetValue<float>(), "GoldValue follows stat in the wire and must bind after recovery for {0}", accessor);

            item.StackSize
                .Should()
                .Be(wire["s"]!.GetValue<int>(), "StackSize follows stat in the wire and must bind after recovery for {0}", accessor);
        }
    }

    [Test]
    public void T3_StringStatItems_AreExactlyTwentyTwo()
    {
        var discovered = StringStatItems()
                         .Select(item => item.Accessor)
                         .ToList();

        discovered.Should()
                  .HaveCount(22, "the snapshot contains exactly this many items whose stat is a string");

        // the hardcoded expectation set must cover every discovered stat string and nothing else
        discovered.Select(accessor => Fixture.Entry("items", accessor)["stat"]!.GetValue<string>())
                  .Should()
                  .BeEquivalentTo(ExpectedScrollStat.Keys);
    }
}