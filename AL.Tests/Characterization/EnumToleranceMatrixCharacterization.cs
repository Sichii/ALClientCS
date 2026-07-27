#region
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.APIClient.Definitions;
using AL.Core.Definitions;
using AL.Core.Json.SystemTextJson;
using AL.SocketClient.Definitions;
using FluentAssertions;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins the full input-shape matrix of <see cref="TolerantEnumConverter{TEnum}" /> (T8): for every enum that carries a
///     tolerant converter, and for a fixed set of wire shapes, the exact value (or throw) the client produces. The
///     committed fixture is that matrix as it was pinned before the migration; it is frozen text and is now the only
///     oracle the System.Text.Json arm is measured against.
/// </summary>
/// <remarks>
///     The enum set is discovered by reflection over the three assemblies that declare tolerant enums, so the matrix
///     tracks the code rather than a hand list. Deserialization goes through
///     <c>
///         TestJson.Data
///     </c>
///     , i.e. the production
///     <c>
///         ALJson.Options
///     </c>
///     , so these results are the ones the live client actually produces. Dictionary-key position is a distinct code path
///     (resolved through
///     <c>
///         ReadAsPropertyName
///     </c>
///     ) whose tolerance differs from value position in the pinned baseline - a naked
///     <c>
///         Dictionary&lt;TEnum,int&gt;
///     </c>
///     never reached the value converter there, so an unknown key killed the payload. That difference is the point of
///     pinning it.
/// </remarks>
public sealed class EnumToleranceMatrixCharacterization
{
    private const string FIXTURE_NAME = "enum-tolerance-matrix.json";

    //the fixture holds the matrix POCO under its exact CLR property names with a two-space indent, which is
    //what these options reproduce - so a regenerated sidecar stays diffable against the committed file
    private static readonly JsonSerializerOptions SnapshotOptions = new()
    {
        WriteIndented = true
    };

    // grep across the solution finds the tolerant converter declared only in these three Enums.cs files
    private static readonly Assembly[] TolerantEnumAssemblies =
    [
        typeof(Slot).Assembly,
        typeof(GameResponseType).Assembly,
        typeof(ServerId).Assembly
    ];

    // decision 5 calls out these five for key position; Condition is fully qualified because
    // AL.SocketClient.Model also declares a Condition entity type
    private static readonly Type[] DictionaryKeyEnums =
    [
        typeof(Condition),
        typeof(Slot),
        typeof(TradeSlot),
        typeof(BankPack),
        typeof(WeaponType)
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

                var converterType = ((JsonConverterAttribute)attributes[0]).ConverterType;

                //LowerCaseTolerantStringEnumConverterFactory derives from the base factory, so one assignability
                //check covers both tolerant markers - and an enum carrying some other converter is not counted
                if ((converterType != null) && typeof(TolerantStringEnumConverterFactory).IsAssignableFrom(converterType))
                    result.Add(type);
            }

