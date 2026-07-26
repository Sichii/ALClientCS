#region
using System;
using System.Globalization;
using System.Text.Json.Nodes;
using AL.Core.Json.SystemTextJson;
using AL.Data;
using AL.Data.Items;
using FluentAssertions;
using StjJsonException = System.Text.Json.JsonException;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins where a fractional JSON number still coerces into an
///     <c>
///         int
///     </c>
///     CLR target and where it no longer does. It coerces on exactly one path: <see cref="LenientInt32Converter" /> is
///     registered solely in <see cref="AttributedObjectStjConverter{T}" />'s inner options, so <see cref="GItem.Grade" />
///     still rounds while <see cref="Recipe.Items" />'
///     <c>
///         int
///     </c>
///     slot - read by <see cref="ArrayToTupleConverter{T1,T2,T3}" />, which deserializes each element under the CALLER's
///     options - throws. Where it does round, the direction is round-half-to-even.
/// </summary>
public class NumericCoercionCharacterization
{
    /// <summary>
    ///     Fractional inputs mapped to the integer the lenient path yields, hardcoded as an independent source of truth rather
    ///     than recomputed from <see cref="Math.Round(double, MidpointRounding)" /> so the assertion pins a value, not a
    ///     tautology. The ties (2.5, 1.5, 0.5) distinguish round-half-to-even from arithmetic rounding: banker's rounding
    ///     gives 2, 2, 0 where arithmetic would give 3, 2, 1. Unchanged from the pre-migration baseline - only the path that
    ///     reaches them moved.
    /// </summary>
    private static readonly (double Input, int Expected)[] FractionalToInt =
    [
        (3.6, 4),
        (3.5, 4),
        (2.6, 3),
        (2.5, 2),
        (2.4, 2),
        (1.5, 2),
        (0.5, 0),
        (-0.5, 0),
        (0.12, 0),
        (0.1, 0)
    ];

    /// <summary>
    ///     Deserializes a recipe entry the way <see cref="AL.Data.Dismantle.DismantleDatum" /> does - through the shared
    ///     options, where the global tuple factory serves <see cref="Recipe.Items" />' per-element binding.
    /// </summary>
    private static Recipe Deserialize(JsonObject wire) => TestJson.Data<Recipe>(wire.ToJsonString())!;

    /// <summary>
    ///     Deserializes an item the way <see cref="ItemsDatum" /> does - through the shared options, which route every
    ///     <c>
    ///         IAttributed
    ///     </c>
    ///     type into <see cref="AttributedObjectStjConverter{T}" />, so the
    ///     <c>
    ///         grade
    ///     </c>
    ///     coercion under test runs on the production lenient path and not the throwing shared one.
    /// </summary>
    private static GItem DeserializeItem(JsonObject wire) => TestJson.Data<GItem>(wire.ToJsonString())!;

    /// <summary>
    ///     Fails loudly if the snapshot no longer carries the shape the assertion pins, so a data refresh that moves these
    ///     values surfaces as a clear message rather than a confusing coercion mismatch.
    /// </summary>
    private static void GuardItemsShape(
        JsonObject wire,
        string accessor,
        double expectedFirstQuantity,
        int expectedItemCount)
    {
        var items = wire["items"] as JsonArray;

        items.Should()
             .NotBeNull("dismantle.{0} must carry an items array", accessor);

        items.Count
             .Should()
             .Be(expectedItemCount, "dismantle.{0} item count is part of the pinned shape", accessor);

        // GetValue<double>() reads any JSON number regardless of how it was written, so an integral first
        // quantity (goldenegg's bare 1) compares cleanly against the expected double.
        items[0]![0]!.GetValue<double>()
                     .Should()
                     .Be(expectedFirstQuantity, "dismantle.{0}'s first quantity is the pinned input", accessor);
    }

