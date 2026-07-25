#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using AL.Data.Conditions;
using AL.Data.Items;
using AL.Data.Monsters;
using AL.Data.NPCs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins the single highest-risk semantic change in the STJ migration:
///     <see cref="AttributedObjectConverter{T}" /> harvests <b>every</b> numeric key that parses to an
///     <see cref="ALAttribute" /> into <see cref="IAttributed.Attributes" /> — including keys that also bind
///     to a declared property, so both <c>GItem.Attack</c> and <c>Attributes[ALAttribute.Attack]</c> hold the
///     value. The migration's <c>[JsonExtensionData]</c> replacement only sees keys with <b>no</b> matching
///     property, so the naive port silently drops every property-backed entry. This suite fixes the current
///     Newtonsoft counts, key sets and values as the target that port must reproduce.
/// </summary>
[TestClass]
public class AttributesCensusCharacterization
{
    private const string SNAPSHOT_FILE = "attributes-census.json";

    /// <summary>
    ///     Real objects with rich stat blocks spanning all four <see cref="IAttributed" /> record types. Includes
    ///     the four the plan names plus extras that carry property-less attributes (PhysicalResistance, Reflection,
    ///     RPiercing) and renamed ones (frequencym/healm/potionsm), so the census exercises both harvest paths.
    /// </summary>
    private static readonly (string Section, string Key)[] Targets =
    [
        ("conditions", "poisoned"),
        ("items", "bataxe"),
        ("items", "fireblade"),
        ("items", "harbringer"),
        ("monsters", "franky"),
        ("monsters", "goo"),
        ("monsters", "phoenix"),
        ("npcs", "citizen0")
    ];

    [TestMethod]
    public void T2_RequiredObjectsHaveExpectedAttributeCounts()
    {
        //every one of these keys also binds to a declared property, so [JsonExtensionData] would see far fewer -
        //goo drops from 7 to 0, fireblade from 3 to 1 (only Attr0 has no property). These counts are the target.
        Assert.AreEqual(3, Deserialize("items", "fireblade").Attributes.Count);
        Assert.AreEqual(7, Deserialize("monsters", "goo").Attributes.Count);
        Assert.AreEqual(2, Deserialize("npcs", "citizen0").Attributes.Count);
        Assert.AreEqual(3, Deserialize("conditions", "poisoned").Attributes.Count);
    }

    [TestMethod]
    public void T2_PropertyAndAttributeValuesAgreeWhereBothExist()
    {
        var mismatches = new List<string>();

        foreach ((var section, var key) in Targets)
        {
            var attributed = Deserialize(section, key);

            foreach ((var attribute, var dictionaryValue) in attributed.Attributes)
            {
                var property = MatchingProperty(attributed.GetType(), attribute);

                if (property == null)
                    continue;

                var propertyValue = (float)property.GetValue(attributed)!;

                if (propertyValue != dictionaryValue)
                    mismatches.Add($"{section}.{key} {attribute}: property={propertyValue} dictionary={dictionaryValue}");
            }
        }

        Assert.AreEqual(0, mismatches.Count, $"Property/dictionary disagreement: {string.Join("; ", mismatches)}");
    }

    [TestMethod]
    public void T2_AttributesCensusMatchesCommittedSnapshot()
    {
        var generated = BuildCensus(Deserialize)
            .ToString(Formatting.Indented);

        //read BEFORE writing: WriteSnapshot and ReadCommittedSnapshot resolve to the same output path, so a
        //write-then-read would defeat both the missing guard and drift detection. Only bootstrap when absent.
        var committed = Fixture.ReadCommittedSnapshot(SNAPSHOT_FILE);

        if (committed == null)
        {
            //bootstrap under a sidecar name, never the committed one: writing the committed name would leave a
            //copy the next run reads back as its own baseline, and the comparison would pass vacuously forever
            var path = Fixture.WriteSnapshot($"{SNAPSHOT_FILE}.generated", generated);

            Assert.Fail(
                $@"Committed snapshot ""{SNAPSHOT_FILE}"" is missing. A fresh copy was generated at ""{path}""; "
                + "copy it into AL.Tests/Fixtures/snapshots and commit it.");
        }

        Assert.AreEqual(
            Normalize(committed),
            Normalize(generated),
            "Attributes census drifted from the committed Newtonsoft baseline.");
    }

