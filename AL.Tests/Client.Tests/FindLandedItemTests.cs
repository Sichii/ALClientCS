#region
using AL.Client.Extensions;
using AL.SocketClient.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     FindLandedItem is how UnequipAsync decides which inventory slot the unequipped item landed in. The server
///     rule restated here rather than probed: add_item puts a non-stackable in the first free slot, while an equip
///     in flight on the same socket swaps in place - so of two identical items appearing at once, the unequipped
///     one is the one on a slot that was empty before the emit. A name-and-level search alone cannot tell them
///     apart, and equipping the wrong one's slot back is how a warrior cleaving with matched dual weapons ended up
///     trying to wear its own axe as an offhand.
/// </summary>
public class FindLandedItemTests
{
    private static Item Copy(string name, int level = 0)
        => new()
        {
            Name = name,
            Level = level
        };

    private static Inventory Holding(params Item?[] items) => new(items.ToList());

    [Test]
    public void TheSlotThatWasEmptyBeforeWinsOverTheDisplacedTwin()
    {
        //both hands wore a fireblade+5. The offhand's copy landed on slot 1, which was empty; the equip racing it
        //displaced the mainhand's copy onto slot 0, which held the axe. The lower index is the twin on purpose -
        //a bare first-match search returns it, and that is the bug this pins
        var inventory = Holding(Copy("fireblade", 5), Copy("fireblade", 5));

        inventory.FindLandedItem(new HashSet<int> { 0 }, "fireblade", 5)!.Index.Should().Be(1);
    }

    [Test]
    public void AStackMergedOntoAnExistingPileFallsBackToTheNameMatch()
    {
        //a stackable unequip can merge onto a pile already held, landing on no empty slot at all - the fallback
        //answers with the pile rather than never resolving
        var inventory = Holding(Copy("hpot0"), Copy("hpot0"));

        inventory.FindLandedItem(new HashSet<int> { 0, 1 }, "hpot0", 0)!.Index.Should().Be(0);
    }

    [Test]
    public void ADifferentLevelIsNotTheItem()
    {
        var inventory = Holding(Copy("fireblade", 7));

        inventory.FindLandedItem(new HashSet<int>(), "fireblade", 5).Should().BeNull();
    }

    [Test]
    public void NothingMatchingMeansNotLandedYet()
    {
        //the character callback runs on every frame with the slot empty, including ones from before the item
        //appeared - answering null is what keeps the await alive until it does
        var inventory = Holding(Copy("bataxe"), null);

        inventory.FindLandedItem(new HashSet<int> { 0 }, "fireblade", 5).Should().BeNull();
    }
}
