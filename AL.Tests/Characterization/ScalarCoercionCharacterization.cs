#region
using System;
using AL.APIClient.Model;
using AL.SocketClient.SocketModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StjJsonException = System.Text.Json.JsonException;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins Newtonsoft's cross-scalar coercion (T10). System.Text.Json throws where Newtonsoft coerces,
///     and under <c>ALSocketClient.OnAny</c> such a throw is swallowed, silently discarding the entire
///     socket frame. These tests fix the current behaviour so the migration has a target.
/// </summary>
/// <remarks>
///     Deserialization goes through <see cref="JsonConvert.DeserializeObject{T}(string)" />, which uses
///     the same default settings as the production socket path
///     (<c>JsonSerializer.CreateDefault(new JsonSerializerSettings())</c>), so these results are the ones
///     the live client actually produces.
/// </remarks>
[TestClass]
public sealed class ScalarCoercionCharacterization
{
    // Each coercion is exercised as an object-member bind, matching how the socket path receives frames.
    private sealed record IntBox
    {
        public int V { get; set; }
    }

    private sealed record LongBox
    {
        public long V { get; set; }
    }

    private sealed record BoolBox
    {
        public bool V { get; set; }
    }

    private sealed record StringBox
    {
        public string V { get; set; } = null!;
    }

    private sealed record FloatBox
    {
        public float V { get; set; }
    }

    [TestMethod]
    public void T10_StringDigits_Into_Int_Coerces()
    {
        var box = JsonConvert.DeserializeObject<IntBox>(@"{""v"":""25""}")!;

        box.V.Should().Be(25);

        //STJ agrees, via NumberHandling.AllowReadingFromString on the shared options
        TestJson.Data<IntBox>(@"{""v"":""25""}")!.V.Should().Be(25);
    }

    [TestMethod]
    public void T10_Number_Into_Bool_Coerces_NonZero_To_True()
    {
        var box = JsonConvert.DeserializeObject<BoolBox>(@"{""v"":152311}")!;

        box.V.Should().BeTrue();

        //STJ agrees, via the globally registered LenientBooleanConverter
        TestJson.Data<BoolBox>(@"{""v"":152311}")!.V.Should().BeTrue();
    }

    // FINDING: the plan (T10 row, S16) lists 100.0 -> int as a Newtonsoft coercion. That holds on the
    // JToken/Value<int>() path, but the socket path deserializes straight into a typed member, where
    // JsonTextReader.ReadAsInt32 rejects a fractional literal outright. Newtonsoft already throws here.
    [TestMethod]
    public void T10_WholeFloat_Into_Int_Throws()
    {
        var act = () => JsonConvert.DeserializeObject<IntBox>(@"{""v"":100.0}");

        act.Should()
           .Throw<JsonReaderException>()
           .WithMessage("*not a valid integer*");

        //STJ also rejects it; only the exception type differs, and both are fatal to the frame either way
        var stjAct = () => TestJson.Data<IntBox>(@"{""v"":100.0}");

        stjAct.Should().Throw<StjJsonException>();
    }

    [TestMethod]
    public void T10_ExponentNotation_Into_Long_Coerces()
    {
        var box = JsonConvert.DeserializeObject<LongBox>(@"{""v"":1e3}")!;

        box.V.Should().Be(1000L);

        StjExponentIntoLong();
    }

    [TestMethod]
    public void T10_Number_Into_String_Coerces()
    {
        var box = JsonConvert.DeserializeObject<StringBox>(@"{""v"":5}")!;

        box.V.Should().Be("5");

        //STJ agrees, via the globally registered LenientStringConverter (an account owner id arrives bare)
        TestJson.Data<StringBox>(@"{""v"":5}")!.V.Should().Be("5");
    }

    [TestMethod]
    public void T10_StringDecimal_Into_Float_Coerces()
    {
        var box = JsonConvert.DeserializeObject<FloatBox>(@"{""v"":""1.5""}")!;

        box.V.Should().Be(1.5f);

        //STJ agrees, via NumberHandling.AllowReadingFromString
        TestJson.Data<FloatBox>(@"{""v"":""1.5""}")!.V.Should().Be(1.5f);
    }

