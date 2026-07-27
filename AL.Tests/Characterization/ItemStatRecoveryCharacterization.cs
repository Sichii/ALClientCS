#region
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
    ///     Every item in the snapshot whose
    ///     <c>
    ///         stat
    ///     </c>
    ///     value is a JSON string, discovered from the raw wire so the count is independent of any deserialization the tests
    ///     also exercise, paired with the <see cref="GItem" /> the converter binds it to. Lazy and shared: every test below
    ///     reads the same 22 items, and deserializing them once is what keeps that from being five passes over the section.
    /// </summary>
    private static readonly Lazy<IReadOnlyList<ScrollItem>> StringStatItems = new(Collect);

    private static IReadOnlyList<ScrollItem> Collect()
    {
        var items = new List<ScrollItem>();

        var section = Fixture.Section("items")
                             .AsObject();

        foreach ((var accessor, var value) in section)
        {
            if (value is not JsonObject wire)
                continue;

            if (wire["stat"]
                    ?.GetValueKind()
                != JsonValueKind.String)
                continue;

            //through ALJson.Options, which resolves the item converter from the attributed factory exactly as
            //GameData binds items
            items.Add(new ScrollItem(accessor, wire, TestJson.Data<GItem>(wire.ToJsonString())!));
        }

        return items;
    }

    [Test]
    public void T3_StringStat_GradeIsNull_AbsentFromWire()
    {
        // the plan lists Grade as a resume canary, but grade is absent from all 22 scrolls' wire; it stays null.
        // pinned as-is: the real resume canaries are Name / GoldValue / StackSize (see the sibling test).
        foreach ((var accessor, var wire, var item) in StringStatItems.Value)
        {
            wire.ContainsKey("grade")
                .Should()
                .BeFalse("grade is absent from the wire for {0}", accessor);

            item.Grade
                .Should()
                .BeNull("grade is absent from the wire for {0}, so Grade cannot serve as a resume canary", accessor);
        }
    }

    [Test]
    public void T3_StringStat_LeavesNumericStatUnbound()
    {
        // the string value never reaches the numeric Stat bind; recovery fills ScrollStat and Stat keeps its default
        foreach ((var accessor, _, var item) in StringStatItems.Value)
            item.Stat
                .Should()
                .Be(0f, "item {0}'s stat was a string, so the numeric Stat property never bound", accessor);
    }

    [Test]
    public void T3_StringStat_RecoversScrollStat()
    {
        foreach ((var accessor, var wire, var item) in StringStatItems.Value)
        {
            var statString = wire["stat"]!.GetValue<string>();

            item.ScrollStat
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
        foreach ((var accessor, var wire, var item) in StringStatItems.Value)
        {
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
        StringStatItems.Value
                       .Should()
                       .HaveCount(22, "the snapshot contains exactly this many items whose stat is a string");

        // the hardcoded expectation set must cover every discovered stat string and nothing else
        StringStatItems.Value
                       .Select(scroll => scroll.Wire["stat"]!.GetValue<string>())
                       .Should()
                       .BeEquivalentTo(ExpectedScrollStat.Keys);
    }

    private sealed record ScrollItem(string Accessor, JsonObject Wire, GItem Item);
}