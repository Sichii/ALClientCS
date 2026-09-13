#region
using AL.Client.Extensions;
using AL.SocketClient.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     FindExchangePrize is how ExchangeAsync decides which inventory slot the prize landed in. The old
///     AsIndexed().Except(snapshot).First() path handed back the lowest occupied slot on every call (CLAUDE.md Item
///     equality remarks), which on a stack of anniversary gifts was the consumed input — the stats feed logged
///     "got anniversarygift × 7" then 6, 5, 4 as each exchange ate one. Restate the server's add_item / consume rule
///     here: the prize is a newly occupied slot, or a pile whose quantity rose after subtracting the units taken from
///     the exchanged slot.
/// </summary>
public class FindExchangePrizeTests
{
    [Test]
    public void ANewSlotIsThePrize()
    {
        var before = Holding(Stack("seashell", 10), null);
        var after = Holding(Stack("seashell", 9), Stack("armorbox", 1));

        var prize = after.FindExchangePrize(before, consumedSlot: 0, consumedCount: 1);

        prize!.Index.Should()
             .Be(1);
        prize.Item.Name.Should()
             .Be("armorbox");
        prize.Item.Quantity.Should()
             .Be(1);
    }

    [Test]
    public void AStackMergedOntoAnExistingPileReportsOnlyTheGainedCount()
    {
        var before = Holding(Stack("seashell", 10), Stack("armorbox", 3));
        var after = Holding(Stack("seashell", 9), Stack("armorbox", 4));

        var prize = after.FindExchangePrize(before, consumedSlot: 0, consumedCount: 1);

        prize!.Index.Should()
             .Be(1);
        prize.Item.Quantity.Should()
             .Be(1);
    }

    [Test]
    public void PrizeStackingOntoTheConsumedSlotReportsNetGain()
    {
        //consume 1 gift, roll 2 gifts back onto the same pile: 8 → 9
        var before = Holding(Stack("anniversarygift", 8));
        var after = Holding(Stack("anniversarygift", 9));

        var prize = after.FindExchangePrize(before, consumedSlot: 0, consumedCount: 1);

        prize!.Index.Should()
             .Be(0);
        prize.Item.Quantity.Should()
             .Be(2);
    }

    [Test]
    public void ConsumptionAloneIsNotAPrize()
    {
        //gold-only table, or the broken Except path's anniversary-gift regression: leftover input is not the prize
        var before = Holding(Stack("anniversarygift", 8), Stack("hpot0", 50));
        var after = Holding(Stack("anniversarygift", 7), Stack("hpot0", 50));

        after.FindExchangePrize(before, consumedSlot: 0, consumedCount: 1)
             .Should()
             .BeNull();
    }

    [Test]
    public void LowestOccupiedSlotIsNotAssumedToBeThePrize()
    {
        //the bug: slot 0 still holds the leftover gifts and would win .First() on a broken Except
        var before = Holding(Stack("anniversarygift", 8), null, Stack("cscroll0", 2));
        var after = Holding(Stack("anniversarygift", 7), Stack("fury", 1), Stack("cscroll0", 2));

        var prize = after.FindExchangePrize(before, consumedSlot: 0, consumedCount: 1);

        prize!.Index.Should()
             .Be(1);
        prize.Item.Name.Should()
             .Be("fury");
    }

    private static Item Stack(string name, int quantity, int level = 0)
        => new()
        {
            Name = name,
            Level = level,
            Quantity = quantity
        };

    private static Inventory Holding(params Item?[] items) => new(items.ToList());
}