    [TestMethod]
    public void T10_EmptyString_Into_Int_Throws()
    {
        var act = () => JsonConvert.DeserializeObject<IntBox>(@"{""v"":""""}");

        act.Should().Throw<JsonSerializationException>();

        var stjAct = () => TestJson.Data<IntBox>(@"{""v"":""""}");

        stjAct.Should().Throw<StjJsonException>();
    }

    [TestMethod]
    public void T10_Null_Into_NonNullableInt_Throws()
    {
        var act = () => JsonConvert.DeserializeObject<IntBox>(@"{""v"":null}");

        act.Should().Throw<JsonSerializationException>();

        var stjAct = () => TestJson.Data<IntBox>(@"{""v"":null}");

        stjAct.Should().Throw<StjJsonException>();
    }

    // FINDING: the plan lists true -> int as a Newtonsoft coercion (Convert.ToInt32(true) == 1). Again
    // that is the JToken path; on the direct-member socket path ReadAsInt32 rejects the boolean literal
    // and Newtonsoft throws. There is no int-coercion of a JSON bool to preserve here.
    [TestMethod]
    public void T10_Bool_Into_Int_Throws()
    {
        var act = () => JsonConvert.DeserializeObject<IntBox>(@"{""v"":true}");

        act.Should()
           .Throw<JsonReaderException>()
           .WithMessage("*Unexpected character*");

        var stjAct = () => TestJson.Data<IntBox>(@"{""v"":true}");

        stjAct.Should().Throw<StjJsonException>();
    }

    /// <summary>
    ///     DIVERGENCE, and one the plan's re-baseline list does not name: Newtonsoft coerces exponent notation
    ///     into an integer member (<c>1e3</c> -> <c>1000</c>); System.Text.Json rejects it outright, for every
    ///     integer type, with or without <c>AllowReadingFromString</c>.
    /// </summary>
    /// <remarks>
    ///     Accepted rather than papered over, because the server cannot reach it: the wire is produced by
    ///     <c>JSON.stringify</c>, which only emits exponent notation at magnitudes of 1e21 and above — already
    ///     far outside <see cref="long" /> — and never for a value an integer member could have held. Floating
    ///     point members are unaffected: System.Text.Json parses exponent notation into
    ///     <see cref="float" />/<see cref="double" /> normally, which is the only place small-magnitude
    ///     exponent forms (<c>1e-7</c>) actually arrive.
    /// </remarks>
    private static void StjExponentIntoLong()
    {
        var stjAct = () => TestJson.Data<LongBox>(@"{""v"":1e3}");

        stjAct.Should().Throw<StjJsonException>();

        //the floating-point counterpart, which is the reachable case, does still coerce
        TestJson.Data<FloatBox>(@"{""v"":1e-7}")!.V.Should().Be(1e-7f);
    }

    #region Real production DTOs receiving these shapes

    // AchievementProgressData.Count is an int; the server sends the string form (JsonConverterTests.cs:21).
    [TestMethod]
    public void T10_AchievementProgressData_StringCount_Into_Int_Coerces()
    {
        const string ACHIEVEMENT_PROGRESS_DATA = @"{
   ""name"":""firehazard"",
   ""count"":""25"",
   ""needed"":""19975""
}";

        var data = JsonConvert.DeserializeObject<AchievementProgressData>(ACHIEVEMENT_PROGRESS_DATA)!;

        data.Count.Should().Be(25);
        data.Needed.Should().Be(19975);

        var stj = TestJson.Socket<AchievementProgressData>(ACHIEVEMENT_PROGRESS_DATA)!;

        stj.Count.Should().Be(25);
        stj.Needed.Should().Be(19975);
    }

    // CharacterInfo.Online is the raw ms-since-last-seen number (0 == offline); IsOnline derives the boolean.
    [TestMethod]
    public void T10_CharacterInfo_NumberOnline_PreservedAsMilliseconds()
    {
        const string CHARACTER_INFO = @"{
   ""id"":""makiz"",
   ""name"":""makiz"",
   ""level"":68,
   ""online"":152311
}";

        var info = JsonConvert.DeserializeObject<CharacterInfo>(CHARACTER_INFO)!;

        info.Online.Should().Be(152311);
        info.IsOnline.Should().BeTrue();

        var stj = TestJson.Api<CharacterInfo>(CHARACTER_INFO)!;

        stj.Online.Should().Be(152311);
        stj.IsOnline.Should().BeTrue();
    }

    #endregion
}
