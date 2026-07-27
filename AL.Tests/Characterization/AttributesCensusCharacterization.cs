#region
using System.Globalization;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Data.Conditions;
using AL.Data.Items;
using AL.Data.Monsters;
using AL.Data.NPCs;
using FluentAssertions;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins the single highest-risk semantic change in the STJ migration: the attributed-object converter harvests
///     <b>
///         every
///     </b>
///     numeric key that parses to an <see cref="ALAttribute" /> into <see cref="IAttributed.Attributes" /> — including
///     keys that also bind to a declared property, so both
///     <c>
///         GItem.Attack
///     </c>
///     and
///     <c>
///         Attributes[ALAttribute.Attack]
///     </c>
///     hold the value. A naive
///     <c>
///         [JsonExtensionData]
///     </c>
///     replacement only sees keys with
///     <b>
///         no
///     </b>
///     matching property and silently drops every property-backed entry. The counts, key sets and values here are the
///     frozen baseline the System.Text.Json converter must reproduce.
/// </summary>
public class AttributesCensusCharacterization
{
    private const string SNAPSHOT_FILE = "attributes-census.json";

    //reproduces Newtonsoft's Formatting.Indented for the census: 2-space indent, and no \uXXXX escaping of
    //< > & + so the rendered text stays byte-comparable against the frozen fixture
    private static readonly JsonSerializerOptions CensusOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    ///     Real objects with rich stat blocks spanning all four <see cref="IAttributed" /> record types. Includes the four the
    ///     plan names plus extras that carry property-less attributes (PhysicalResistance, Reflection, RPiercing) and renamed
    ///     ones (frequencym/healm/potionsm), so the census exercises both harvest paths.
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

    private static JsonObject BuildCensus()
    {
        var root = new JsonObject();

        foreach ((var section, var key) in Targets.OrderBy(target => $"{target.Section}.{target.Key}", StringComparer.Ordinal))
        {
            var attributed = Deserialize(section, key);
            var attributes = new JsonArray();

            foreach ((var attribute, var value) in attributed.Attributes.OrderBy(pair => pair.Key.ToString(), StringComparer.Ordinal))
            {
                var entry = new JsonObject
                {
                    ["attr"] = attribute.ToString(),
                    ["value"] = Number(value)
                };

                var property = MatchingProperty(attributed.GetType(), attribute);

                if (property != null)
                {
                    entry["hasProperty"] = true;
                    entry["propertyValue"] = Number((float)property.GetValue(attributed)!);
                } else
                    entry["hasProperty"] = false;

                attributes.Add(entry);
            }

            root[$"{section}.{key}"] = new JsonObject
            {
                ["count"] = attributed.Attributes.Count,
                ["attributes"] = attributes
            };
        }

        return root;
    }

    /// <summary>
    ///     One entry through the production System.Text.Json options, whose
    ///     <c>
    ///         AttributedObjectConverterFactory
    ///     </c>
    ///     claims every <see cref="IAttributed" />.
    /// </summary>
    private static IAttributed Deserialize(string section, string key)
    {
        var json = Fixture.Entry(section, key)
                          .ToJsonString();

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
    ///     The float stat property whose CLR name matches the attribute's member name case-insensitively (Hp -> HP, Mp -> MP,
    ///     MpCost -> MPCost), or null for attributes with no declared property (PhysicalResistance, Attr0, ...).
    /// </summary>
    private static PropertyInfo? MatchingProperty(Type type, ALAttribute attribute)
    {
        var name = Enum.GetName(attribute);

        if (name == null)
            return null;

        var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

        return property?.PropertyType == typeof(float) ? property : null;
    }

    /// <summary>
    ///     Renders a float exactly as the frozen snapshot holds it: round-trip format, plus a trailing
    ///     <c>
    ///         .0
    ///     </c>
    ///     when the text carries no decimal point or exponent (
    ///     <c>
    ///         4
    ///     </c>
    ///     ->
    ///     <c>
    ///         4.0
    ///     </c>
    ///     ). Parsed back into a node so the writer emits that text verbatim rather than applying its own shortest-round-trip
    ///     policy, which drops the
    ///     <c>
    ///         .0
    ///     </c>
    ///     and would turn every integral stat into a false diff.
    /// </summary>
    private static JsonNode Number(float value)
    {
        var text = value.ToString("R", CultureInfo.InvariantCulture);

        if (text.IndexOfAny(
                [
                    '.',
                    'E',
                    'e'
                ])
            < 0)
            text += ".0";

        return JsonNode.Parse(text)!;
    }

    /// <summary>
    ///     The migration's proof for this suite: the census built through the production System.Text.Json options must equal
    ///     the committed Newtonsoft-era snapshot, byte for byte. The rendering is held formatting-neutral by
    ///     <see cref="CensusOptions" /> and <see cref="Number" />, so any difference is a real value difference.
    /// </summary>
    /// <remarks>
    ///     A missing snapshot is a hard failure with no bootstrap: the committed text
    ///     <b>
    ///         is
    ///     </b>
    ///     the frozen oracle, and regenerating it from the engine under test would make this comparison pass vacuously
    ///     forever.
    /// </remarks>
    [Test]
    public void T2_AttributesCensusMatchesCommittedSnapshot()
    {
        var generated = BuildCensus()
            .ToJsonString(CensusOptions);

        Fixture.ShouldMatchCommittedSnapshot(generated, SNAPSHOT_FILE);
    }

    [Test]
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

                //exact: both sides bound from the same JSON token, so any difference at all is a real divergence
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (propertyValue != dictionaryValue)
                    mismatches.Add($"{section}.{key} {attribute}: property={propertyValue} dictionary={dictionaryValue}");
            }
        }

        mismatches.Count
                  .Should()
                  .Be(0, $"Property/dictionary disagreement: {string.Join("; ", mismatches)}");
    }

    [Test]
    public void T2_RequiredObjectsHaveExpectedAttributeCounts()
    {
        //every one of these keys also binds to a declared property, so [JsonExtensionData] would see far fewer -
        //goo drops from 7 to 0, fireblade from 3 to 1 (only Attr0 has no property). These counts are the target.
        Deserialize("items", "fireblade")
            .Attributes
            .Count
            .Should()
            .Be(3);

        Deserialize("monsters", "goo")
            .Attributes
            .Count
            .Should()
            .Be(7);

        Deserialize("npcs", "citizen0")
            .Attributes
            .Count
            .Should()
            .Be(2);

        Deserialize("conditions", "poisoned")
            .Attributes
            .Count
            .Should()
            .Be(3);
    }
}