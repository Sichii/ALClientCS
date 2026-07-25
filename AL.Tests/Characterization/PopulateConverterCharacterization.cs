#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AL.Core.Definitions;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins the five paths that replaced Newtonsoft's <c>serializer.Populate</c> converters, driven through the
///     production socket options: <see cref="AL.SocketClient.Json.SystemTextJson.DisappearDataConverter" />,
///     <see cref="AL.SocketClient.Json.SystemTextJson.EventAndBossDataConverter" />,
///     <see cref="Player.OnDeserialized" />, <see cref="Character.OnDeserialized" /> and the
///     <c>StringOrObjectConverter</c> path that drives <see cref="Prediction" />. Every value here was pinned
///     against Newtonsoft first and reproduces unchanged, with one deliberate exception: a bare
///     <see cref="Player" /> now back-fills its slots too (see <see cref="T14_BarePlayer_Slots_ArePrePopulated" />).
/// </summary>
[TestClass]
public class PopulateConverterCharacterization
{
    #region DisappearData - the three "s" shapes
    [TestMethod]
    public void T14_DisappearData_ArrayShape_SetsToOrientation_NotSpawnId()
    {
        const string DISAPPEAR_ARRAY = @"{ ""id"":""CeeNote"", ""reason"":""transport"", ""s"":[5, 10, 1] }";

        var data = TestJson.Socket<DisappearData>(DISAPPEAR_ARRAY);

        Assert.IsNotNull(data);
        Assert.AreEqual("CeeNote", data.Id);
        Assert.AreEqual("transport", data.Reason);

        // array "s" => orientation, and the spawn-id branch is never taken
        Assert.IsNull(data.ToSpawnId);
        Assert.IsNotNull(data.ToOrientation);
        Assert.AreEqual(5f, data.ToOrientation!.X);
        Assert.AreEqual(10f, data.ToOrientation.Y);
        Assert.AreEqual(Direction.Left, data.ToOrientation.Direction);
    }

    [TestMethod]
    public void T14_DisappearData_IntegerShape_SetsToSpawnId()
    {
        const string DISAPPEAR_INT = @"{ ""id"":""CeeNote"", ""reason"":""transport"", ""s"":1 }";

        var data = TestJson.Socket<DisappearData>(DISAPPEAR_INT);

        Assert.IsNotNull(data);
        Assert.AreEqual("CeeNote", data.Id);

        // Newtonsoft guarded this branch with `spawn == default ? int : null`, where `spawn` was always the
        // default Orientation, so the guard never fired and the spawn id was always read. The port drops the
        // dead guard and reads it outright; the observable output is the same and this pin holds either way.
        Assert.AreEqual(1, data.ToSpawnId);
        Assert.IsNull(data.ToOrientation);
    }

    [TestMethod]
    public void T14_DisappearData_AbsentShape_LeavesBothNull()
    {
        const string DISAPPEAR_ABSENT = @"{ ""id"":""CeeNote"", ""reason"":""transport"" }";

        var data = TestJson.Socket<DisappearData>(DISAPPEAR_ABSENT);

        Assert.IsNotNull(data);
        Assert.AreEqual("CeeNote", data.Id);
        Assert.IsNull(data.ToSpawnId);
        Assert.IsNull(data.ToOrientation);
    }
    #endregion

    #region EventAndBossData - boss dictionary keyed by name, Id mirrors the key
    [TestMethod]
    public void T14_EventAndBossData_FillsBossInfo_EachIdEqualsKey()
    {
        const string EVENT_AND_BOSS_DATA = @"{
   ""icegolem"":{ ""live"":true, ""map"":""winterland"", ""hp"":16000000, ""max_hp"":16000000, ""x"":808.9124940370274, ""y"":407.6040564394661 },
   ""snowman"":{ ""live"":true, ""map"":""winterland"", ""hp"":1200, ""max_hp"":1200, ""x"":1111.7317564125299, ""y"":-785.8382420118533 },
   ""franky"":{ ""live"":true, ""map"":""level2w"", ""hp"":120000000, ""max_hp"":120000000, ""x"":-278.0075274742135, ""y"":187.81118535586882 }
}";

        var data = TestJson.Socket<EventAndBossData>(EVENT_AND_BOSS_DATA);

        Assert.IsNotNull(data);
        Assert.AreEqual(3, data.BossInfo.Count);
        CollectionAssert.AreEquivalent(
            new[] { "icegolem", "snowman", "franky" },
            data.BossInfo.Keys.ToArray());

        // the converter assigns each BossInfo.Id from its dictionary key via a compiled setter
        foreach ((var key, var boss) in data.BossInfo)
            Assert.AreEqual(key, boss.Id);

