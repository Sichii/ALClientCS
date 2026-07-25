#region
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     T13 — collection identity and mutation under Newtonsoft.
/// </summary>
/// <remarks>
///     Production downcasts a deserialized <see cref="System.Collections.Generic.IReadOnlyList{T}" /> /
///     <see cref="System.Collections.Generic.IReadOnlyDictionary{TKey,TValue}" /> to its concrete mutable
///     type and mutates it in place (ALClient.cs:4077/4172, PlayerConverter.cs:22, Inventory.cs:35). STJ
///     materialises those interfaces as different runtime types, so these tests pin the exact type
///     Newtonsoft produces today and prove the downcast currently succeeds — the migration must keep it
///     succeeding.
/// </remarks>
[TestClass]
public class CollectionIdentityCharacterization
{
    /// <summary>
    ///     A minimal real-shape player frame: id plus one equipped slot. Player's type-level converter is
    ///     AttributedObjectConverter (new() + serializer.Populate) — NOT PlayerConverter, so no slot fill.
    /// </summary>
    private const string PLAYER_FRAME = @"{""id"":""testplayer"",""ctype"":""mage"",""slots"":{""mainhand"":{""name"":""fireblade""}}}";

    /// <summary>
    ///     A minimal real-shape character frame. Character's converter is CharacterConverter : PlayerConverter,
    ///     so this is the only path that reaches PlayerConverter.cs:22 (the Slots downcast + slot fill) and
    ///     CharacterConverter's Inventory.SetCapacity (the Inventory.cs:35 downcast). Both "items" and "isize"
    ///     are required — SetCapacity dereferences Inventory, which stays null! if "items" is absent.
    /// </summary>
    private const string CHARACTER_FRAME =
        @"{""id"":""me"",""ctype"":""mage"",""slots"":{""mainhand"":{""name"":""fireblade""}},""items"":[{""name"":""hpot0""}],""isize"":42}";

    /// <summary>
    ///     A minimal real-shape event_and_boss frame: one object-valued boss entry plus a scalar flag.
    /// </summary>
    private const string EVENT_AND_BOSS_FRAME = @"{""egghunt"":false,""franky"":{""live"":true,""hp"":100,""map"":""main"",""x"":0,""y"":0}}";

    /// <summary>
    ///     A minimal real-shape entities frame: one monster and one player.
    /// </summary>
    private const string ENTITIES_FRAME =
        @"{""in"":""main"",""map"":""main"",""type"":""all"",""monsters"":[{""id"":""m1"",""type"":""goo""}],""players"":[{""id"":""p1"",""ctype"":""mage""}]}";

    #region Player.Slots — plain Player goes through AttributedObjectConverter, no slot fill
    [TestMethod]
    public void T13_PlainPlayer_Slots_IsMutableDictionary_WithWireEntriesOnly()
    {
        var player = JsonConvert.DeserializeObject<Player>(PLAYER_FRAME);

        player.Should().NotBeNull();

        // Newtonsoft's ObjectCreationHandling.Auto reuses the non-null Dictionary initializer and appends,
        // so the runtime type stays the mutable Dictionary and the downcast holds.
        player!.Slots.Should().BeOfType<Dictionary<Slot, SlotItem?>>();

        var downcast = (Dictionary<Slot, SlotItem?>)player.Slots;

        // Player's converter is AttributedObjectConverter, NOT PlayerConverter, so the missing-slot fill
        // never runs for a plain player: only the single wire entry is present.
        downcast.Should().ContainKey(Slot.MainHand);
        downcast[Slot.MainHand].Should().NotBeNull();
        downcast.Count.Should().Be(1);

        var stj = TestJson.Socket<Player>(PLAYER_FRAME)!;

        // the property that actually matters: production downcasts this, so it must stay the mutable Dictionary
        stj.Slots.Should().BeOfType<Dictionary<Slot, SlotItem?>>();

        var stjDowncast = (Dictionary<Slot, SlotItem?>)stj.Slots;

        stjDowncast.Should().ContainKey(Slot.MainHand);
        stjDowncast[Slot.MainHand].Should().NotBeNull();

        // DIVERGENCE (T14): Phase 4 moved the missing-slot back-fill onto Player.OnDeserialized, which STJ fires
        // for a bare Player too - where Newtonsoft only ran it from PlayerConverter, i.e. for Character. So an
        // entities-frame player now carries every Slot key with a null value instead of only its wire entries.
        stjDowncast.Count.Should().Be(System.Enum.GetValues<Slot>().Length);
        stjDowncast[Slot.Trade30].Should().BeNull();
    }
    #endregion

