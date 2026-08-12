#region
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     T13 — collection identity and mutation after deserialization.
/// </summary>
/// <remarks>
///     Production downcasts a deserialized <see cref="System.Collections.Generic.IReadOnlyList{T}" /> /
///     <see cref="System.Collections.Generic.IReadOnlyDictionary{TKey,TValue}" /> to its concrete mutable type and mutates
///     it in place (ALClient's boss bookkeeping,
///     <c>
///         Player.OnDeserialized
///     </c>
///     ,
///     <c>
///         Inventory.SetCapacity
///     </c>
///     ). These tests pin the exact runtime type System.Text.Json materialises those interfaces as, and prove every one of
///     those downcasts still succeeds.
/// </remarks>
public class CollectionIdentityCharacterization
{
    /// <summary>
    ///     A minimal real-shape player frame: id plus one equipped slot.
    ///     <c>
    ///         Player.OnDeserialized
    ///     </c>
    ///     back-fills the rest, so even a bare player comes back carrying every <see cref="Slot" /> key.
    /// </summary>
    private const string PLAYER_FRAME = @"{""id"":""testplayer"",""ctype"":""mage"",""slots"":{""mainhand"":{""name"":""fireblade""}}}";

    /// <summary>
    ///     A minimal real-shape character frame.
    ///     <c>
    ///         Character.OnDeserialized
    ///     </c>
    ///     runs both downcasts: the Slots downcast + slot fill it inherits from <see cref="Player" />, and
    ///     <c>
    ///         Inventory.SetCapacity
    ///     </c>
    ///     's
    ///     <c>
    ///         (List&lt;Item?&gt;)Items
    ///     </c>
    ///     . Both "items" and "isize" are present — without "items" the inventory is null and SetCapacity is skipped entirely.
    /// </summary>
    private const string CHARACTER_FRAME
        = @"{""id"":""me"",""ctype"":""mage"",""slots"":{""mainhand"":{""name"":""fireblade""}},""items"":[{""name"":""hpot0""}],""isize"":42}";

    /// <summary>
    ///     A minimal real-shape event_and_boss frame: one object-valued boss entry plus a scalar flag.
    /// </summary>
    private const string EVENT_AND_BOSS_FRAME
        = @"{""egghunt"":false,""franky"":{""live"":true,""hp"":100,""map"":""main"",""x"":0,""y"":0}}";

    /// <summary>
    ///     A minimal real-shape entities frame: one monster and one player.
    /// </summary>
    private const string ENTITIES_FRAME
        = @"{""in"":""main"",""map"":""main"",""type"":""all"",""monsters"":[{""id"":""m1"",""type"":""goo""}],""players"":[{""id"":""p1"",""ctype"":""mage""}]}";

    #region EventAndBossData.BossInfo — ALClient's in-place downcast
    [Test]
    public void T13_BossInfo_DowncastSucceedsAndIsMutable()
    {
        var data = TestJson.Socket<EventAndBossData>(EVENT_AND_BOSS_FRAME)!;

        var downcast = (Dictionary<string, BossInfo>)data.BossInfo;

        downcast.Should()
                .ContainKey("franky");

        downcast["franky"]
            .Id
            .Should()
            .Be("franky");

        // ALClient mutates this dictionary in place (DestroyEntity removes, OnServerInfo mutates HP).
        downcast.Remove("franky");

        data.BossInfo
            .Should()
            .BeEmpty();
    }
    #endregion

    #region EntitiesData.Monsters / .Players — IReadOnlyList<T> materialisation
    [Test]
    public void T13_EntitiesData_Collections_RuntimeType()
    {
        var entities = TestJson.Socket<EntitiesData>(ENTITIES_FRAME);

        entities.Should()
                .NotBeNull();

        entities.Monsters
                .Should()
                .HaveCount(1);

        entities.Players
                .Should()
                .HaveCount(1);

        // FINDING vs the plan: it claims IReadOnlyList<T> materialises as ReadOnlyCollection<T>. It does NOT
        // here — both properties are declared with a concrete `new List<T>()` initializer and STJ REPLACES that
        // instance with a List<T> of its own, so the runtime type is List<T> either way. There is no
        // ReadOnlyCollection->List flip for these members; the real behavioural difference is replace-vs-append
        // against a pre-seeded initializer (S15, measured below).
        entities.Monsters
                .Should()
                .BeOfType<List<Monster>>();

        entities.Players
                .Should()
                .BeOfType<List<Player>>();
    }
    #endregion

    #region Inventory.Items — the SetCapacity downcast
    [Test]
    public void T13_InventorySetCapacity_DowncastToListSucceeds()
    {
        // InventoryConverter must hand the ctor a List<Item?>, or SetCapacity's internal (List<Item?>)Items
        // breaks in production with an InvalidCastException.
        var inventory = TestJson.Socket<Inventory>(@"[{""name"":""hpot0""}]")!;

        var act = () => inventory.SetCapacity(42);

        act.Should()
           .NotThrow();
    }

    #endregion

    #region Inventory.GetEnumerator — SetPrediction lands mid-enumeration
    [Test]
    public void T13_InventoryEnumeration_SurvivesSetPrediction()
    {
        // q_data arrives on the socket thread and writes one slot in place while a consumer is part way through a
        // scan of the same inventory. Handing out List<T>'s enumerator made that an InvalidOperationException.
        var inventory = TestJson.Socket<Inventory>(@"[{""name"":""hpot0""},{""name"":""mpot0""},{""name"":""cscroll0""}]")!;

        var act = () =>
        {
            var seen = 0;

            foreach (var _ in inventory)
            {
                if (seen == 0)
                    inventory.SetPrediction(1, new Prediction());

                seen++;
            }

            return seen;
        };

        act.Should()
           .NotThrow()
           .Which
           .Should()
           .Be(3);
    }
    #endregion

