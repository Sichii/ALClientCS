#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using AL.Core.Json;
using AL.Core.Json.Converters;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StjSerializer = System.Text.Json.JsonSerializer;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins the full input-shape matrix of <see cref="TolerantStringEnumConverter" /> under Newtonsoft (T8).
///     This is the spec the System.Text.Json <c>TolerantEnumConverter</c> is written against: for every enum
///     that carries the tolerant converter, and for a fixed set of wire shapes, the exact value (or throw)
///     Newtonsoft produces today.
/// </summary>
/// <remarks>
///     The enum set is discovered by reflection over the three assemblies that declare tolerant enums, so the
///     matrix tracks the code rather than a hand list. Deserialization goes through
///     <see cref="JsonConvert.DeserializeObject(string, Type)" />, which uses the same default settings as the
///     production socket path, so these results are the ones the live client actually produces. Dictionary-key
///     position is a distinct code path (STJ resolves it through <c>ReadAsPropertyName</c>); under Newtonsoft a
///     naked <c>Dictionary&lt;TEnum,int&gt;</c> ignores the value converter entirely and uses the framework's
///     own key parser, so its tolerance differs from value position - that difference is the point of pinning it.
/// </remarks>
[TestClass]
public sealed class EnumToleranceMatrixCharacterization
{
    private const string FIXTURE_NAME = "enum-tolerance-matrix.json";

    // grep across the solution finds the tolerant converter declared only in these three Enums.cs files
    private static readonly Assembly[] TolerantEnumAssemblies =
    [
        typeof(AL.Core.Definitions.Slot).Assembly,
        typeof(AL.SocketClient.Definitions.GameResponseType).Assembly,
        typeof(AL.APIClient.Definitions.ServerId).Assembly
    ];

    // decision 5 calls out these five for key position; Condition is fully qualified because
    // AL.SocketClient.Model also declares a Condition entity type
    private static readonly Type[] DictionaryKeyEnums =
    [
        typeof(AL.Core.Definitions.Condition),
        typeof(AL.Core.Definitions.Slot),
        typeof(AL.Core.Definitions.TradeSlot),
        typeof(AL.Core.Definitions.BankPack),
        typeof(AL.Core.Definitions.WeaponType)
    ];

    private static IReadOnlyList<Type> DiscoverTolerantEnums()
    {
        var result = new List<Type>();

        foreach (var assembly in TolerantEnumAssemblies)
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsEnum)
                    continue;

                var attributes = type.GetCustomAttributes(typeof(JsonConverterAttribute), false);

                if (attributes.Length == 0)
                    continue;

