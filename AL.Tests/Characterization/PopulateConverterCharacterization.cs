#region
using System;
using System.Linq;
using System.Reflection;
using AL.Core.Definitions;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins the five paths that replaced the pre-migration
///     <c>
///         serializer.Populate
///     </c>
///     converters, driven through the production socket options:
///     <see cref="AL.SocketClient.Json.SystemTextJson.DisappearDataConverter" />,
///     <see cref="AL.SocketClient.Json.SystemTextJson.EventAndBossDataConverter" />, <see cref="Player.OnDeserialized" />,
///     <see cref="Character.OnDeserialized" /> and the
///     <c>
///         StringOrObjectConverter
///     </c>
///     path that drives <see cref="Prediction" />. Every value here was pinned pre-migration first and reproduces
///     unchanged, with one deliberate exception: a bare <see cref="Player" /> now back-fills its slots too (see
///     <see cref="T14_BarePlayer_Slots_ArePrePopulated" />).
/// </summary>
public class PopulateConverterCharacterization
{
    #region EventAndBossData - boss dictionary keyed by name, Id mirrors the key
    [Test]
    public void T14_EventAndBossData_FillsBossInfo_EachIdEqualsKey()
    {
        const string EVENT_AND_BOSS_DATA = @"{
   ""icegolem"":{ ""live"":true, ""map"":""winterland"", ""hp"":16000000, ""max_hp"":16000000, ""x"":808.9124940370274, ""y"":407.6040564394661 },
   ""snowman"":{ ""live"":true, ""map"":""winterland"", ""hp"":1200, ""max_hp"":1200, ""x"":1111.7317564125299, ""y"":-785.8382420118533 },
   ""franky"":{ ""live"":true, ""map"":""level2w"", ""hp"":120000000, ""max_hp"":120000000, ""x"":-278.0075274742135, ""y"":187.81118535586882 }
}";

        var data = TestJson.Socket<EventAndBossData>(EVENT_AND_BOSS_DATA);

        data.Should()
            .NotBeNull();

        data.BossInfo
            .Count
            .Should()
            .Be(3);

        data.BossInfo
            .Keys
            .ToArray()
            .Should()
            .BeEquivalentTo("icegolem", "snowman", "franky");

        // the converter assigns each BossInfo.Id from its dictionary key via a compiled setter
        foreach ((var key, var boss) in data.BossInfo)
            boss.Id
                .Should()
                .Be(key);

        var iceGolem = data.BossInfo["icegolem"];

        iceGolem.Live
                .Should()
                .BeTrue();

        iceGolem.Map
                .Should()
                .Be("winterland");

        iceGolem.HP
                .Should()
                .Be(16000000f);

        iceGolem.MaxHP
                .Should()
                .Be(16000000f);

        iceGolem.X
                .Should()
                .BeApproximately(808.9125f, 0.001f);

