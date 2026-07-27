#region
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.Core.Interfaces;
using AL.Core.Json.SystemTextJson;
using FluentAssertions;
#endregion

// every stat accessor on TestAttributedBase exists to give the converter a binding target; the serializer is the
// only writer, so none of them is visibly assigned
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace AL.Tests.SystemTextJson.Tests;

/// <summary>
///     STJ Phase 2 mechanics check for the recursion-safe attributed-object converter. Not the full equivalence gate
///     (Phase 5, against real GItem/entity frames once the models are STJ-annotated) — this validates the converter LOGIC
///     in isolation with purpose-built STJ-annotated types: recursion-safe declared fill, the every-key presence pass,
///     numeric-ALAttribute harvest (incl. property-backed), non-numeric keys not throwing, GItem-style scroll-stat
///     recovery, and lenient int rounding on the inner path.
/// </summary>
public sealed class AttributedObjectConverterTests
{
    [Test]
    public void Entity_FillsProps_MarksPresence_HarvestsNumericAttributes()
    {
        var entity = JsonSerializer.Deserialize<TestEntity>("""{"attack":5,"armor":3,"heal":true}""", Opts())!;

        // declared members filled by the recursion-safe inner deserialize (no stack overflow)
        entity.Attack
              .Should()
              .Be(5);

        entity.Armor
              .Should()
              .Be(3);

        // every top-level wire key marked present, including the non-numeric one
        entity.PresentKeys
              .Should()
              .BeEquivalentTo("attack", "armor", "heal");

        // numeric ALAttribute keys harvested (property-backed: Attack is both a prop AND a dict entry)
        entity.Attributes
              .Should()
              .ContainKey(ALAttribute.Attack)
              .WhoseValue
              .Should()
              .Be(5);

        entity.Attributes
              .Should()
              .ContainKey(ALAttribute.Armor)
              .WhoseValue
              .Should()
              .Be(3);

        // a non-numeric ALAttribute-named key (heal=true) is skipped, never harvested, and does not throw
        entity.Attributes
              .Should()
              .NotContainKey(ALAttribute.Heal);
    }

    [Test]
    public void GItem_RecoversScrollStat_KeepsStatZero_RoundsFractionalInt()
    {
        var item = JsonSerializer.Deserialize<TestGItem>("""{"attack":5,"grade":3.6,"stat":"int"}""", Opts())!;

        // a non-numeric `stat` string is stripped before binding, so the float Stat stays 0 (no throw)
        item.Stat
            .Should()
            .Be(0);

        item.RecoveredScroll
            .Should()
            .Be(ALAttribute.Int);

        // the rest of the object still binds - stripping the bad key resumes where the pre-migration [OnError] did
        item.Attack
            .Should()
            .Be(5);

        // lenient int rounding on the inner path: 3.6 -> 4 (banker's rounding), preserved from the previous stack
        item.Grade
            .Should()
            .Be(4);

        item.Attributes
            .Should()
            .ContainKey(ALAttribute.Attack)
            .WhoseValue
            .Should()
            .Be(5);
    }

    [Test]
    public void NestedAttributed_StillHarvests()
    {
        // the inner options must keep the factory for NESTED IAttributed members (excluding only the outer type),
        // or the nested harvest is silently dropped — the Phase 2 gate's major finding.
        var container = JsonSerializer.Deserialize<TestContainer>("""{"attack":1,"child":{"armor":9}}""", Opts())!;

        container.Attributes
                 .Should()
                 .ContainKey(ALAttribute.Attack)
                 .WhoseValue
                 .Should()
                 .Be(1);

        container.Child
                 .Should()
                 .NotBeNull();

        container.Child!.Armor
                 .Should()
                 .Be(9);

        container.Child
                 .Attributes
                 .Should()
                 .ContainKey(ALAttribute.Armor)
                 .WhoseValue
                 .Should()
                 .Be(9, "the nested attributed value still harvests");
    }

    [Test]
    public void NullToken_YieldsNull()
        => JsonSerializer.Deserialize<TestEntity>("null", Opts())
                         .Should()
                         .BeNull();

    private static JsonSerializerOptions Opts()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        options.Converters.Add(new AttributedObjectConverterFactory());

        return options;
    }

    // Minimal IAttributed surface: every stat is a public init auto-prop so STJ binds any that appear on the
    // wire, and Attributes is a concrete mutable dictionary the harvest writes into.
    private abstract class TestAttributedBase : IAttributed
    {
        public float APiercing { get; init; }
        public float Armor { get; init; }
        public float Attack { get; init; }

        [JsonIgnore]
        public IReadOnlyDictionary<ALAttribute, float> Attributes { get; init; } = new Dictionary<ALAttribute, float>();

        public float Awesomeness { get; init; }
        public float Blast { get; init; }
        public float Bling { get; init; }
        public float Charisma { get; init; }
        public float Crit { get; init; }
        public float CritDamage { get; init; }
        public float Cuteness { get; init; }
        public float Dex { get; init; }
        public float DReturn { get; init; }
        public float Evasion { get; init; }
        public float Explosion { get; init; }
        public float FireResistance { get; init; }
        public float For { get; init; }
        public float FreezeResistance { get; init; }
        public float Frequency { get; init; }
        public float FrequencyMod { get; init; }
        public float Gold { get; init; }
        public float GoldSteal { get; init; }
        public float HealMod { get; init; }
        public float HP { get; init; }
        public float Int { get; init; }
        public float Lifesteal { get; init; }
        public float Luck { get; init; }
        public float ManaSteal { get; init; }
        public float Miss { get; init; }
        public float MP { get; init; }
        public float MPCost { get; init; }
        public float MPReduction { get; init; }
        public float Output { get; init; }
        public float PoisonResistance { get; init; }
        public float PotionsMod { get; init; }
        public float Range { get; init; }
        public float Reflection { get; init; }
        public float Resistance { get; init; }
        public float RPiercing { get; init; }
        public float Speed { get; init; }
        public float Stat { get; init; }
        public float Str { get; init; }
        public float StunChance { get; init; }
        public float Vit { get; init; }
        public float XP { get; init; }
    }

    private sealed class TestContainer : TestAttributedBase
    {
        public TestNested? Child { get; init; }
    }

    private sealed class TestEntity : TestAttributedBase, IKeyPresenceCapturable
    {
        public List<string> PresentKeys { get; } = new();

        public void MarkPresent(string key) => PresentKeys.Add(key);
    }

    private sealed class TestGItem : TestAttributedBase, IScrollStatRecoverable
    {
        public int Grade { get; init; }

        public ALAttribute? RecoveredScroll { get; private set; }

        public void RecoverScrollStat(string statName)
        {
            if (EnumHelper.TryParse(statName, out ALAttribute attribute))
                RecoveredScroll = attribute;
        }
    }

    private sealed class TestNested : TestAttributedBase;
}