#region
using AL.Core.Definitions;
using AL.Data;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     Which class an item names as the only one allowed to equip it - or none, for the items that stay
///     class-neutral by leaving the key out entirely.
/// </summary>
public class GItemTests : GameDataTestBed
{
    /// <summary>A single-class lock, the common case.</summary>
    [Test]
    public void AClassLockedItemNamesItsClass()
    {
        var hat = GameData.Items["mrnhat"];

        hat!.Classes
           .Should()
           .NotBeNull();

        hat.Classes!
           .Should()
           .BeEquivalentTo(new[] { ALClass.Ranger });
    }

    /// <summary>fury names four classes whose MainStat does not agree - several is real, not a formality.</summary>
    [Test]
    public void AnItemLockedToSeveralClassesNamesThemAll()
    {
        var fury = GameData.Items["fury"];

        fury!.Classes!.Count
            .Should()
            .Be(4);

        fury.Classes!
            .Should()
            .Contain(ALClass.Paladin);
    }

    /// <summary>No key in the def means anybody, so this is null rather than an empty list.</summary>
    [Test]
    public void AClassNeutralItemNamesNoClass()
        => GameData.Items["cape"]!.Classes
                                  .Should()
                                  .BeNull();
}