        var iceGolem = data.BossInfo["icegolem"];
        Assert.IsTrue(iceGolem.Live);
        Assert.AreEqual("winterland", iceGolem.Map);
        Assert.AreEqual(16000000f, iceGolem.HP);
        Assert.AreEqual(16000000f, iceGolem.MaxHP);
        Assert.AreEqual(808.9125f, iceGolem.X, 0.001f);
        Assert.AreEqual(407.6041f, iceGolem.Y, 0.001f);
    }
    #endregion

    #region Prediction - the "p" key: bare string, object, and false
    [TestMethod]
    public void T14_Prediction_BareString_SetsTitle_WithoutContainsData()
    {
        var item = TestJson.Socket<Item>(@"{ ""name"":""fireblade"", ""p"":""shiny"" }");

        Assert.IsNotNull(item);
        Assert.IsNotNull(item.Prediction);
        Assert.AreEqual("shiny", item.Prediction!.Title);
        Assert.IsFalse(item.Prediction.ContainsData);
    }

    [TestMethod]
    public void T14_Prediction_Object_SetsContainsData_AndUpgradeDetails()
    {
        var item = TestJson.Socket<Item>(
            @"{ ""name"":""placeholder"", ""p"":{ ""chance"":0.6, ""name"":""fireblade"", ""level"":2, ""scroll"":""scroll1"" } }");

        Assert.IsNotNull(item);
        Assert.IsNotNull(item.Prediction);
        Assert.IsTrue(item.Prediction!.ContainsData);
        Assert.AreEqual("fireblade", item.Prediction.Name);
        Assert.AreEqual(0.6f, item.Prediction.Chance);
        Assert.AreEqual(2, item.Prediction.Level);
        Assert.AreEqual("scroll1", item.Prediction.ScrollName);
        Assert.IsNull(item.Prediction.Title);
    }

    [TestMethod]
    public void T14_Prediction_False_YieldsNull_WithoutThrowing()
    {
        // the false shape is the P3-PROTO-01 blocker: a shiny-holding account sends p=false on plain
        // items, and the converter must yield null rather than throw or every player frame is discarded.
        var item = TestJson.Socket<Item>(@"{ ""name"":""fireblade"", ""p"":false }");

        Assert.IsNotNull(item);
        Assert.IsNull(item.Prediction);
    }
    #endregion

    #region Player / Character OnDeserialized - Slots pre-fill and Inventory capacity
    [TestMethod]
    public void T14_Character_Slots_FullyPrePopulated_EvenWhenPayloadNamesFew()
    {
        // names exactly one slot; Player.OnDeserialized must back-fill every remaining Slot member with null
        const string CHARACTER = @"{
   ""id"":""me"",
   ""isize"":42,
   ""items"":[],
   ""slots"":{ ""mainhand"":{ ""name"":""fireblade"" } }
}";

        var character = TestJson.Socket<Character>(CHARACTER);

        Assert.IsNotNull(character);

        var expectedSlots = Enum.GetValues<Slot>();
        Assert.AreEqual(expectedSlots.Length, character.Slots.Count);
        foreach (var slot in expectedSlots)
            Assert.IsTrue(character.Slots.ContainsKey(slot), $"missing slot {slot}");

        // the named slot survives the back-fill; all others are present but null
        Assert.IsNotNull(character.Slots[Slot.MainHand]);
        Assert.AreEqual("fireblade", character.Slots[Slot.MainHand]!.Name);
        Assert.IsNull(character.Slots[Slot.Chest]);
    }

    [TestMethod]
    public void T14_Character_Inventory_CapacityEqualsInventorySize()
    {
        // Character.OnDeserialized runs after every member is bound, so InventorySize is set before
        // Inventory.SetCapacity reads it; if the hook stops firing, capacity reads the wire array's length
        // instead. Capacity is internal, so read it reflectively.
        const string CHARACTER = @"{
   ""id"":""me"",
   ""isize"":42,
   ""items"":[{ ""name"":""hpot0"" }, null, { ""name"":""mpot0"" }]
}";

        var character = TestJson.Socket<Character>(CHARACTER);

        Assert.IsNotNull(character);
        Assert.AreEqual(42, character.InventorySize);
        Assert.AreEqual(3, character.Inventory.Count);

        // Items is a public IReadOnlyList<Item?> property backed by a List<Item?>; read it to reach Capacity.
        var itemsProperty = typeof(Inventory).GetProperty("Items", BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(itemsProperty, "Inventory.Items accessor renamed - update this pin.");
        var backingList = itemsProperty!.GetValue(character.Inventory)!;
        var capacity = (int)backingList.GetType().GetProperty("Capacity")!.GetValue(backingList)!;

        Assert.AreEqual(character.InventorySize, capacity);
    }

    [TestMethod]
    public void T14_BarePlayer_Slots_ArePrePopulated()
    {
        // Conscious re-baseline, 0 -> every Slot. Newtonsoft ran the pre-fill inside PlayerConverter, which
        // only Character/CharacterData/StartData resolved, so a bare Player kept exactly the slots its payload
        // named. The pre-fill now lives on Player.OnDeserialized and runs for every Player shape. This is a
        // widening: the production sites that index Slots with [] no longer depend on which shape arrived.
        var player = TestJson.Socket<Player>(@"{ ""id"":""a"" }");

        Assert.IsNotNull(player);
        Assert.AreEqual(Enum.GetValues<Slot>().Length, player.Slots.Count);
    }
    #endregion
}
