#region
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.Core.Geometry;
using AL.Core.Json.SystemTextJson;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.SystemTextJson.Tests;

/// <summary>
///     STJ Phase 1 self-check for the ported leaf converters. Not the full equivalence gate (that is Phase 5,
///     against the committed Newtonsoft snapshots) — this pins the highest-risk behaviours now: the tolerant
///     enum factory + nullable-enum path, degrade-to-zero, lowercase emit, tuple truncation, and falsy/array
///     handling, so a port regression fails here rather than four phases later.
/// </summary>
[TestClass]
public sealed class LeafConverterTests
{
    private static JsonSerializerOptions Opts(params JsonConverter[] converters)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        foreach (var converter in converters)
            options.Converters.Add(converter);

        return options;
    }

    [TestMethod]
    public void TolerantEnum_KnownAlias_And_CaseInsensitive_Parse()
    {
        JsonSerializer.Deserialize<TColor>("\"grn\"", Opts()).Should().Be(TColor.Green);
        JsonSerializer.Deserialize<TColor>("\"RED\"", Opts()).Should().Be(TColor.Red);
    }

    [TestMethod]
    public void TolerantEnum_Unknown_DegradesToZero()
        => JsonSerializer.Deserialize<TColor>("\"nope\"", Opts()).Should().Be(TColor.None);

    [TestMethod]
    public void TolerantEnum_Nullable_Null_StaysNull()
        => JsonSerializer.Deserialize<Holder>("""{"c":null}""", Opts())!.Color.Should().BeNull();

    [TestMethod]
    public void TolerantEnum_Nullable_Unknown_DegradesToZero()
        => JsonSerializer.Deserialize<Holder>("""{"c":"nope"}""", Opts())!.Color.Should().Be(TColor.None);

    [TestMethod]
    public void LowerCaseTolerantEnum_RoundTrips_Lowercase()
    {
        JsonSerializer.Deserialize<TSlot>("\"mainhand\"", Opts()).Should().Be(TSlot.MainHand);
        JsonSerializer.Serialize(TSlot.MainHand, Opts()).Should().Be("\"mainhand\"");
    }

    //the converter's T1?,T2? are unconstrained, so at runtime it handles ValueTuple<string,int> (the ? on an
    //unconstrained value-type arg erases to the non-nullable type) — deserialize the same closed type it declares.
    [TestMethod]
    public void Tuple_ShortArray_FillsPrefix_RestDefault()
    {
        var (a, b) = JsonSerializer.Deserialize<(string, int)>("""["only"]""", Opts(new ArrayToTupleConverter<string, int>()));
        a.Should().Be("only");
        b.Should().Be(0);
    }

    [TestMethod]
    public void Tuple_FullArray_FillsAll()
    {
        var (a, b) = JsonSerializer.Deserialize<(string, int)>("""["x",5]""", Opts(new ArrayToTupleConverter<string, int>()));
        a.Should().Be("x");
        b.Should().Be(5);
    }

    [TestMethod]
    public void Tuple_OverLengthArray_YieldsDefault()
    {
        var (a, b) = JsonSerializer.Deserialize<(string, int)>("""["x",1,2]""", Opts(new ArrayToTupleConverter<string, int>()));
        a.Should().BeNull("an over-length array falls to the _ => default arm, matching Newtonsoft");
        b.Should().Be(0);
    }

    [TestMethod]
    public void Point_FromArray()
    {
        var point = JsonSerializer.Deserialize<Point>("[3,4]", Opts(new ArrayToPointConverter()));
        point.X.Should().Be(3);
        point.Y.Should().Be(4);
    }

    [TestMethod]
    public void Afk_NullFalse_StringTrue_BoolPassThrough()
    {
        var options = Opts(new AfkConverter());
        JsonSerializer.Deserialize<bool>("null", options).Should().BeFalse();
        JsonSerializer.Deserialize<bool>("\"2026-01-01\"", options).Should().BeTrue();
        JsonSerializer.Deserialize<bool>("true", options).Should().BeTrue();
    }

    [TestMethod]
    public void ArrayOrSingle_Null_Single_Array()
    {
        var options = Opts(new ArrayOrSingleConverter<TColor>());
        JsonSerializer.Deserialize<IReadOnlyList<TColor>>("null", options).Should().BeEmpty();
        JsonSerializer.Deserialize<IReadOnlyList<TColor>>("\"red\"", options).Should().Equal(TColor.Red);
        JsonSerializer.Deserialize<IReadOnlyList<TColor>>("""["red","grn"]""", options).Should().Equal(TColor.Red, TColor.Green);
    }

    [TestMethod]
    public void FalsyList_False_EmptyList_Array_Parsed()
    {
        var options = Opts(new FalsyListConverter<int>());
        JsonSerializer.Deserialize<IReadOnlyList<int>>("false", options).Should().BeEmpty();
        JsonSerializer.Deserialize<IReadOnlyList<int>>("[1,2]", options).Should().Equal(1, 2);
    }

    //gate finding #1: Newtonsoft's StringEnumConverter (AllowIntegerValues default) binds a quoted numeric
    //string to the underlying value; the port must match or every tolerant enum drops "17"-style values to zero.
    [TestMethod]
    public void TolerantEnum_QuotedNumericString_BindsToUnderlying()
        => JsonSerializer.Deserialize<TColor>("\"2\"", Opts()).Should().Be(TColor.Green);

    //gate finding #5: a number in the AFK slot must coerce, not throw (a throw discards the whole frame).
    [TestMethod]
    public void Afk_Number_CoercesToBool()
    {
        var options = Opts(new AfkConverter());
        JsonSerializer.Deserialize<bool>("1", options).Should().BeTrue();
        JsonSerializer.Deserialize<bool>("0", options).Should().BeFalse();
    }

    [JsonConverter(typeof(TolerantStringEnumConverterFactory))]
    private enum TColor
    {
        None = 0,
        Red = 1,
        [EnumMember(Value = "grn")]
        Green = 2
    }

    [JsonConverter(typeof(LowerCaseTolerantStringEnumConverterFactory))]
    private enum TSlot
    {
        None = 0,
        MainHand = 1,
        OffHand = 2
    }

    private sealed class Holder
    {
        [JsonPropertyName("c")]
        public TColor? Color { get; set; }
    }
}
