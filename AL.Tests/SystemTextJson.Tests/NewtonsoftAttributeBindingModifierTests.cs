#region
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AL.Core.Json.SystemTextJson;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJson = Newtonsoft.Json;
#endregion

namespace AL.Tests.SystemTextJson.Tests;

/// <summary>
///     STJ Phase 3 check for the modifier that drives System.Text.Json binding from the models' existing
///     Newtonsoft attributes — so the migration touches zero members while both engines run side by side. Pins
///     that it reproduces Newtonsoft's binding: renames, non-public setters, private [JsonProperty] members, and
///     [JsonIgnore] exclusion — plus the one place it does not, an un-attributed non-public setter it over-binds.
/// </summary>
[TestClass]
public sealed class NewtonsoftAttributeBindingModifierTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { NewtonsoftAttributeBindingModifier.Modify } }
    };

    [TestMethod]
    public void BindsRenames_NonPublicSetters_PrivateMembers_And_ExcludesIgnored()
    {
        // "unbound" is the key the Unbound probe answers to; sending "in" instead (as this literal used to) meant no
        // wire key ever reached the member, so its "stays null" claim held for free.
        const string WIRE = """{"wire_name":"hi","nonpublic":5,"ren_np":9,"fld":42,"priv":7,"ignored":"nope","unbound":"x"}""";

        var model = JsonSerializer.Deserialize<Model>(WIRE, Options)!;

        // renamed public init property
        model.Renamed.Should().Be("hi");

        // public property with a non-public (protected) setter — bare [JsonProperty], matched case-insensitively
        model.NonPublic.Should().Be(5);

        // renamed public property with a non-public setter
        model.RenamedNonPublic.Should().Be(9);

        // private [JsonProperty] field surfaced via CreateJsonPropertyInfo
        model.ProbeField.Should().Be(42);

        // private [JsonProperty] property surfaced via CreateJsonPropertyInfo
        model.ProbePrivateProperty.Should().Be(7);

        // Newtonsoft [JsonIgnore] removes the member even though the wire carries "ignored"
        model.Ignored.Should().BeNull();

        // Re-baselined null -> "x": the wire now sends the key, and the modifier binds it. It supplies a Set for ANY
        // public property with a non-public setter, where Newtonsoft — and ForcedObjectConverter.BuildMembers, which
        // gets this right — bind one only when it carries [JsonProperty]. So EntityBase.In stays unbound on the entity
        // path production takes, and nothing live rides on the gap; Phase 6's [JsonInclude] cutover closes it.
        model.Unbound.Should().Be("x", "the modifier binds a non-attributed non-public setter that Newtonsoft leaves null");
    }

    /// <summary>
    ///     Phase 6a's premise, pinned: native System.Text.Json attributes reproduce every member shape the
    ///     modifier hand-surfaces, and — the row that matters — leave an <b>un-attributed</b> non-public setter
    ///     alone, which is what makes deleting the modifier a fix rather than a regression.
    /// </summary>
    /// <remarks>
    ///     Deliberately resolved through a bare <see cref="DefaultJsonTypeInfoResolver" /> with no modifier
    ///     installed, so it measures System.Text.Json itself. Without this the whole phase rests on a scratch
    ///     probe, and the first thing the flip would do is discover the premise was wrong.
    /// </remarks>
    [TestMethod]
    public void NativeAttributes_BindEveryShape_TheModifierHandSurfaces()
    {
        const string WIRE = """{"wire_name":"hi","nonpublic":5,"ren_np":9,"fld":42,"priv":7,"unbound":"x"}""";

        var native = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        var model = JsonSerializer.Deserialize<NativeModel>(WIRE, native)!;

        model.Renamed.Should().Be("hi", "[JsonPropertyName] on a public init property");
        model.NonPublic.Should().Be(5, "[JsonInclude] enables a non-public setter");
        model.RenamedNonPublic.Should().Be(9, "[JsonInclude] + [JsonPropertyName] together");
        model.ProbeField.Should().Be(42, "[JsonInclude] binds a private field, with no IncludeFields");
        model.ProbePrivateProperty.Should().Be(7, "[JsonInclude] binds a fully private property");

        model.Unbound
             .Should()
             .BeNull(
                 "an un-attributed non-public setter stays unbound natively — this is EntityBase.In, and native "
                 + "binding gets it right by construction where the modifier over-binds it");
    }

    private sealed class Model
    {
        [NJson.JsonProperty("wire_name")]
        public string? Renamed { get; init; }

        [NJson.JsonProperty]
        public int NonPublic { get; protected set; }

        [NJson.JsonProperty("ren_np")]
        public int RenamedNonPublic { get; protected set; }

        [NJson.JsonProperty("fld")]
        private int Field;

        [NJson.JsonProperty("priv")]
        private int PrivateProperty { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string? Ignored { get; set; }

        // no [JsonProperty]; non-public setter — the EntityBase.In shape, which Newtonsoft leaves unbound
        public string? Unbound { get; protected set; }

        public int ProbeField => Field;

        public int ProbePrivateProperty => PrivateProperty;
    }

    /// <summary>The same six shapes, expressed in native System.Text.Json attributes instead.</summary>
    private sealed class NativeModel
    {
        [JsonPropertyName("wire_name")]
        public string? Renamed { get; init; }

        [JsonInclude]
        public int NonPublic { get; protected set; }

        [JsonInclude]
        [JsonPropertyName("ren_np")]
        public int RenamedNonPublic { get; protected set; }

        [JsonInclude]
        [JsonPropertyName("fld")]
        private int Field;

        [JsonInclude]
        [JsonPropertyName("priv")]
        private int PrivateProperty { get; set; }

        // no attribute; non-public setter — the EntityBase.In shape, which must stay unbound
        public string? Unbound { get; protected set; }

        public int ProbeField => Field;

        public int ProbePrivateProperty => PrivateProperty;
    }
}
