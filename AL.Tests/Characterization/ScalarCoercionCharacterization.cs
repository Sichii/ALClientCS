#region
using AL.APIClient.Model;
using AL.SocketClient.SocketModel;
using FluentAssertions;
using StjJsonException = System.Text.Json.JsonException;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins cross-scalar coercion (T10): which mismatched wire scalars still bind to a CLR member and which throw. A throw
///     matters beyond the test — under
///     <c>
///         ALSocketClient.OnAny
///     </c>
///     it is swallowed, silently discarding the entire socket frame.
/// </summary>
/// <remarks>
///     Deserialization goes through <see cref="TestJson" />, so every result here is produced by the same production
///     options the live client binds frames with, not by a serializer configured for the test.
/// </remarks>
public sealed class ScalarCoercionCharacterization
{
    // FINDING: the plan lists true -> int as a coercion to preserve (Convert.ToInt32(true) == 1). That was
    // only ever the DOM path; binding a bool literal straight into an int member always threw. There is no
    // int-coercion of a JSON bool to preserve here.
    [Test]
    public void T10_Bool_Into_Int_Throws()
    {
        var act = () => TestJson.Data<IntBox>(@"{""v"":true}");

        act.Should()
           .Throw<StjJsonException>();
    }

    [Test]
    public void T10_EmptyString_Into_Int_Throws()
    {
        var act = () => TestJson.Data<IntBox>(@"{""v"":""""}");

        act.Should()
           .Throw<StjJsonException>();
    }

    /// <summary>
    ///     DIVERGENCE, and one the plan's re-baseline list does not name: exponent notation used to coerce into an integer
    ///     member (
    ///     <c>
    ///         1e3
    ///     </c>
    ///     ->
    ///     <c>
    ///         1000
    ///     </c>
    ///     ); System.Text.Json rejects it outright, for every integer type, with or without
    ///     <c>
    ///         AllowReadingFromString
    ///     </c>
    ///     . The System.Text.Json side is what is asserted here, and the test is named for it rather than for the behaviour it
    ///     replaced.
    /// </summary>
    /// <remarks>
    ///     Accepted rather than papered over, because the server cannot reach it: the wire is produced by
    ///     <c>
    ///         JSON.stringify
    ///     </c>
    ///     , which only emits exponent notation at magnitudes of 1e21 and above — already far outside <see cref="long" /> —
    ///     and never for a value an integer member could have held. Floating point members are unaffected: System.Text.Json
    ///     parses exponent notation into <see cref="float" />/<see cref="double" /> normally, which is the only place
    ///     small-magnitude exponent forms (
    ///     <c>
    ///         1e-7
    ///     </c>
    ///     ) actually arrive.
    /// </remarks>
    [Test]
    public void T10_ExponentNotation_Into_Long_Throws()
    {
        var act = () => TestJson.Data<LongBox>(@"{""v"":1e3}");

        act.Should()
           .Throw<StjJsonException>();

        //the floating-point counterpart, which is the reachable case, does still coerce
        TestJson.Data<FloatBox>(@"{""v"":1e-7}")!.V
                .Should()
                .Be(1e-7f);
    }

    [Test]
    public void T10_Null_Into_NonNullableInt_Throws()
    {
        var act = () => TestJson.Data<IntBox>(@"{""v"":null}");

        act.Should()
           .Throw<StjJsonException>();
    }

    [Test]
    public void T10_Number_Into_Bool_Coerces_NonZero_To_True()
        =>

            //coerces via the globally registered LenientBooleanConverter
            TestJson.Data<BoolBox>(@"{""v"":152311}")!.V
                    .Should()
                    .BeTrue();

    [Test]
    public void T10_Number_Into_String_Coerces()
        =>

            //coerces via the globally registered LenientStringConverter (an account owner id arrives bare)
            TestJson.Data<StringBox>(@"{""v"":5}")!.V
                    .Should()
                    .Be("5");

    [Test]
    public void T10_StringDecimal_Into_Float_Coerces()
        =>

            //coerces via NumberHandling.AllowReadingFromString
            TestJson.Data<FloatBox>(@"{""v"":""1.5""}")!.V
                    .Should()
                    .Be(1.5f);

    [Test]
    public void T10_StringDigits_Into_Int_Coerces()
        =>

            //coerces via NumberHandling.AllowReadingFromString on the shared options
            TestJson.Data<IntBox>(@"{""v"":""25""}")!.V
                    .Should()
                    .Be(25);

    // FINDING: the plan (T10 row, S16) lists 100.0 -> int as a coercion to preserve. It never was one on the
    // path that matters: a frame binds straight into a typed member, and both engines reject a fractional
    // literal there. Only the exception type was re-baselined, and either way the frame is lost.
    [Test]
    public void T10_WholeFloat_Into_Int_Throws()
    {
        var act = () => TestJson.Data<IntBox>(@"{""v"":100.0}");

        act.Should()
           .Throw<StjJsonException>();
    }

    // Each coercion is exercised as an object-member bind, matching how the socket path receives frames, so
    // every setter below is written by the serializer rather than by the test.
    private sealed record BoolBox
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool V { get; set; }
    }

    private sealed record FloatBox
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public float V { get; set; }
    }

    private sealed record IntBox
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int V { get; set; }
    }

    private sealed record LongBox
    {
        //never read - the long case only ever pins a throw, but the member must exist for 1e3 to have a target
        // ReSharper disable once UnusedMember.Global
        public long V { get; set; }
    }

    private sealed record StringBox
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string V { get; set; } = null!;
    }

    #region Real production DTOs receiving these shapes
    // AchievementProgressData.Count is an int; the server sends the string form (JsonConverterTests.cs:21).
    [Test]
    public void T10_AchievementProgressData_StringCount_Into_Int_Coerces()
    {
        const string ACHIEVEMENT_PROGRESS_DATA = @"{
   ""name"":""firehazard"",
   ""count"":""25"",
   ""needed"":""19975""
}";

        //Socket, not Data: this DTO arrives on a frame, so the string-to-int coercion must hold under the
        //options the transport actually binds with.
        var data = TestJson.Socket<AchievementProgressData>(ACHIEVEMENT_PROGRESS_DATA)!;

        data.Count
            .Should()
            .Be(25);

        data.Needed
            .Should()
            .Be(19975);
    }

    // CharacterInfo.Online is the raw ms-since-last-seen number (0 == offline); IsOnline derives the boolean.
    [Test]
    public void T10_CharacterInfo_NumberOnline_PreservedAsMilliseconds()
    {
        const string CHARACTER_INFO = @"{
   ""id"":""makiz"",
   ""name"":""makiz"",
   ""level"":68,
   ""online"":152311
}";

        //Api, not Socket: CharacterInfo comes back from the REST endpoint.
        var info = TestJson.Api<CharacterInfo>(CHARACTER_INFO)!;

        info.Online
            .Should()
            .Be(152311);

        info.IsOnline
            .Should()
            .BeTrue();
    }
    #endregion
}