    #region Player.Slots — the slot fill runs for a bare Player too
    [Test]
    public void T13_PlainPlayer_Slots_IsMutableDictionary_WithAllSlotsFilled()
    {
        var player = TestJson.Socket<Player>(PLAYER_FRAME);

        player.Should()
              .NotBeNull();

        // the property that actually matters: production downcasts this, so it must stay the mutable Dictionary
        player.Slots
              .Should()
              .BeOfType<Dictionary<Slot, SlotItem?>>();

        var downcast = (Dictionary<Slot, SlotItem?>)player.Slots;

        downcast.Should()
                .ContainKey(Slot.MainHand);

        downcast[Slot.MainHand]
            .Should()
            .NotBeNull();

        // DIVERGENCE (T14): Phase 4 moved the missing-slot back-fill onto Player.OnDeserialized, which fires for
        // a bare Player too — the old converter only ran it for Character. So an entities-frame player now
        // carries every Slot key with a null value instead of only its wire entries.
        downcast.Count
                .Should()
                .Be(
                    Enum.GetValues<Slot>()
                        .Length);

        downcast[Slot.Trade30]
            .Should()
            .BeNull();
    }
    #endregion

    #region Character — the path reaching both the Slots and the Inventory downcast
    [Test]
    public void T13_Character_AllSlotsFilled_AndSlotsDowncastSucceeds()
    {
        var character = TestJson.Socket<Character>(CHARACTER_FRAME);

        character.Should()
                 .NotBeNull();

        // Reaching here proves Player.OnDeserialized's (Dictionary<Slot, SlotItem?>)Slots did not throw.
        character.Slots
                 .Should()
                 .BeOfType<Dictionary<Slot, SlotItem?>>();

        var downcast = (Dictionary<Slot, SlotItem?>)character.Slots;

        // every Slot member missing from the wire is filled with null
        downcast.Count
                .Should()
                .Be(
                    Enum.GetValues<Slot>()
                        .Length);

        downcast.Should()
                .ContainKey(Slot.MainHand);

        downcast[Slot.MainHand]
            .Should()
            .NotBeNull();

        downcast.Should()
                .ContainKey(Slot.Trade30);

        downcast[Slot.Trade30]
            .Should()
            .BeNull();
    }

    [Test]
    public void T13_Character_Inventory_SetCapacityDowncastSucceeds()
    {
        // Character.OnDeserialized calls Inventory.SetCapacity(InventorySize), which runs the (List<Item?>)Items
        // downcast. InventoryConverter must build the Inventory from a List<Item?> or this is InvalidCastException.
        var act = () => TestJson.Socket<Character>(CHARACTER_FRAME);

        act.Should()
           .NotThrow();

        var character = act()!;

        character.Inventory
                 .Count
                 .Should()
                 .Be(1);

        character.Inventory[0]!.Name
                 .Should()
                 .Be("hpot0");
    }

    /// <summary>
    ///     The post-deserialize step must survive a frame with no
    ///     <c>
    ///         items
    ///     </c>
    ///     key, which leaves the inventory null. Reachable the moment the callback actually fires, and fatal to the whole
    ///     frame if it does not guard — the socket path swallows the throw and discards everything.
    /// </summary>
    [Test]
    public void T13_Character_WithoutItems_OnDeserialized_DoesNotThrow()
    {
        var stjAct = () => TestJson.Socket<Character>(@"{""id"":""me"",""ctype"":""mage"",""isize"":42}");

        stjAct.Should()
              .NotThrow();

        //the slot fill still ran, so an unequipped slot reads null rather than throwing KeyNotFoundException
        var character = stjAct()!;

        ((Dictionary<Slot, SlotItem?>)character.Slots).Count
                                                      .Should()
                                                      .Be(
                                                          Enum.GetValues<Slot>()
                                                              .Length);
    }
    #endregion

    #region Pre-seeded collection initializer — populated vs replaced
    private sealed class SeededListHolder
    {
        [JsonPropertyName("values")]
        public List<int> Values { get; set; } =
            [
                1,
                2,
                3
            ];
    }

    private sealed class SeededGetOnlyListHolder
    {
        [JsonPropertyName("values")]
        public List<int> Values { get; } =
            [
                1,
                2,
                3
            ];
    }

    [Test]
    public void T13_PreSeededCollection_WithSetter_IsReplacedNotAppended()
    {
        var holder = TestJson.Data<SeededListHolder>(@"{""values"":[4,5]}");

        holder.Should()
              .NotBeNull();

        // DIVERGENCE (S15): the pre-migration serializer's ObjectCreationHandling.Auto reused the initialized
        // instance and APPENDED, yielding 1,2,3,4,5. System.Text.Json discards the seed and takes the wire
        // array whole. Latent today because every production seed is empty, but the semantics differ.
        holder.Values
              .Should()
              .Equal(4, 5);
    }

    [Test]
    public void T13_PreSeededCollection_GetOnly_IsDroppedNotAppended()
    {
        var holder = TestJson.Data<SeededGetOnlyListHolder>(@"{""values"":[4,5]}");

        holder.Should()
              .NotBeNull();

        // DIVERGENCE (S15), sharper face: with no setter System.Text.Json can neither replace nor populate, so
        // the wire array is dropped entirely and the seed survives untouched. The pre-migration serializer
        // appended to it, yielding 1,2,3,4,5.
        holder.Values
              .Should()
              .Equal(1, 2, 3);
    }
    #endregion
}