                if (((JsonConverterAttribute)attributes[0]).ConverterType == typeof(TolerantStringEnumConverter))
                    result.Add(type);
            }

        return result.OrderBy(static type => type.FullName, StringComparer.Ordinal)
                     .ToList();
    }

    #region Discovery — the plan's "35" is stale

    // FINDING: MIGRATION-PLAN T8 says "35 tolerant enums". Reality is 41: 35 in AL.Core, plus 3 in
    // AL.SocketClient (GameResponseType, ALSocketMessageType, ALSocketEmitType) and 3 in AL.APIClient
    // (ServerId, ServerRegion, APIMethod). The plan counted only AL.Core.
    [TestMethod]
    public void T8_TolerantEnum_Discovery_Finds_41_Not_35()
    {
        var enums = DiscoverTolerantEnums();

        enums.Should()
             .HaveCount(41, "grep finds the tolerant converter on 35 AL.Core + 3 AL.SocketClient + 3 AL.APIClient enums");

        enums.Count(static type => type.Assembly == typeof(AL.Core.Definitions.Slot).Assembly)
             .Should()
             .Be(35);
    }

    #endregion

    #region The matrix — committed fixture is the pinned baseline

    [TestMethod]
    public void T8_ToleranceMatrix_MatchesCommittedFixture()
    {
        var generated = BuildMatrixJson();

        var committed = Fixture.ReadCommittedSnapshot(FIXTURE_NAME);

        if (committed == null)
        {
            //sidecar name, never the committed one - see AttributesCensusCharacterization for why
            var path = Fixture.WriteSnapshot($"{FIXTURE_NAME}.generated", generated);

            Assert.Fail(
                $"Committed fixture '{FIXTURE_NAME}' is missing. Generated it at {path}; copy it into "
                + "AL.Tests/Fixtures/snapshots and re-run so the build copies it back beside the test binary.");
        }

        Normalize(generated)
            .Should()
            .Be(Normalize(committed), "the pinned Newtonsoft enum-tolerance matrix must not move under the migration");
    }

    /// <summary>
    ///     The migration's proof for T8: every cell of the tolerance matrix re-run through the production
    ///     System.Text.Json options, compared against the pinned Newtonsoft outcome. Cells that agree are the
    ///     proof; cells that differ must each fall into a consciously accepted class, and the class counts are
    ///     pinned so a new kind of divergence cannot slip in behind an existing one.
    /// </summary>
    [TestMethod]
    public void T8_ToleranceMatrix_StjPath_DivergesOnlyWhereExpected()
    {
        var newtonsoft = BuildMatrix(useStj: false);
        var stj = BuildMatrix(useStj: true);

        var byClass = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);

        foreach ((var typeName, var entry) in newtonsoft.Value)
            foreach ((var cell, var expected) in entry.Cells)
                Classify(byClass, $"value {typeName}.{cell}", expected, stj.Value[typeName].Cells[cell]);

        foreach ((var typeName, var entry) in newtonsoft.DictionaryKey)
            foreach ((var cell, var expected) in entry.Cells)
                Classify(byClass, $"key {typeName}.{cell}", expected, stj.DictionaryKey[typeName].Cells[cell]);

        var unexpected = byClass.TryGetValue(UNEXPECTED, out var list) ? list : [];

        Assert.AreEqual(
            0,
            unexpected.Count,
            $"System.Text.Json diverges from the pinned Newtonsoft tolerance matrix in an unclassified way:\n{string.Join("\n", unexpected)}");

        var counts = byClass.ToDictionary(pair => pair.Key, pair => pair.Value.Count);

        CollectionAssert.AreEquivalent(
            ExpectedDivergenceCounts.ToList(),
            counts.ToList(),
            $"the accepted divergence classes moved:\n{string.Join("\n", counts.Select(pair => $"{pair.Key}={pair.Value}"))}");
    }

    private const string UNEXPECTED = "9_UNEXPECTED";

    /// <summary>
    ///     Divergence class -> how many cells fall in it. Pinned so both a new class and a change in an existing
    ///     class's population fail loudly.
    /// </summary>
    /// <remarks>
    ///     Every one of the 369 value-position cells across all 41 tolerant enums agrees exactly — including the
    ///     shapes the converter degrades rather than throws on, so no cell reaches an exception and the two
    ///     engines' exception-type names never even come into it. The whole measured divergence is the five
    ///     unknown-key cells, one per dictionary-key enum, which is exactly what the plan predicted.
    /// </remarks>
    private static readonly IReadOnlyDictionary<string, int> ExpectedDivergenceCounts = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["2_dictionaryKeyDegradesInsteadOfThrowing"] = 5
    };

    /// <summary>
    ///     Buckets one cell's Newtonsoft-vs-STJ outcome pair. Agreement is not recorded; a difference lands in
    ///     an accepted class or, failing that, in <see cref="UNEXPECTED" />.
    /// </summary>
    private static void Classify(SortedDictionary<string, List<string>> byClass, string location, string newtonsoft, string stj)
    {
        if (newtonsoft == stj)
            return;

        var bothThrow = newtonsoft.StartsWith("THROW", StringComparison.Ordinal) && stj.StartsWith("THROW", StringComparison.Ordinal);

        //both engines reject the shape and only name the exception differently - not a behavioural difference,
        //because every caller either catches everything or lets the frame die
        var key = bothThrow ? "1_throwTypeRenamed"

                  //the deliberate improvement: a naked Dictionary<TEnum,int> never consulted the value converter
                  //under Newtonsoft, so one unknown key killed the whole payload. STJ routes the key through
                  //ReadAsPropertyName and degrades it to the zero member, matching value position.
                  : newtonsoft.StartsWith("THROW", StringComparison.Ordinal) && stj.StartsWith("count=", StringComparison.Ordinal)
                      ? "2_dictionaryKeyDegradesInsteadOfThrowing"
                      : UNEXPECTED;

        if (!byClass.TryGetValue(key, out var list))
            byClass[key] = list = [];

        list.Add($"{location}: newtonsoft={newtonsoft} stj={stj}");
    }

    #endregion

    #region Spot checks — the destructive cells the plan singles out

    // TradeSlot.Trade1 = 17: Newtonsoft parses a numeric string as the underlying value, so "17" binds to
    // Trade1 rather than degrading. A wrong value here consumed real inventory once (EmitPayloadTests).
    [TestMethod]
    public void T8_TradeSlot_NumericString17_Value_BindsToTrade1()
    {
        var value = JsonConvert.DeserializeObject(@"""17""", typeof(AL.Core.Definitions.TradeSlot));

        value.Should().Be(AL.Core.Definitions.TradeSlot.Trade1);
    }

    // The whole reason the converter exists: an unrecognized value degrades to the zero member (None),
    // never throws, so the frame carrying it survives.
    [TestMethod]
    public void T8_TradeSlot_Unknown_Value_DegradesToZeroMember()
    {
        var value = JsonConvert.DeserializeObject(@"""trade99""", typeof(AL.Core.Definitions.TradeSlot));

        value.Should().Be(AL.Core.Definitions.TradeSlot.None);
    }

    // LowerCaseNamingStrategy: the server sends "mainhand"; it must still read back to Slot.MainHand.
    [TestMethod]
    public void T8_Slot_LowercaseWireName_Value_Binds()
    {
        var value = JsonConvert.DeserializeObject(@"""mainhand""", typeof(AL.Core.Definitions.Slot));

        value.Should().Be(AL.Core.Definitions.Slot.MainHand);
    }

    // [EnumMember] alias resolution in value position.
    [TestMethod]
    public void T8_WeaponType_EnumMemberAlias_Value_Binds()
    {
        var value = JsonConvert.DeserializeObject(@"""great_staff""", typeof(AL.Core.Definitions.WeaponType));

        value.Should().Be(AL.Core.Definitions.WeaponType.GreatStaff);
    }

    // Dictionary-key position is the separate path. A naked Dictionary<TEnum,int> never consults the value
    // converter, so an unknown key is NOT tolerated here - Newtonsoft throws for the whole payload. This is
    // exactly the behaviour the STJ ReadAsPropertyName override changes (degrade-to-zero), so it is pinned.
    [TestMethod]
    public void T8_TradeSlot_Unknown_DictionaryKey_Throws()
    {
        var act = () => JsonConvert.DeserializeObject<Dictionary<AL.Core.Definitions.TradeSlot, int>>(@"{""trade99"":1}");

        act.Should().Throw<JsonSerializationException>();
    }

    // The STJ half of the pin above, and the only accepted divergence in the entire matrix - the five
    // 5_unknownString key cells counted by 2_dictionaryKeyDegradesInsteadOfThrowing. TolerantEnumConverter
    // .ReadAsPropertyName runs the key through the same tolerant parse as value position, so the frame
    // survives carrying a phantom zero-member entry instead of dying. The Newtonsoft test stays the oracle.
    [TestMethod]
    public void T8_TradeSlot_Unknown_DictionaryKey_StjDegradesToZeroMember()
    {
        var dict = TestJson.Data<Dictionary<AL.Core.Definitions.TradeSlot, int>>(@"{""trade99"":1}")!;

        dict.Should().ContainKey(AL.Core.Definitions.TradeSlot.None);
        dict.Should().HaveCount(1);
    }

    // What that degrade costs, which nothing else in the suite states: unknown keys do not merely survive,
    // they COLLIDE. Every unrecognized key becomes the zero member, so N unknown keys collapse into one
    // entry, last write wins, and the caller is told nothing (the converter logs once per distinct raw key,
    // then returns default; STJ writes dictionary entries by indexer assignment, which never rejects a repeat).
    //
    // So the two dictionary-key paths now have opposite semantics and both are reachable from production:
    //   TolerantEnumKeyDictionaryConverter, on ConcurrentDictionary members (Monster.Conditions) - SKIPS the
    //     unknown key, so the count stays honest (ToleranceTests.UnknownConditionKeyIsSkippedAndKnownOnesSurvive
    //     pins count 2, not 3).
    //   a bare Dictionary<TEnum,TValue> member (Player.Slots, GClass.Mainhand/Offhand/Doublehand) - DEGRADES
    //     the key and merges, so the count holds but a value is silently overwritten.
    // ReadAsPropertyName therefore does NOT make the whole-dict converter redundant: deleting it turns skip
    // into merge on every live entity.
    [TestMethod]
    public void T8_Unknown_DictionaryKeys_StjDegradeAndMerge_LastWriteWins()
    {
        // two distinct unknown keys, one surviving entry - the second value overwrote the first
        var collided = TestJson.Data<Dictionary<AL.Core.Definitions.Slot, int>>(@"{""zzz1"":1,""zzz2"":2}")!;

        collided.Should().HaveCount(1);
        collided[AL.Core.Definitions.Slot.None].Should().Be(2);

        // a known key is untouched; the unknown one lands beside it as a phantom None
        var mixed = TestJson.Data<Dictionary<AL.Core.Definitions.Slot, int>>(@"{""mainhand"":1,""zzz"":2}")!;

        mixed.Should().HaveCount(2);
        mixed[AL.Core.Definitions.Slot.MainHand].Should().Be(1);
        mixed[AL.Core.Definitions.Slot.None].Should().Be(2);
    }

    // A known lowercase key still binds in the naked-dictionary path (case-insensitive, EnumMember honoured).
    [TestMethod]
    public void T8_Slot_LowercaseWireName_DictionaryKey_Binds()
    {
        var dict = JsonConvert.DeserializeObject<Dictionary<AL.Core.Definitions.Slot, int>>(@"{""mainhand"":1}")!;

        dict.Should().ContainKey(AL.Core.Definitions.Slot.MainHand);
        dict.Should().HaveCount(1);
    }

    #endregion

    #region Matrix construction

    private static string BuildMatrixJson() => JsonConvert.SerializeObject(BuildMatrix(useStj: false), Formatting.Indented);

    private static Matrix BuildMatrix(bool useStj)
    {
        var enums = DiscoverTolerantEnums();

        var matrix = new Matrix
        {
            EnumCount = enums.Count
        };

        foreach (var type in enums)
            matrix.Value[type.FullName!] = BuildValueEntry(type, useStj);

        foreach (var type in DictionaryKeyEnums)
            matrix.DictionaryKey[type.FullName!] = BuildKeyEntry(type, useStj);

        return matrix;
    }

    private static ValueEnumEntry BuildValueEntry(Type type, bool useStj)
    {
        var (zero, sample) = ZeroAndSample(type);
        var (aliasMember, aliasValue) = FirstAlias(type);

        var cells = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["1_exactName"] = ValueOutcome(type, JsonConvert.ToString(sample), useStj),
            ["2_lowercase"] = ValueOutcome(type, JsonConvert.ToString(sample.ToLowerInvariant()), useStj),
            ["3_UPPERCASE"] = ValueOutcome(type, JsonConvert.ToString(sample.ToUpperInvariant()), useStj),
            ["4_enumMemberAlias"] = aliasValue == null
                ? "<no-enum-member>"
                : ValueOutcome(type, JsonConvert.ToString(aliasValue), useStj),
            ["5_unknownString"] = ValueOutcome(type, JsonConvert.ToString("zzz_not_a_member_zzz"), useStj),
            ["6_numericString17"] = ValueOutcome(type, @"""17""", useStj),
            ["7_jsonNull"] = ValueOutcome(type, "null", useStj),
            ["8_fractional_1_5"] = ValueOutcome(type, "1.5", useStj),
            ["9_boolTrue"] = ValueOutcome(type, "true", useStj)
        };

        return new ValueEnumEntry
        {
            ZeroMember = zero,
            SampleMember = sample,
            AliasMember = aliasMember,
            AliasValue = aliasValue,
            Cells = cells
        };
    }

    private static KeyEnumEntry BuildKeyEntry(Type type, bool useStj)
    {
        var (_, sample) = ZeroAndSample(type);
        var (aliasMember, aliasValue) = FirstAlias(type);

        // JSON object keys are always strings, so only the string-representable shapes apply in key position
        var cells = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["1_exactName"] = KeyOutcome(type, sample, useStj),
            ["2_lowercase"] = KeyOutcome(type, sample.ToLowerInvariant(), useStj),
            ["3_UPPERCASE"] = KeyOutcome(type, sample.ToUpperInvariant(), useStj),
            ["4_enumMemberAlias"] = aliasValue == null ? "<no-enum-member>" : KeyOutcome(type, aliasValue, useStj),
            ["5_unknownString"] = KeyOutcome(type, "zzz_not_a_member_zzz", useStj),
            ["6_numericString17"] = KeyOutcome(type, "17", useStj)
        };

        return new KeyEnumEntry
        {
            AliasMember = aliasMember,
            Cells = cells
        };
    }

    private static string ValueOutcome(Type type, string json, bool useStj)
    {
        try
        {
            var value = useStj
                ? StjSerializer.Deserialize(json, type, ALJson.Options)
                : JsonConvert.DeserializeObject(json, type);

            return value == null ? "<null>" : $"{value} = {Enum.Format(type, value, "D")}";
        } catch (Exception ex)
        {
            return $"THROW {ex.GetType().Name}";
        }
    }

    private static string KeyOutcome(Type type, string rawKey, bool useStj)
    {
        var dictionaryType = typeof(Dictionary<,>).MakeGenericType(type, typeof(int));
        var json = $"{{{JsonConvert.ToString(rawKey)}:1}}";

        try
        {
            var dictionary = (IDictionary)(useStj
                ? StjSerializer.Deserialize(json, dictionaryType, ALJson.Options)!
                : JsonConvert.DeserializeObject(json, dictionaryType)!);

            var entries = dictionary.Keys
                                    .Cast<object>()
                                    .Select(key => $"{key} = {Enum.Format(type, key, "D")}")
                                    .ToList();

            return $"count={dictionary.Count} [{string.Join(", ", entries)}]";
        } catch (Exception ex)
        {
            return $"THROW {ex.GetType().Name}";
        }
    }

    // zero = the member with underlying value 0; sample = first non-zero member so "unknown -> zero" is
    // visibly distinct from "sample -> sample". Enum.Format("D") reads the decimal regardless of the
    // underlying type, so this is safe for the ulong [Flags] WeaponType.
    private static (string Zero, string Sample) ZeroAndSample(Type type)
    {
        var names = Enum.GetNames(type);
        var zero = string.Empty;
        var sample = string.Empty;

        foreach (var name in names)
        {
            var isZero = Enum.Format(type, Enum.Parse(type, name), "D") == "0";

            if (isZero && zero.Length == 0)
                zero = name;
            else if (!isZero && sample.Length == 0)
                sample = name;
        }

        if (sample.Length == 0)
            sample = names.Length > 0 ? names[0] : string.Empty;

        return (zero, sample);
    }

    private static (string? Member, string? Value) FirstAlias(Type type)
    {
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attribute = field.GetCustomAttribute<EnumMemberAttribute>();

            if (attribute?.Value != null)
                return (field.Name, attribute.Value);
        }

        return (null, null);
    }

    private static string Normalize(string value) => value.Replace("\r\n", "\n")
                                                          .Trim();

    private sealed class Matrix
    {
        public int EnumCount { get; set; }

        public SortedDictionary<string, ValueEnumEntry> Value { get; set; } = new(StringComparer.Ordinal);

        public SortedDictionary<string, KeyEnumEntry> DictionaryKey { get; set; } = new(StringComparer.Ordinal);
    }

    private sealed class ValueEnumEntry
    {
        public string ZeroMember { get; set; } = string.Empty;

        public string SampleMember { get; set; } = string.Empty;

        public string? AliasMember { get; set; }

        public string? AliasValue { get; set; }

        public SortedDictionary<string, string> Cells { get; set; } = new(StringComparer.Ordinal);
    }

    private sealed class KeyEnumEntry
    {
        public string? AliasMember { get; set; }

        public SortedDictionary<string, string> Cells { get; set; } = new(StringComparer.Ordinal);
    }

    #endregion
}