    /// <summary>
    ///     Round-trip-safe invariant literal for a double, so
    ///     <c>
    ///         de-DE
    ///     </c>
    ///     cannot turn 3.6 into "3,6".
    /// </summary>
    private static string Literal(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    [Test]
    public void T4_AttributedInnerPath_FractionalIntoInt_RoundsHalfToEven()
    {
        // Same expectations as the pre-migration attributed-fill pin, reached differently: the lenient converter
        // lives only in the attributed converter's inner options, so the table must be driven through a type that
        // gets there. GItem.Grade is int?, which System.Text.Json's nullable wrapper routes into the same
        // converter. A plain holder is not IAttributed and throws on every row - see the test below.
        foreach ((var input, var expected) in FractionalToInt)
            TestJson.Data<GItem>($$"""{"grade":{{Literal(input)}}}""")!.Grade
                    .Should()
                    .Be(expected, "the attributed inner options round {0} half-to-even into GItem.Grade", input);
    }

    [Test]
    public void T4_DismantleGoldenegg_FractionalQuantity_CoercesToZero()
    {
        // items = [[1,"goldnugget"],[0.5,"goldnugget"]]; Quantity is float now, so 0.5 is preserved.
        var wire = (JsonObject)Fixture.Entry("dismantle", "goldenegg");

        GuardItemsShape(
            wire,
            "goldenegg",
            1,
            2);

        var recipe = Deserialize(wire);

        recipe.Items
              .Should()
              .HaveCount(2);

        recipe.Items[0]
              .Should()
              .Be((1f, "goldnugget", 0), "the first pair carries an integral quantity");

        recipe.Items[1]
              .Should()
              .Be((0.5f, "goldnugget", 0), "Quantity is float now, so 0.5 is recovered instead of rounding to 0");

        recipe.Cost
              .Should()
              .Be(120000);
    }

    [Test]
    public void T4_DismantleLostearring_FractionalQuantity_CoercesToZero()
    {
        // 0.12 into float Quantity (Phase 1 widening); array is length 2 so Level defaults to 0.
        var wire = (JsonObject)Fixture.Entry("dismantle", "lostearring");

        GuardItemsShape(
            wire,
            "lostearring",
            0.12,
            1);

        var recipe = Deserialize(wire);

        recipe.Items
              .Should()
              .ContainSingle();

        (var quantity, var itemName, var level) = recipe.Items[0];

        quantity.Should()
                .Be(0.12f, "Recipe.Items.Quantity widened to float now recovers 0.12 instead of rounding to 0");

        itemName.Should()
                .Be("goldnugget");

        level.Should()
             .Be(0, "the wire pair carries no level, so the tuple's third slot defaults to 0");

        recipe.Cost
              .Should()
              .Be(36000);
    }

    [Test]
    public void T4_DismantleMolesteeth_FractionalQuantity_CoercesToZero()
    {
        // 0.1 into float Quantity (Phase 1 widening), item is platinumnugget (not goldnugget).
        var wire = (JsonObject)Fixture.Entry("dismantle", "molesteeth");

        GuardItemsShape(
            wire,
            "molesteeth",
            0.1,
            1);

        var recipe = Deserialize(wire);

        recipe.Items
              .Should()
              .ContainSingle()
              .Which
              .Should()
              .Be((0.1f, "platinumnugget", 0), "Quantity is float now, so 0.1 is recovered instead of rounding to 0");

        recipe.Cost
              .Should()
              .Be(100000);
    }

    [Test]
    public void T4_ItemScroll4_FractionalGrade_RoundsToFour()
    {
        // grade 3.6 into int? Grade; scroll4 is the ONLY item in the snapshot with a fractional grade.
        var wire = (JsonObject)Fixture.Entry("items", "scroll4");

        wire["grade"]!.GetValue<double>()
                      .Should()
                      .Be(3.6, "the pinned input is scroll4's fractional grade");

        var item = DeserializeItem(wire);

        item.Grade
            .Should()
            .Be(4, "3.6 rounds to 4 through the attributed converter's inner lenient path");

        // members after grade in the wire must still bind - a throw here would have abandoned the object
        item.GoldValue
            .Should()
            .Be(640000000f);

        item.StackSize
            .Should()
            .Be(9999);

        item.Name
            .Should()
            .Be("Ultimate Upgrade Scroll");
    }

    [Test]
    public void T4_SharedOptionsPath_FractionalIntoInt_Throws()
    {
        // Finding, not a target: outside an IAttributed type nothing supplies leniency, so a fractional number
        // bound to an int throws - as the pre-migration text reader also did, only the exception type was
        // re-baselined. This is the behaviour that keeps LenientInt32Converter scoped to the attributed inner
        // options rather than registered globally: the socket path must keep rejecting a fractional int rather
        // than silently rounding a live frame.
        var act = () => TestJson.Data<NullableIntHolder>("""{"value":3.6}""");

        act.Should()
           .Throw<StjJsonException>("the shared options bind an int with the strict Int32 reader");
    }

    [Test]
    public void T4_TupleElementPath_FractionalIntoInt_Throws()
    {
        // Inverted, consciously: this pinned rounding before the migration, when the tuple reader coerced each
        // element through the DOM. TupleElement.Read now calls node.Deserialize<T>(options) with the CALLER's
        // options, which carry no LenientInt32Converter, so the int slot throws. The snapshot is safe only by luck
        // of the data: Level is the model's only int tuple slot, and of the 188 craft+dismantle item entries just
        // 10 carry one, all integral. Quantity was widened to float in Phase 1 and still coerces.
        var act = () => TestJson.Data<Recipe>("""{"cost":1,"items":[[1,"goldnugget",3.6]]}""");

        act.Should()
           .Throw<StjJsonException>("the shared options bind a tuple's int element with the strict Int32 reader");
    }

    // no [JsonPropertyName]: the shared options match case-insensitively, so "value" binds to Value on its own.
    private sealed class NullableIntHolder
    {
        // the binding target under test; only ever written by the serializer, and only when it does not throw
        // ReSharper disable once UnusedMember.Global
        public int? Value { get; set; }
    }
}