        return result.OrderBy(static type => type.FullName, StringComparer.Ordinal)
                     .ToList();
    }

    #region The matrix — committed fixture is the pinned baseline
    /// <summary>
    ///     The migration's proof for T8: every cell of the tolerance matrix re-run through the production System.Text.Json
    ///     options, compared against the committed fixture - the matrix as it was pinned before the migration. Cells that
    ///     agree are the proof; cells that differ must each fall into a consciously accepted class, and the class counts are
    ///     pinned so a new kind of divergence cannot slip in behind an existing one.
    /// </summary>
    [Test]
    public void T8_ToleranceMatrix_StjPath_DivergesFromPinnedFixtureOnlyWhereExpected()
    {
        var pinned = ReadPinnedMatrix();
        var stj = BuildMatrix();

        //the shape of the matrix itself is part of the pin: an enum gaining or losing the tolerant marker, or a
        //dictionary-key enum being dropped, must fail here rather than quietly shrink the measured set
        stj.EnumCount
           .Should()
           .Be(pinned.EnumCount);

        stj.Value
           .Keys
           .Should()
           .BeEquivalentTo(pinned.Value.Keys);

        stj.DictionaryKey
           .Keys
           .Should()
           .BeEquivalentTo(pinned.DictionaryKey.Keys);

        var byClass = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);

        foreach ((var typeName, var entry) in pinned.Value)
        {
            var actual = stj.Value[typeName];

            //zero/sample/alias are reflection-derived rather than engine-derived, so they must match exactly
            actual.ZeroMember
                  .Should()
                  .Be(entry.ZeroMember, typeName);

            actual.SampleMember
                  .Should()
                  .Be(entry.SampleMember, typeName);

            actual.AliasMember
                  .Should()
                  .Be(entry.AliasMember, typeName);

            actual.AliasValue
                  .Should()
                  .Be(entry.AliasValue, typeName);

            foreach ((var cell, var expected) in entry.Cells)
                Classify(
                    byClass,
                    $"value {typeName}.{cell}",
                    expected,
                    actual.Cells[cell]);
        }

        foreach ((var typeName, var entry) in pinned.DictionaryKey)
        {
            var actual = stj.DictionaryKey[typeName];

            actual.AliasMember
                  .Should()
                  .Be(entry.AliasMember, typeName);

            foreach ((var cell, var expected) in entry.Cells)
                Classify(
                    byClass,
                    $"key {typeName}.{cell}",
                    expected,
                    actual.Cells[cell]);
        }

        var unexpected = byClass.TryGetValue(UNEXPECTED, out var list) ? list : [];

        unexpected.Count
                  .Should()
                  .Be(
                      0,
                      $"System.Text.Json diverges from the pinned tolerance matrix in an unclassified way:\n{string.Join("\n", unexpected)}");

        var counts = byClass.ToDictionary(pair => pair.Key, pair => pair.Value.Count);

        counts.ToList()
              .Should()
              .BeEquivalentTo(
                  ExpectedDivergenceCounts.ToList(),
                  $"the accepted divergence classes moved:\n{string.Join("\n", counts.Select(pair => $"{pair.Key}={pair.Value}"))}");
    }

    private const string UNEXPECTED = "9_UNEXPECTED";

    /// <summary>
    ///     Divergence class -> how many cells fall in it. Pinned so both a new class and a change in an existing class's
    ///     population fail loudly.
    /// </summary>
    /// <remarks>
    ///     Every one of the 369 value-position cells across all 41 tolerant enums agrees with the pin exactly — including the
    ///     shapes the converter degrades rather than throws on, so no cell reaches an exception and exception-type names never
    ///     even come into it. The whole measured divergence is the five unknown-key cells, one per dictionary-key enum, which
    ///     is exactly what the plan predicted.
    /// </remarks>
    private static readonly IReadOnlyDictionary<string, int> ExpectedDivergenceCounts = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["2_dictionaryKeyDegradesInsteadOfThrowing"] = 5
    };

    /// <summary>
    ///     Buckets one cell's pinned-vs-STJ outcome pair. Agreement is not recorded; a difference lands in an accepted class
    ///     or, failing that, in <see cref="UNEXPECTED" />.
    /// </summary>
    private static void Classify(
        SortedDictionary<string, List<string>> byClass,
        string location,
        string pinned,
        string stj)
    {
        if (pinned == stj)
            return;

        var bothThrow = pinned.StartsWith("THROW", StringComparison.Ordinal) && stj.StartsWith("THROW", StringComparison.Ordinal);

        //the shape is rejected either way and only the exception name moved - not a behavioural difference,
        //because every caller either catches everything or lets the frame die
        var key = bothThrow
            ? "1_throwTypeRenamed"

            //the deliberate improvement: in the pinned baseline a naked Dictionary<TEnum,int> never
            //consulted the value converter, so one unknown key killed the whole payload. STJ routes the key
            //through ReadAsPropertyName and degrades it to the zero member, matching value position.
            : pinned.StartsWith("THROW", StringComparison.Ordinal) && stj.StartsWith("count=", StringComparison.Ordinal)
                ? "2_dictionaryKeyDegradesInsteadOfThrowing"
                : UNEXPECTED;

        if (!byClass.TryGetValue(key, out var list))
            byClass[key] = list = [];

        list.Add($"{location}: pinned={pinned} stj={stj}");
    }
    #endregion

    #region Spot checks — the dictionary-key cells the matrix cannot state
    // Dictionary-key position is the separate path, and the only accepted divergence in the entire matrix - the
    // five 5_unknownString key cells counted by 2_dictionaryKeyDegradesInsteadOfThrowing. The pinned baseline
    // throws for the whole payload there (the fixture's DictionaryKey 5_unknownString cells, still the oracle);
    // TolerantEnumConverter.ReadAsPropertyName runs the key through the same tolerant parse as value position,
    // so the frame survives carrying a phantom zero-member entry instead of dying.
    [Test]
    public void T8_TradeSlot_Unknown_DictionaryKey_StjDegradesToZeroMember()
    {
        var dict = TestJson.Data<Dictionary<TradeSlot, int>>(@"{""trade99"":1}")!;

        dict.Should()
            .ContainKey(TradeSlot.None);

        dict.Should()
            .HaveCount(1);
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
    [Test]
    public void T8_Unknown_DictionaryKeys_StjDegradeAndMerge_LastWriteWins()
    {
        // two distinct unknown keys, one surviving entry - the second value overwrote the first
        var collided = TestJson.Data<Dictionary<Slot, int>>(@"{""zzz1"":1,""zzz2"":2}")!;

        collided.Should()
                .HaveCount(1);

        collided[Slot.None]
            .Should()
            .Be(2);

        // a known key is untouched; the unknown one lands beside it as a phantom None
        var mixed = TestJson.Data<Dictionary<Slot, int>>(@"{""mainhand"":1,""zzz"":2}")!;

        mixed.Should()
             .HaveCount(2);

        mixed[Slot.MainHand]
            .Should()
            .Be(1);

        mixed[Slot.None]
            .Should()
            .Be(2);
    }

    // A known lowercase key still binds in the naked-dictionary path (case-insensitive, EnumMember honoured).
    [Test]
    public void T8_Slot_LowercaseWireName_DictionaryKey_Binds()
    {
        var dict = TestJson.Data<Dictionary<Slot, int>>(@"{""mainhand"":1}")!;

        dict.Should()
            .ContainKey(Slot.MainHand);

        dict.Should()
            .HaveCount(1);
    }
    #endregion

    #region Matrix construction
    /// <summary>
    ///     The committed fixture, parsed. Missing fixture writes a sidecar to copy in and fails loudly rather than measuring
    ///     nothing.
    /// </summary>
    private static Matrix ReadPinnedMatrix()
    {
        var committed = Fixture.ReadCommittedSnapshot(FIXTURE_NAME);

        if (committed == null)
        {
            //sidecar name, never the committed one - see AttributesCensusCharacterization for why
            var path = Fixture.WriteSnapshot($"{FIXTURE_NAME}.generated", BuildMatrixJson());

            committed.Should()
                     .NotBeNull(
                         $"Committed fixture '{FIXTURE_NAME}' is missing. Generated it at {path}; copy it into "
                         + "AL.Tests/Fixtures/snapshots and re-run so the build copies it back beside the test binary.");
        }

        return JsonSerializer.Deserialize<Matrix>(committed, SnapshotOptions)
               ?? throw new InvalidOperationException($"Committed fixture '{FIXTURE_NAME}' did not parse into a matrix.");
    }

    private static string BuildMatrixJson() => JsonSerializer.Serialize(BuildMatrix(), SnapshotOptions);

    private static Matrix BuildMatrix()
    {
        var enums = DiscoverTolerantEnums();

        var matrix = new Matrix
        {
            EnumCount = enums.Count
        };

        foreach (var type in enums)
            matrix.Value[type.FullName!] = BuildValueEntry(type);

        foreach (var type in DictionaryKeyEnums)
            matrix.DictionaryKey[type.FullName!] = BuildKeyEntry(type);

        return matrix;
    }

    private static ValueEnumEntry BuildValueEntry(Type type)
    {
        (var zero, var sample) = ZeroAndSample(type);
        (var aliasMember, var aliasValue) = FirstAlias(type);

        var cells = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["1_exactName"] = ValueOutcome(type, Quote(sample)),
            ["2_lowercase"] = ValueOutcome(type, Quote(sample.ToLowerInvariant())),
            ["3_UPPERCASE"] = ValueOutcome(type, Quote(sample.ToUpperInvariant())),
            ["4_enumMemberAlias"] = aliasValue == null ? "<no-enum-member>" : ValueOutcome(type, Quote(aliasValue)),
            ["5_unknownString"] = ValueOutcome(type, Quote("zzz_not_a_member_zzz")),
            ["6_numericString17"] = ValueOutcome(type, @"""17"""),
            ["7_jsonNull"] = ValueOutcome(type, "null"),
            ["8_fractional_1_5"] = ValueOutcome(type, "1.5"),
            ["9_boolTrue"] = ValueOutcome(type, "true")
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

    private static KeyEnumEntry BuildKeyEntry(Type type)
    {
        (_, var sample) = ZeroAndSample(type);
        (var aliasMember, var aliasValue) = FirstAlias(type);

        // JSON object keys are always strings, so only the string-representable shapes apply in key position
        var cells = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["1_exactName"] = KeyOutcome(type, sample),
            ["2_lowercase"] = KeyOutcome(type, sample.ToLowerInvariant()),
            ["3_UPPERCASE"] = KeyOutcome(type, sample.ToUpperInvariant()),
            ["4_enumMemberAlias"] = aliasValue == null ? "<no-enum-member>" : KeyOutcome(type, aliasValue),
            ["5_unknownString"] = KeyOutcome(type, "zzz_not_a_member_zzz"),
            ["6_numericString17"] = KeyOutcome(type, "17")
        };

        return new KeyEnumEntry
        {
            AliasMember = aliasMember,
            Cells = cells
        };
    }

    //escaping choices do not survive the round-trip back through the parser, so the cell outcome is unaffected
    //by which encoder writes the literal
    private static string Quote(string value) => JsonSerializer.Serialize(value);

    private static string ValueOutcome(Type type, string json)
    {
        try
        {
            var value = TestJson.Data(json, type);

            return value == null ? "<null>" : $"{value} = {Enum.Format(type, value, "D")}";
        } catch (Exception ex)
        {
            return $"THROW {ex.GetType().Name}";
        }
    }

    private static string KeyOutcome(Type type, string rawKey)
    {
        var dictionaryType = typeof(Dictionary<,>).MakeGenericType(type, typeof(int));
        var json = $"{{{Quote(rawKey)}:1}}";

        try
        {
            var dictionary = (IDictionary)TestJson.Data(json, dictionaryType)!;

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

            if (isZero && (zero.Length == 0))
                zero = name;
            else if (!isZero && (sample.Length == 0))
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

    private sealed class Matrix
    {
        public SortedDictionary<string, KeyEnumEntry> DictionaryKey { get; set; } = new(StringComparer.Ordinal);
        public int EnumCount { get; set; }

        public SortedDictionary<string, ValueEnumEntry> Value { get; set; } = new(StringComparer.Ordinal);
    }

    private sealed class ValueEnumEntry
    {
        public string? AliasMember { get; set; }

        public string? AliasValue { get; set; }

        public SortedDictionary<string, string> Cells { get; set; } = new(StringComparer.Ordinal);

        public string SampleMember { get; set; } = string.Empty;
        public string ZeroMember { get; set; } = string.Empty;
    }

    private sealed class KeyEnumEntry
    {
        public string? AliasMember { get; set; }

        public SortedDictionary<string, string> Cells { get; set; } = new(StringComparer.Ordinal);
    }
    #endregion
}