    #region Character — the only path reaching PlayerConverter.cs:22 and Inventory.cs:35
    [TestMethod]
    public void T13_Character_PlayerConverterFillsAllSlots_AndSlotsDowncastSucceeds()
    {
        var character = JsonConvert.DeserializeObject<Character>(CHARACTER_FRAME);

        character.Should().NotBeNull();

        // Reaching here proves PlayerConverter.cs:22's (Dictionary<Slot, SlotItem?>)player.Slots did not throw.
        character!.Slots.Should().BeOfType<Dictionary<Slot, SlotItem?>>();

        var downcast = (Dictionary<Slot, SlotItem?>)character.Slots;

        // PlayerConverter fills every missing Slot member with null; only Character reaches that loop.
        downcast.Count.Should().Be(System.Enum.GetValues<Slot>().Length);
        downcast.Should().ContainKey(Slot.MainHand);
        downcast[Slot.MainHand].Should().NotBeNull();
        downcast.Should().ContainKey(Slot.Trade30);
        downcast[Slot.Trade30].Should().BeNull();

        var stj = TestJson.Socket<Character>(CHARACTER_FRAME)!;

        stj.Slots.Should().BeOfType<Dictionary<Slot, SlotItem?>>();

        var stjDowncast = (Dictionary<Slot, SlotItem?>)stj.Slots;

        // identical under STJ: the fill moved to Player.OnDeserialized but reaches Character just the same
        stjDowncast.Count.Should().Be(System.Enum.GetValues<Slot>().Length);
        stjDowncast[Slot.MainHand].Should().NotBeNull();
        stjDowncast[Slot.Trade30].Should().BeNull();
    }

    [TestMethod]
    public void T13_Character_Inventory_SetCapacityDowncastSucceeds()
    {
        // CharacterConverter calls Inventory.SetCapacity(InventorySize) during deserialization, which runs
        // the (List<Item?>)Items downcast at Inventory.cs:35. A throw would surface as an InvalidCastException.
        var act = () => JsonConvert.DeserializeObject<Character>(CHARACTER_FRAME);

        act.Should().NotThrow();

        var character = act()!;
        character.Inventory.Count.Should().Be(1);
        character.Inventory[0]!.Name.Should().Be("hpot0");

        // the same downcast under STJ, where InventoryConverter builds the Inventory through its ctor
        var stjAct = () => TestJson.Socket<Character>(CHARACTER_FRAME);

        stjAct.Should().NotThrow();

        var stj = stjAct()!;
        stj.Inventory.Count.Should().Be(1);
        stj.Inventory[0]!.Name.Should().Be("hpot0");
    }
    /// <summary>
    ///     The post-deserialize step must survive a frame with no <c>items</c> key, which leaves the inventory
    ///     null. Reachable the moment the callback actually fires, and fatal to the whole frame if it does not
    ///     guard — the socket path swallows the throw and discards everything.
    /// </summary>
    [TestMethod]
    public void T13_Character_WithoutItems_OnDeserialized_DoesNotThrow()
    {
        var stjAct = () => TestJson.Socket<Character>(@"{""id"":""me"",""ctype"":""mage"",""isize"":42}");

        stjAct.Should().NotThrow();

        //the slot fill still ran, so an unequipped slot reads null rather than throwing KeyNotFoundException
        var character = stjAct()!;

        ((Dictionary<Slot, SlotItem?>)character.Slots).Count.Should().Be(System.Enum.GetValues<Slot>().Length);
    }

    #endregion

    #region EventAndBossData.BossInfo — ALClient.cs:4077,4172 downcast
    [TestMethod]
    public void T13_BossInfo_RuntimeTypeIsMutableDictionary()
    {
        var data = JsonConvert.DeserializeObject<EventAndBossData>(EVENT_AND_BOSS_FRAME);

        data.Should().NotBeNull();
        data!.BossInfo.Should().BeOfType<Dictionary<string, BossInfo>>();

        TestJson.Socket<EventAndBossData>(EVENT_AND_BOSS_FRAME)!
                .BossInfo.Should()
                .BeOfType<Dictionary<string, BossInfo>>();
    }

    [TestMethod]
    public void T13_BossInfo_DowncastSucceedsAndIsMutable()
    {
        var data = JsonConvert.DeserializeObject<EventAndBossData>(EVENT_AND_BOSS_FRAME)!;

        var downcast = (Dictionary<string, BossInfo>)data.BossInfo;

        downcast.Should().ContainKey("franky");
        downcast["franky"].Id.Should().Be("franky");

        // ALClient mutates this dictionary in place (DestroyEntity removes, OnServerInfo mutates HP).
        downcast.Remove("franky");
        data.BossInfo.Should().BeEmpty();

        var stj = TestJson.Socket<EventAndBossData>(EVENT_AND_BOSS_FRAME)!;
        var stjDowncast = (Dictionary<string, BossInfo>)stj.BossInfo;

        stjDowncast.Should().ContainKey("franky");
        stjDowncast["franky"].Id.Should().Be("franky");

        stjDowncast.Remove("franky");
        stj.BossInfo.Should().BeEmpty();
    }
    #endregion

