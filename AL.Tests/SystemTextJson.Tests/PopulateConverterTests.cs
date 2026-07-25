#region
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AL.Core.Json.Attributes;
using AL.Core.Json.Interfaces;
using AL.Core.Json.SystemTextJson;
using AL.SocketClient.SocketModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJson = Newtonsoft.Json;
using NsConverters = AL.Core.Json.Converters;
#endregion

namespace AL.Tests.SystemTextJson.Tests;

/// <summary>
///     STJ Phase 2 mechanics check for the recursion-prone reusable converters (array-to-object, string-or-object,
///     falsy). Each is registered IN the options — the configuration that triggers self-recursion — so if
///     <see cref="RecursionSafeOptions" /> failed to drop the converter for the inner deserialize, these would
///     StackOverflow and crash the host (as an unguarded self-deserialize did in Phase 1) rather than fail an
///     assertion. EventAndBoss/Disappear share the pattern but register as concrete instances rather than
///     factories, so they get their own end-to-end pair at the bottom of this file.
/// </summary>
[TestClass]
public sealed class PopulateConverterTests
{
    private static JsonSerializerOptions Opts(params JsonConverter[] converters)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        foreach (var converter in converters)
            options.Converters.Add(converter);

        return options;
    }

    [TestMethod]
    public void ArrayToObject_MapsPositionalArray_ByIndex_NoRecursion()
    {
        var point = JsonSerializer.Deserialize<TestPoint>("[1.5,2.5]", Opts(new ArrayToObjectConverter<TestPoint>()))!;
        point.X.Should().Be(1.5f);
        point.Y.Should().Be(2.5f);
    }

    [TestMethod]
    public void StringOrObject_String_SetsNamedProperty_ContainsDataFalse()
    {
        var value = JsonSerializer.Deserialize<TestOptional>("\"hello\"", Opts(new StringOrObjectConverter<TestOptional>("Name")))!;
        value.Name.Should().Be("hello");
        value.ContainsData.Should().BeFalse("a bare string is the scalar shape");
    }

    [TestMethod]
    public void StringOrObject_Object_Populates_ContainsDataTrue_NoRecursion()
    {
        var value = JsonSerializer.Deserialize<TestOptional>(
            """{"name":"world","count":3}""",
            Opts(new StringOrObjectConverter<TestOptional>("Name")))!;
        value.Name.Should().Be("world");
        value.Count.Should().Be(3);
        value.ContainsData.Should().BeTrue("an object carries data");
    }

    //the SHARED options register these as FACTORIES, not concrete instances. RecursionSafeOptions removes a
    //converter by exact type, which misses a factory — so the inner fill re-resolved the factory and the object
    //bound to null (array-to-object) / recursed to a StackOverflow (string-or-object). These pin the factory path.
    [TestMethod]
    public void ArrayToObject_ViaFactory_MapsPositionalArray_NoNull()
    {
        var point = JsonSerializer.Deserialize<TestPoint>("[1.5,2.5]", Opts(new ArrayToObjectConverterFactory()))!;
        point.X.Should().Be(1.5f);
        point.Y.Should().Be(2.5f);
    }

    [TestMethod]
    public void StringOrObject_ViaFactory_Object_Populates_NoStackOverflow()
    {
        var value = JsonSerializer.Deserialize<TestOptional>(
            """{"name":"world","count":3}""",
            Opts(new NewtonsoftStringOrObjectConverterFactory()))!;
        value.Name.Should().Be("world");
        value.Count.Should().Be(3);
        value.ContainsData.Should().BeTrue();
    }

    [TestMethod]
    public void Falsy_Int_FalseAndNull_YieldDefault_ValuePassesThrough()
    {
        var options = Opts(new FalsyConverter<int>(7));
        JsonSerializer.Deserialize<int>("false", options).Should().Be(7);
        JsonSerializer.Deserialize<int>("null", options).Should().Be(7);
        JsonSerializer.Deserialize<int>("5", options).Should().Be(5);
    }

    [TestMethod]
    public void Falsy_Enum_False_Default_KnownParses_UnknownDefaults()
    {
        var options = Opts(new FalsyConverter<FEnum>(FEnum.None));
        JsonSerializer.Deserialize<FEnum>("false", options).Should().Be(FEnum.None);
        JsonSerializer.Deserialize<FEnum>("\"GREEN\"", options).Should().Be(FEnum.Green);
        JsonSerializer.Deserialize<FEnum>("\"grn\"", options).Should().Be(FEnum.Green, "the EnumMember alias resolves");
        JsonSerializer.Deserialize<FEnum>("\"zzz\"", options).Should().Be(FEnum.None, "an unknown name degrades to the default");
    }

    //EventAndBoss and Disappear are the only Populate-style converters the shared options register as concrete
    //INSTANCES, so they are the only exercise of RecursionSafeOptions' remove-by-exact-type branch — the factory
    //tests above cannot reach it. Drive them through the real SocketJson.Options rather than a hand-built set: if
    //that branch stops dropping the instance, the declared-member fill re-enters Read and StackOverflows, killing
    //the test host outright instead of failing an assertion, so no downstream test would report it either.
    [TestMethod]
    public void EventAndBoss_ViaSharedOptions_FillsFlagsAndBosses_NoStackOverflow()
    {
        var data = TestJson.Socket<EventAndBossData>(
            """{"egghunt":true,"holidayseason":true,"icegolem":{"live":true,"map":"winterland","x":808.9,"y":407.6}}""")!;

        data.EggHunt.Should().BeTrue("the declared-field fill ran under the recursion-safe options");
        data.HolidaySeason.Should().BeTrue();
        data.Valentines.Should().BeFalse("an absent flag keeps its default");

        data.BossInfo.ContainsKey("icegolem").Should().BeTrue("every remaining object-valued key is a boss");
        data.BossInfo["icegolem"].Id.Should().Be("icegolem", "the converter stamps each boss with its wire key");
        data.BossInfo["icegolem"].Live.Should().BeTrue();
    }

    [TestMethod]
    public void Disappear_ViaSharedOptions_FillsDeclaredMembersAndSpawnShape_NoStackOverflow()
    {
        var data = TestJson.Socket<DisappearData>(
            """{"id":"CeeNote","reason":"transport","to":"main","invis":true,"s":[5,10,1]}""")!;

        data.Id.Should().Be("CeeNote");
        data.ToMap.Should().Be("main", "the renamed declared member bound under the recursion-safe options");
        data.Invisible.Should().BeTrue();
        data.ToOrientation.Should().NotBeNull("the 's' special case still runs after the inner fill");
        data.ToSpawnId.Should().BeNull("an array 's' is an orientation, never a spawn id");
    }

    private sealed record TestPoint(
        [property: JsonArrayIndex(0)]
        float X,
        [property: JsonArrayIndex(1)]
        float Y);

    [NJson.JsonConverter(typeof(NsConverters.StringOrObjectConverter<TestOptional>), nameof(Name))]
    private sealed class TestOptional : IOptionalObject
    {
        public bool ContainsData { get; set; }
        public string? Name { get; set; }
        public int Count { get; set; }
    }

    private enum FEnum
    {
        None = 0,
        Red = 1,
        [EnumMember(Value = "grn")]
        Green = 2
    }
}
