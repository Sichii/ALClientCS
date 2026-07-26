#region
using System.Linq;
using AL.Core.Definitions;
using AL.Data.Items;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     Asserts the exact wire form of the emit payloads that were silently wrong. Two of these were destructive: a
///     mis-cased trade slot made the server consume the item instead of listing it, and a missing "calculate" flag turned
///     a dry-run upgrade into a real one.
/// </summary>
[NotInParallel(ParallelKeys.GAME_DATA)]
public class EmitPayloadTests
{
    [Test]
    public void BankPacksSerializeLowercase()
    {
        TestJson.Emit(BankPack.Items0)
                .Should()
                .Be(@"""items0""");

        TestJson.Emit(BankPack.Items1)
                .Should()
                .Be(@"""items1""");
    }

    [Test]
    public void CraftPairsArePositionThenInventorySlot()
    {
        //server.js:5970 reads x[1] as the inventory slot; functions.js:2867 pushes [gridPosition, slot]
        int[] slots =
        [
            3,
            7
        ];

        var payload = new
        {
            items = slots.Select((inventorySlot, gridPosition) => new[]
            {
                gridPosition,
                inventorySlot
            })
        };

        TestJson.Emit(payload)
                .Should()
                .Be(@"{""items"":[[0,3],[1,7]]}");
    }

    [Test]
    public void EquipmentSlotsSerializeLowercase()
    {
        TestJson.Emit(Slot.MainHand)
                .Should()
                .Be(@"""mainhand""");

        TestJson.Emit(Slot.OffHand)
                .Should()
                .Be(@"""offhand""");

        TestJson.Emit(Slot.Ring1)
                .Should()
                .Be(@"""ring1""");

        TestJson.Emit(Slot.Helmet)
                .Should()
                .Be(@"""helmet""");
    }

    [Test]
    public void ItemPriceBindsToTheValueKeyNotTheGoldFindStat()
    {
        //"g" is the item's value; "gold" on an item is the gold-find stat and is 0 for almost every item,
        //so pricing off it made the affordability check a no-op
        const string FIREBLADE = @"{ ""name"":""fireblade"", ""g"":96000 }";
        const string HORSECAPEG = @"{ ""name"":""horsecapeg"", ""g"":1200, ""gold"":3 }";
        const string CASH_ITEM = @"{ ""name"":""wbook1"", ""g"":2000, ""cash"":120 }";

        //G-data binds through ALJson.Options (what GameData.Bind uses), not the socket options
        var fireblade = TestJson.Data<GItem>(FIREBLADE)!;
        var horsecape = TestJson.Data<GItem>(HORSECAPEG)!;
        var cashItem = TestJson.Data<GItem>(CASH_ITEM)!;

        fireblade.GoldValue
                 .Should()
                 .Be(96000f);

        fireblade.Gold
                 .Should()
                 .Be(0f);

        horsecape.GoldValue
                 .Should()
                 .Be(1200f);

        horsecape.Gold
                 .Should()
                 .Be(3f);

        //cash is the shell price, a number - not a flag
        cashItem.Cash
                .Should()
                .Be(120f);

        fireblade.Cash
                 .Should()
                 .Be(0f);
    }

    [Test]
    public void LowercasedEnumsStillDeserialize()
    {
        TestJson.Socket<Slot>(@"""mainhand""")
                .Should()
                .Be(Slot.MainHand);

        TestJson.Socket<TradeSlot>(@"""trade1""")
                .Should()
                .Be(TradeSlot.Trade1);

        TestJson.Socket<BankPack>(@"""items0""")
                .Should()
                .Be(BankPack.Items0);

        //and an unknown value still degrades rather than throwing
        TestJson.Socket<Slot>(@"""nonexistent_slot""")
                .Should()
                .Be(Slot.None);
    }

    [Test]
    public void TradeSlotsSerializeLowercase()
    {
        //"Trade1" misses get_trade_slots entirely, and the server then falls through to the
        //consume branch - listing an hpot0 drank it
        TestJson.Emit(TradeSlot.Trade1)
                .Should()
                .Be(@"""trade1""");

        TestJson.Emit(TradeSlot.Trade30)
                .Should()
                .Be(@"""trade30""");
    }
}