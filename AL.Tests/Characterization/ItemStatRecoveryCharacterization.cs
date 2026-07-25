#region
using System.Collections.Generic;
using System.Linq;
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using AL.Data.Items;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins the <see cref="GItem" /> <c>[OnError]</c> recovery (GItem.cs:205-223). When <c>stat</c> arrives as
///     a string rather than a number the numeric bind throws; the handler resolves the string to an
///     <see cref="ALAttribute" />, sets <see cref="GItem.ScrollStat" />, and marks the error handled so
///     deserialization resumes with the remaining wire members. System.Text.Json has no <c>[OnError]</c>
///     equivalent and instead strips the offending <c>stat</c> key before binding and recovers it afterwards, so
///     every pin here runs through both engines and the Newtonsoft result stays the target.
/// </summary>
[TestClass]
public class ItemStatRecoveryCharacterization
{
    /// <summary>
    ///     The wire <c>stat</c> string of each recovering item mapped to the attribute the handler resolves it to.
    ///     Hardcoded as an independent source of truth so the assertion does not merely re-run the parser it pins.
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
    ///     Every item in the snapshot whose <c>stat</c> value is a JSON string, discovered from the raw wire so the
    ///     count is independent of any deserialization the tests also exercise.
    /// </summary>
    private static IEnumerable<(string Accessor, JObject Wire)> StringStatItems()
        => ((JObject)Fixture.Section("items")).Properties()
                                              .Where(property => property.Value is JObject wire
                                                                 && wire["stat"] is JValue { Type: JTokenType.String })
                                              .Select(property => (property.Name, (JObject)property.Value));

    /// <summary>
    ///     The item's wire JSON through both engines. Every pin below is asserted against both, which is what
    ///     proves the System.Text.Json strip-and-recover redesign reproduces <c>[OnError]</c> rather than merely
    ///     passing tests written for it.
    /// </summary>
    private static IEnumerable<(string Engine, GItem Item)> BothEngines(JObject wire)
    {
        var json = wire.ToString(Formatting.None);

        //Newtonsoft takes the converter explicitly, the way ItemsDatum's ItemConverterType applies it per entry
        yield return ("Newtonsoft", JsonConvert.DeserializeObject<GItem>(json, new AttributedObjectConverter<GItem>())!);

        //ALJson.Options resolves the same converter from the attributed factory, which is how GameData binds items
        yield return ("STJ", TestJson.Data<GItem>(json)!);
    }

    [TestMethod]
    public void T3_StringStatItems_AreExactlyTwentyTwo()
    {
        var discovered = StringStatItems()
            .Select(item => item.Accessor)
            .ToList();

        discovered.Should()
                  .HaveCount(22, "the snapshot contains exactly this many items whose stat is a string");

        // the hardcoded expectation set must cover every discovered stat string and nothing else
        discovered.Select(accessor => (string)((JObject)Fixture.Entry("items", accessor))["stat"]!)
                  .Should()
                  .BeEquivalentTo(ExpectedScrollStat.Keys);
    }

    [TestMethod]
    public void T3_StringStat_RecoversScrollStat()
    {
        foreach ((var accessor, var wire) in StringStatItems())
        {
            var statString = (string)wire["stat"]!;

            foreach ((var engine, var item) in BothEngines(wire))
                item.ScrollStat
                    .Should()
                    .Be(ExpectedScrollStat[statString], "{0}: item {1} declares stat=\"{2}\"", engine, accessor, statString);
        }
    }

    [TestMethod]
    public void T3_StringStat_LeavesNumericStatUnbound()
    {
        // the numeric Stat bind is what threw; the handler recovers ScrollStat but Stat keeps its default
        foreach ((var accessor, var wire) in StringStatItems())
            foreach ((var engine, var item) in BothEngines(wire))
                item.Stat
                    .Should()
                    .Be(0f, "{0}: item {1}'s stat was a string, so the numeric Stat property never bound", engine, accessor);
    }

    [TestMethod]
    public void T3_StringStat_ResumesBindingLaterWireMembers()
    {
        // name, s and g all follow stat in the wire object; if the handler abandoned the object instead of
        // resuming, these would hold their defaults (null / 1 / 0)
        foreach ((var accessor, var wire) in StringStatItems())
            foreach ((var engine, var item) in BothEngines(wire))
            {
                item.Name
                    .Should()
                    .Be((string)wire["name"]!, "{0}: Name follows stat in the wire and must bind after recovery for {1}", engine, accessor);

                item.GoldValue
                    .Should()
                    .Be((float)wire["g"]!, "{0}: GoldValue follows stat in the wire and must bind after recovery for {1}", engine, accessor);

                item.StackSize
                    .Should()
                    .Be((int)wire["s"]!, "{0}: StackSize follows stat in the wire and must bind after recovery for {1}", engine, accessor);
            }
    }

    [TestMethod]
    public void T3_StringStat_GradeIsNull_AbsentFromWire()
    {
        // the plan lists Grade as a resume canary, but grade is absent from all 22 scrolls' wire; it stays null.
        // pinned as-is: the real resume canaries are Name / GoldValue / StackSize (see the sibling test).
        foreach ((var accessor, var wire) in StringStatItems())
        {
            wire.ContainsKey("grade")
                .Should()
                .BeFalse("grade is absent from the wire for {0}", accessor);

            foreach ((var engine, var item) in BothEngines(wire))
                item.Grade
                    .Should()
                    .BeNull("{0}: grade is absent from the wire for {1}, so Grade cannot serve as a resume canary", engine, accessor);
        }
    }
}
