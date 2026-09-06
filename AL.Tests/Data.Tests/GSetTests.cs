#region
using System.Text.Json;
using AL.APIClient;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Data;
using AL.Data.Sets;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     The armor set ladder, checked against figures folded by hand rather than by asking the rollup what it thinks.
/// </summary>
/// <remarks>
///     Built on <see cref="GameDataTestBed" /> rather than the committed fixture on purpose. The thing most likely to
///     break this is not our arithmetic but the payload changing shape - the server folds the tiers into its own copy of G
///     at boot and we depend on that fold never reaching the appengine. Only a live read can notice that changing.
/// </remarks>
public sealed class GSetTests : GameDataTestBed
{
    /// <summary>
    ///     A set authors fewer tiers than it has pieces, and the server's fold runs to the member count regardless - so the
    ///     counts past the last authored tier repeat the top total rather than granting nothing.
    /// </summary>
    [Test]
    public void CountsPastTheTopTierKeepTheTopTotal()
    {
        var holidays = GameData.Sets["holidays"]!;

        holidays.Tiers
                .Should()
                .HaveCount(17, "the fold runs to the member count, not to the seven tiers authored");

        holidays.Tiers[6]
                .InEffect
                .Attributes
                .Should()
                .BeEquivalentTo(
                    holidays.Tiers[3].InEffect.Attributes,
                    "tiers five through seven are authored empty, so seven pieces is what four is");
    }

    /// <summary>
    ///     Every set builds a rung per member item, so a count can always be indexed straight into the ladder.
    /// </summary>
    [Test]
    public void EverySetHasOneTierPerMemberItem()
        => GameData.Sets
                   .Entries
                   .Values
                   .Should()
                   .OnlyContain(set => set.Tiers.Count == set.Items.Count);

    /// <summary>
    ///     Heavy armor is the clearest case: one stat, every tier, so the ladder is checkable in the head. 2, 4, 6, 10 and 16
    ///     on the wire fold to 2, 6, 12, 22 and 38.
    /// </summary>
    [Test]
    [Arguments(1, 2f, 2f)]
    [Arguments(2, 4f, 6f)]
    [Arguments(3, 6f, 12f)]
    [Arguments(4, 10f, 22f)]
    [Arguments(5, 16f, 38f)]
    public void HeavyArmorFoldsEachTierIntoTheNext(int pieces, float adds, float inEffect)
    {
        var tier = GameData.Sets["wt3"]!.Tiers[pieces - 1];

        tier.Pieces
            .Should()
            .Be(pieces);

        tier.Adds
            .Attributes
            .GetValueOrDefault(ALAttribute.For)
            .Should()
            .Be(adds, "that is the tier's own line on the wire");

        tier.InEffect
            .Attributes
            .GetValueOrDefault(ALAttribute.For)
            .Should()
            .Be(inEffect, "the server applies every tier up to the worn count, summed");
    }

    /// <summary>
    ///     <see cref="GSetTier.InEffect" /> is built by hand in
    ///     <c>
    ///         GameData.EnrichSets
    ///     </c>
    ///     , which fills only <see cref="AttributedRecordBase.Attributes" /> and leaves the ~45 declared stat properties at
    ///     their default of nought - filling them by hand too would mean fighting the
    ///     <c>
    ///         init
    ///     </c>
    ///     /
    ///     <c>
    ///         protected set
    ///     </c>
    ///     accessors <see cref="AttributedRecordBase" /> declares them with, which was tried and rejected as worse than the
    ///     asymmetry. <see cref="GSetTier.Adds" /> has no such gap: it comes off the wire, and the JSON converter fills both
    ///     halves of every deserialized <see cref="GSetBonus" />. So the two properties on the same tier answer
    ///     <c>
    ///         .For
    ///     </c>
    ///     differently for no reason a caller can see by looking at the type. A future consumer reaching for
    ///     <c>
    ///         tier.InEffect.For
    ///     </c>
    ///     - the obviously named, most natural way to ask - gets a silent nought instead of a compile error or a thrown
    ///     exception. Read a set bonus through <see cref="AttributedRecordBase.Attributes" />, always; this test exists to
    ///     fail loudly the day someone forgets that and to explain why when it does.
    /// </summary>
    [Test]
    public void InEffectLivesOnlyInAttributesWhileAddsAlsoFillsTheDeclaredProperties()
    {
        var tier = GameData.Sets["wt3"]!.Tiers[4];

        tier.Pieces
            .Should()
            .Be(5);

        tier.InEffect
            .Attributes
            .GetValueOrDefault(ALAttribute.For)
            .Should()
            .Be(38f, "the dictionary is the only representation EnrichSets populates on InEffect");

        tier.InEffect
            .For
            .Should()
            .Be(0f, "the declared property is never filled by hand - reading it silently loses the bonus");

        tier.Adds
            .Attributes
            .GetValueOrDefault(ALAttribute.For)
            .Should()
            .Be(16f, "deserialization fills the dictionary just like it does for InEffect's raw ingredients");

        tier.Adds
            .For
            .Should()
            .Be(16f, "deserialization also fills the declared property, unlike the hand-built InEffect");
    }

    /// <summary>
    ///     The raw tiers are cleared once folded. Anything left holding them would show up as raw JSON in the game-data
    ///     explorer, and would make the fold unsafe to run twice.
    /// </summary>
    [Test]
    public void TheRawTiersAreClearedOnceFolded()
        => GameData.Sets
                   .Entries
                   .Values
                   .Should()
                   .OnlyContain(set => set.WireTiers == null);

    /// <summary>
    ///     The assumption everything else rests on: the wire carries per-tier deltas. If the appengine ever starts serving
    ///     what the game server folds for itself,
    ///     <c>
    ///         EnrichSets
    ///     </c>
    ///     would double every bonus - and silently, because a summed table is still a well-formed one.
    /// </summary>
    [Test]
    public async Task TheShippedTableIsRawRatherThanRolledUp()
    {
        using var payload = JsonDocument.Parse(await AlApiClient.GetGameDataAsync());

        var thirdTier = payload.RootElement
                               .GetProperty("sets")
                               .GetProperty("rugged")
                               .GetProperty("3");

        thirdTier.TryGetProperty("armor", out _)
                 .Should()
                 .BeTrue("rugged's third tier is the one that grants armor");

        thirdTier.TryGetProperty("str", out _)
                 .Should()
                 .BeFalse("strength belongs to the second tier - finding it here means the payload arrives folded");
    }
}