    #region Inventory.Items — Inventory.cs:35 downcast
    [TestMethod]
    public void T13_InventoryItems_RuntimeTypeIsMutableList()
    {
        var inventory = JsonConvert.DeserializeObject<Inventory>(@"[{""name"":""hpot0""},{""name"":""mpot0""}]");

        inventory.Should().NotBeNull();
        inventory!.Count.Should().Be(2);

        // Nullable-reference annotations are erased at runtime, so List<Item> and List<Item?> are the
        // same type; the private Items field is what SetCapacity downcasts.
        inventory[0].Should().NotBeNull();
        inventory[0]!.Name.Should().Be("hpot0");

        var stj = TestJson.Socket<Inventory>(@"[{""name"":""hpot0""},{""name"":""mpot0""}]")!;

        stj.Count.Should().Be(2);
        stj[0]!.Name.Should().Be("hpot0");
    }

    [TestMethod]
    public void T13_InventorySetCapacity_DowncastToListSucceeds()
    {
        var inventory = JsonConvert.DeserializeObject<Inventory>(@"[{""name"":""hpot0""}]")!;

        // SetCapacity performs (List<Item?>)Items internally; a throw here would be an InvalidCastException.
        var act = () => inventory.SetCapacity(42);

        act.Should().NotThrow();

        // InventoryConverter must hand the ctor a List<Item?>, or this downcast breaks in production
        var stj = TestJson.Socket<Inventory>(@"[{""name"":""hpot0""}]")!;
        var stjAct = () => stj.SetCapacity(42);

        stjAct.Should().NotThrow();
    }
    #endregion

    #region EntitiesData.Monsters / .Players — IReadOnlyList<T> materialisation
    [TestMethod]
    public void T13_EntitiesData_Collections_RuntimeType()
    {
        var entities = JsonConvert.DeserializeObject<EntitiesData>(ENTITIES_FRAME);

        entities.Should().NotBeNull();
        entities!.Monsters.Should().HaveCount(1);
        entities.Players.Should().HaveCount(1);

        // FINDING vs the plan: it claims Newtonsoft materialises IReadOnlyList<T> as ReadOnlyCollection<T>.
        // It does NOT here — because both properties are initialized to a concrete `new List<T>()`, and
        // Newtonsoft's ObjectCreationHandling.Auto REUSES that instance and appends, so the runtime type is
        // List<T>, the same type STJ produces. There is no ReadOnlyCollection->List flip for these members;
        // the real behavioural difference is that STJ REPLACES the initializer where Newtonsoft appends (S15).
        entities.Monsters.Should().BeOfType<List<Monster>>();
        entities.Players.Should().BeOfType<List<Player>>();

        var stj = TestJson.Socket<EntitiesData>(ENTITIES_FRAME)!;

        // STJ replaces the initializer rather than appending to it, but lands on the same runtime type
        stj.Monsters.Should().BeOfType<List<Monster>>();
        stj.Players.Should().BeOfType<List<Player>>();
        stj.Monsters.Should().HaveCount(1);
        stj.Players.Should().HaveCount(1);
    }
    #endregion

    #region Pre-seeded collection initializer — populated vs replaced
    private sealed class SeededListHolder
    {
        [JsonProperty("values")]
        public List<int> Values { get; set; } = [1, 2, 3];
    }

    private sealed class SeededGetOnlyListHolder
    {
        [JsonProperty("values")]
        public List<int> Values { get; } = [1, 2, 3];
    }

    [TestMethod]
    public void T13_PreSeededCollection_WithSetter_IsAppendedNotReplaced()
    {
        var holder = JsonConvert.DeserializeObject<SeededListHolder>(@"{""values"":[4,5]}");

        holder.Should().NotBeNull();

        // Newtonsoft's default ObjectCreationHandling.Auto reuses the initialized instance and APPENDS.
        // STJ replaces the whole collection. This is S15 — latent today because every production seed is
        // empty, but the semantics differ.
        holder!.Values.Should().Equal(1, 2, 3, 4, 5);

        // S15, now measured rather than predicted: STJ discards the seed and takes the wire array whole.
        TestJson.Data<SeededListHolder>(@"{""values"":[4,5]}")!.Values.Should().Equal(4, 5);
    }

    [TestMethod]
    public void T13_PreSeededCollection_GetOnly_IsAppendedNotReplaced()
    {
        var holder = JsonConvert.DeserializeObject<SeededGetOnlyListHolder>(@"{""values"":[4,5]}");

        holder.Should().NotBeNull();
        holder!.Values.Should().Equal(1, 2, 3, 4, 5);

        // S15's second, sharper face: with no setter STJ cannot replace and will not populate, so the wire
        // array is dropped entirely and the seed survives untouched. Newtonsoft appended to it.
        TestJson.Data<SeededGetOnlyListHolder>(@"{""values"":[4,5]}")!.Values.Should().Equal(1, 2, 3);
    }
    #endregion
}