        iceGolem.Y
                .Should()
                .BeApproximately(407.6041f, 0.001f);
    }
    #endregion

    #region DisappearData - the three "s" shapes
    [Test]
    public void T14_DisappearData_ArrayShape_SetsToOrientation_NotSpawnId()
    {
        const string DISAPPEAR_ARRAY = @"{ ""id"":""CeeNote"", ""reason"":""transport"", ""s"":[5, 10, 1] }";

        var data = TestJson.Socket<DisappearData>(DISAPPEAR_ARRAY);

        data.Should()
            .NotBeNull();

        data.Id
            .Should()
            .Be("CeeNote");

        data.Reason
            .Should()
            .Be("transport");

        // array "s" => orientation, and the spawn-id branch is never taken
        data.ToSpawnId
            .Should()
            .BeNull();

        data.ToOrientation
            .Should()
            .NotBeNull();

        data.ToOrientation!.X
            .Should()
            .Be(5f);

        data.ToOrientation
            .Y
            .Should()
            .Be(10f);

        data.ToOrientation
            .Direction
            .Should()
            .Be(Direction.Left);
    }

    [Test]
    public void T14_DisappearData_IntegerShape_SetsToSpawnId()
    {
        const string DISAPPEAR_INT = @"{ ""id"":""CeeNote"", ""reason"":""transport"", ""s"":1 }";

        var data = TestJson.Socket<DisappearData>(DISAPPEAR_INT);

        data.Should()
            .NotBeNull();

        data.Id
            .Should()
            .Be("CeeNote");

        // The pre-migration converter guarded this branch with `spawn == default ? int : null`, where `spawn` was always the
        // default Orientation, so the guard never fired and the spawn id was always read. The port drops the
        // dead guard and reads it outright; the observable output is the same and this pin holds either way.
        data.ToSpawnId
            .Should()
            .Be(1);

        data.ToOrientation
            .Should()
            .BeNull();
    }

    [Test]
    public void T14_DisappearData_AbsentShape_LeavesBothNull()
    {
        const string DISAPPEAR_ABSENT = @"{ ""id"":""CeeNote"", ""reason"":""transport"" }";

        var data = TestJson.Socket<DisappearData>(DISAPPEAR_ABSENT);

        data.Should()
            .NotBeNull();

        data.Id
            .Should()
            .Be("CeeNote");

        data.ToSpawnId
            .Should()
            .BeNull();

        data.ToOrientation
            .Should()
            .BeNull();
    }
    #endregion

    #region Prediction - the "p" key: bare string, object, and false
    [Test]
    public void T14_Prediction_BareString_SetsTitle_WithoutContainsData()
    {
        var item = TestJson.Socket<Item>(@"{ ""name"":""fireblade"", ""p"":""shiny"" }");

        item.Should()
            .NotBeNull();

        item.Prediction
            .Should()
            .NotBeNull();

        item.Prediction!.Title
            .Should()
            .Be("shiny");

        item.Prediction
            .ContainsData
            .Should()
            .BeFalse();
    }

    [Test]
    public void T14_Prediction_Object_SetsContainsData_AndUpgradeDetails()
    {
        var item = TestJson.Socket<Item>(
            @"{ ""name"":""placeholder"", ""p"":{ ""chance"":0.6, ""name"":""fireblade"", ""level"":2, ""scroll"":""scroll1"" } }");

        item.Should()
            .NotBeNull();

        item.Prediction
            .Should()
            .NotBeNull();

        item.Prediction!.ContainsData
            .Should()
            .BeTrue();

        item.Prediction
            .Name
            .Should()
            .Be("fireblade");

        item.Prediction
            .Chance
            .Should()
            .Be(0.6f);

        item.Prediction
            .Level
            .Should()
            .Be(2);

        item.Prediction
            .ScrollName
            .Should()
            .Be("scroll1");

        item.Prediction
            .Title
            .Should()
            .BeNull();
    }

    [Test]
    public void T14_Prediction_False_YieldsNull_WithoutThrowing()
    {
        // the false shape is the P3-PROTO-01 blocker: a shiny-holding account sends p=false on plain
        // items, and the converter must yield null rather than throw or every player frame is discarded.
        var item = TestJson.Socket<Item>(@"{ ""name"":""fireblade"", ""p"":false }");

        item.Should()
            .NotBeNull();

        item.Prediction
            .Should()
            .BeNull();
    }
    #endregion

    #region Player / Character OnDeserialized - Slots pre-fill and Inventory capacity
    [Test]
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

        character.Should()
                 .NotBeNull();

        var expectedSlots = Enum.GetValues<Slot>();

        character.Slots
                 .Count
                 .Should()
                 .Be(expectedSlots.Length);

        foreach (var slot in expectedSlots)
            character.Slots
                     .ContainsKey(slot)
                     .Should()
                     .BeTrue($"missing slot {slot}");

        // the named slot survives the back-fill; all others are present but null
        character.Slots[Slot.MainHand]
                 .Should()
                 .NotBeNull();

        character.Slots[Slot.MainHand]!.Name
                 .Should()
                 .Be("fireblade");

        character.Slots[Slot.Chest]
                 .Should()
                 .BeNull();
    }

    [Test]
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

        character.Should()
                 .NotBeNull();

        character.InventorySize
                 .Should()
                 .Be(42);

        character.Inventory
                 .Count
                 .Should()
                 .Be(3);

        // Items is a public IReadOnlyList<Item?> property backed by a List<Item?>; read it to reach Capacity.
        var itemsProperty = typeof(Inventory).GetProperty("Items", BindingFlags.Public | BindingFlags.Instance);

        itemsProperty.Should()
                     .NotBeNull("Inventory.Items accessor renamed - update this pin.");
        var backingList = itemsProperty.GetValue(character.Inventory)!;

        var capacity = (int)backingList.GetType()
                                       .GetProperty("Capacity")!.GetValue(backingList)!;

        capacity.Should()
                .Be(character.InventorySize);
    }

    [Test]
    public void T14_BarePlayer_Slots_ArePrePopulated()
    {
        // Conscious re-baseline, 0 -> every Slot. The pre-migration converter ran the pre-fill inside PlayerConverter, which
        // only Character/CharacterData/StartData resolved, so a bare Player kept exactly the slots its payload
        // named. The pre-fill now lives on Player.OnDeserialized and runs for every Player shape. This is a
        // widening: the production sites that index Slots with [] no longer depend on which shape arrived.
        var player = TestJson.Socket<Player>(@"{ ""id"":""a"" }");

        player.Should()
              .NotBeNull();

        player.Slots
              .Count
              .Should()
              .Be(
                  Enum.GetValues<Slot>()
                      .Length);
    }
    #endregion
}