    /// <summary>
    ///     The migration's actual proof for this suite: the census regenerated through the production
    ///     System.Text.Json options must equal the <b>same</b> committed Newtonsoft snapshot, byte for byte.
    ///     Only the deserializer is swapped — the rendering stays on <see cref="JObject" /> so any difference
    ///     is a real value difference and never a formatting one.
    /// </summary>
    [TestMethod]
    public void T2_AttributesCensus_StjPath_ReproducesCommittedSnapshot()
    {
        var generated = BuildCensus(DeserializeStj)
            .ToString(Formatting.Indented);

        var committed = Fixture.ReadCommittedSnapshot(SNAPSHOT_FILE);

        Assert.IsNotNull(committed, $@"Committed snapshot ""{SNAPSHOT_FILE}"" is missing.");

        Assert.AreEqual(
            Normalize(committed),
            Normalize(generated),
            "the System.Text.Json attributed-object path does not reproduce the pinned Newtonsoft census");
    }

    private static JObject BuildCensus(Func<string, string, IAttributed> deserialize)
    {
        var root = new JObject();

        foreach ((var section, var key) in Targets.OrderBy(target => $"{target.Section}.{target.Key}", StringComparer.Ordinal))
        {
            var attributed = deserialize(section, key);
            var attributes = new JArray();

            foreach ((var attribute, var value) in attributed.Attributes.OrderBy(pair => pair.Key.ToString(), StringComparer.Ordinal))
            {
                var entry = new JObject
                {
                    ["attr"] = attribute.ToString(),
                    ["value"] = value
                };

                var property = MatchingProperty(attributed.GetType(), attribute);

                if (property != null)
                {
                    entry["hasProperty"] = true;
                    entry["propertyValue"] = (float)property.GetValue(attributed)!;
                } else
                    entry["hasProperty"] = false;

                attributes.Add(entry);
            }

            root[$"{section}.{key}"] = new JObject
            {
                ["count"] = attributed.Attributes.Count,
                ["attributes"] = attributes
            };
        }

        return root;
    }

    private static IAttributed Deserialize(string section, string key)
    {
        var token = Fixture.Entry(section, key);

        return section switch
        {
            "items"      => Convert<GItem>(token),
            "monsters"   => Convert<GMonster>(token),
            "npcs"       => Convert<GNPC>(token),
            "conditions" => Convert<GCondition>(token),
            _            => throw new InvalidOperationException($@"Unhandled section ""{section}"".")
        };
    }

    /// <summary>
    ///     The same entry through the production System.Text.Json options, whose
    ///     <c>AttributedObjectConverterFactory</c> claims every <see cref="IAttributed" />.
    /// </summary>
    private static IAttributed DeserializeStj(string section, string key)
    {
        var json = Fixture.Entry(section, key)
                          .ToString(Formatting.None);

        return section switch
        {
            "items"      => TestJson.Data<GItem>(json)!,
            "monsters"   => TestJson.Data<GMonster>(json)!,
            "npcs"       => TestJson.Data<GNPC>(json)!,
            "conditions" => TestJson.Data<GCondition>(json)!,
            _            => throw new InvalidOperationException($@"Unhandled section ""{section}"".")
        };
    }

    /// <summary>
    ///     Deserializes one entry through the exact converter its datum applies via
    ///     <c>[JsonObject(ItemConverterType = ...)]</c>, reproducing <c>GameData.Populate</c>'s default-serializer
    ///     path (<c>JsonConvert.DeserializeObject&lt;GameData&gt;</c> uses no custom settings) for a single object.
    /// </summary>
    private static T Convert<T>(JToken token) where T: IAttributed, new()
    {
        var serializer = new JsonSerializer();
        serializer.Converters.Add(new AttributedObjectConverter<T>());

        return token.ToObject<T>(serializer)!;
    }

    /// <summary>
    ///     The float stat property whose CLR name matches the attribute's member name case-insensitively
    ///     (Hp -> HP, Mp -> MP, MpCost -> MPCost), or null for attributes with no declared property
    ///     (PhysicalResistance, Attr0, ...).
    /// </summary>
    private static PropertyInfo? MatchingProperty(Type type, ALAttribute attribute)
    {
        var name = Enum.GetName(attribute);

        if (name == null)
            return null;

        var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

        return property?.PropertyType == typeof(float) ? property : null;
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n");
}
