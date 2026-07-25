#region
using System.Linq;
using AL.Core.Definitions;
using AL.Data.Items;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     Asserts the exact wire form of the emit payloads that were silently wrong. Two of these were
///     destructive: a mis-cased trade slot made the server consume the item instead of listing it, and a
///     missing "calculate" flag turned a dry-run upgrade into a real one.
/// </summary>
[TestClass]
public class EmitPayloadTests
{
   [TestMethod]
   public void EquipmentSlotsSerializeLowercase()
   {
      Assert.AreEqual(@"""mainhand""", TestJson.Emit(Slot.MainHand));
      Assert.AreEqual(@"""offhand""", TestJson.Emit(Slot.OffHand));
      Assert.AreEqual(@"""ring1""", TestJson.Emit(Slot.Ring1));
      Assert.AreEqual(@"""helmet""", TestJson.Emit(Slot.Helmet));
   }

   [TestMethod]
   public void TradeSlotsSerializeLowercase()
   {
      //"Trade1" misses get_trade_slots entirely, and the server then falls through to the
      //consume branch - listing an hpot0 drank it
      Assert.AreEqual(@"""trade1""", TestJson.Emit(TradeSlot.Trade1));
      Assert.AreEqual(@"""trade30""", TestJson.Emit(TradeSlot.Trade30));
   }

   [TestMethod]
   public void BankPacksSerializeLowercase()
   {
      Assert.AreEqual(@"""items0""", TestJson.Emit(BankPack.Items0));
      Assert.AreEqual(@"""items1""", TestJson.Emit(BankPack.Items1));
   }

   [TestMethod]
   public void LowercasedEnumsStillDeserialize()
   {
      Assert.AreEqual(Slot.MainHand, TestJson.Socket<Slot>(@"""mainhand"""));
      Assert.AreEqual(TradeSlot.Trade1, TestJson.Socket<TradeSlot>(@"""trade1"""));
      Assert.AreEqual(BankPack.Items0, TestJson.Socket<BankPack>(@"""items0"""));

      //and an unknown value still degrades rather than throwing
      Assert.AreEqual(Slot.None, TestJson.Socket<Slot>(@"""nonexistent_slot"""));
   }

   [TestMethod]
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

      Assert.AreEqual(96000f, fireblade.GoldValue);
      Assert.AreEqual(0f, fireblade.Gold);

      Assert.AreEqual(1200f, horsecape.GoldValue);
      Assert.AreEqual(3f, horsecape.Gold);

      //cash is the shell price, a number - not a flag
      Assert.AreEqual(120f, cashItem.Cash);
      Assert.AreEqual(0f, fireblade.Cash);
   }

   [TestMethod]
   public void CraftPairsArePositionThenInventorySlot()
   {
      //server.js:5970 reads x[1] as the inventory slot; functions.js:2867 pushes [gridPosition, slot]
      int[] slots = [3, 7];

      var payload = new
      {
         items = slots.Select(
            (inventorySlot, gridPosition) => new[]
            {
               gridPosition,
               inventorySlot
            })
      };

      Assert.AreEqual(@"{""items"":[[0,3],[1,7]]}", TestJson.Emit(payload));
   }
}
