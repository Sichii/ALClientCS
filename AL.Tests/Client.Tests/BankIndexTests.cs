#region
using AL.APIClient.Interfaces;
using AL.APIClient.Model;
using AL.APIClient.Response;
using AL.Client;
using AL.Client.Model;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.Data;
using AL.SocketClient;
using AL.SocketClient.Model;
using AL.Tests.Characterization;
using Common.Logging;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     FindOptimalBankIndex is public, and the contract of its explicit-slot branch is what any bank-organize routine
///     builds on. The explicit-slot tests need neither a socket nor game data: that branch returns before it reads either.
///     The stacking tests read an item's stack size off the committed snapshot.
/// </summary>
[NotInParallel(ParallelKeys.GAME_DATA)]
public class BankIndexTests
{
    private const BankPack PACK = BankPack.Items0;

    [Test]
    public void ALockedPileIsNotOfferedAsAStackTarget()
    {
        //can_stack refuses a locked pile outright (js/old_common_functions.js:398)
        var client = ClientHolding(
            Item("hpot0") with
            {
                LockType = ItemLockType.Locked
            });

        client.FindOptimalBankIndex(Indexed("hpot0"), PACK)
              .Should()
              .Be((PACK, 1));
    }

    [Test]
    public void ANamedOccupiedSlotIsReturnedRatherThanRefused()
    {
        //the emit's swap branch exchanges the two slots raw, so organizing into an occupied slot is the ordinary
        //case. Returning null here is what DepositItemAsync turns into "no space"
        var client = ClientHolding(Item("hpot0"), Item("mpot0"));

        client.FindOptimalBankIndex(Indexed("cscroll0"), PACK, 0)
              .Should()
              .Be((PACK, 0));
    }

    [Test]
    public void APackThisCharacterCannotReachIsRefused()
    {
        var client = ClientHolding(Item("hpot0"));

        client.FindOptimalBankIndex(Indexed("cscroll0"), BankPack.Items7, 0)
              .Should()
              .BeNull();
    }

    [Test]
    public void APileWhoseDataAgreesIsOfferedAsAStackTarget()
    {
        var client = ClientHolding(Item("cxjar", "makeawish"));

        client.FindOptimalBankIndex(Indexed("cxjar", "makeawish"), PACK)
              .Should()
              .Be((PACK, -1));
    }

    [Test]
    public void APileWhoseDataDiffersIsNotOfferedAsAStackTarget()
    {
        //the server stacks two cxjars only when their data agrees (js/old_common_functions.js:396). A name-only match
        //handed the store sentinel to a pack whose one cxjar held another appearance, and in a full pack the server
        //answered storage_full on every trip while the vault had room elsewhere
        var client = ClientHolding(Item("cxjar", "makeawish"));

        client.FindOptimalBankIndex(Indexed("cxjar", "ikissyou"), PACK)
              .Should()
              .Be((PACK, 1));
    }

    [Test]
    public void ASlotPastTheEndOfThePackIsRefused()
    {
        var client = ClientHolding(Item("hpot0"));

        client.FindOptimalBankIndex(Indexed("cscroll0"), PACK, 42)
              .Should()
              .BeNull();

        client.FindOptimalBankIndex(Indexed("cscroll0"), PACK, -2)
              .Should()
              .BeNull();
    }

    [Test]
    public void AnEmptySlotPastTheEndOfATrimmedPackIsStillAValidTarget()
    {
        //a pack arrives trimmed to its highest occupied slot, so the array is shorter than the 42 slots the server
        //counts. Bounding on the array's length rejects free slots
        var client = ClientHolding(Item("hpot0"));

        client.FindOptimalBankIndex(Indexed("cscroll0"), PACK, 41)
              .Should()
              .Be((PACK, 41));
    }

    private static Warrior ClientHolding(params Item?[] pack)
    {
        var client = new Warrior(
            "test",
            new UnusedApiClient(),
            new ALSocketClient(new FormattedLogger("test", LogManager.GetLogger<ALSocketClient>())));

        var bank = new BankInfo
        {
            Items = new Dictionary<BankPack, IReadOnlyList<Item?>>
            {
                [PACK] = pack
            }
        };

        //private setter: the frame that normally fills this arrives over the socket, and none of these tests has one
        typeof(ALClient).GetProperty(nameof(ALClient.Bank))!.SetValue(client, bank);

        return client;
    }

    [Before(Class)]
    public static void EnsureGameData()
    {
        //from the committed snapshot so no credentials are needed, the way ProjectileMitigationTests does it
        if (GameData.Version == 0)
            GameData.Populate(Fixture.GameDataJson);
    }

    private static InventoryIndexer Indexed(string name, string? data = null)
        => new()
        {
            Index = 0,
            Item = Item(name, data)
        };

    private static Item Item(string name, string? data = null)
        => new()
        {
            Name = name,
            Quantity = 1,
            Data = data
        };

    [Test]
    public void TheStoreSentinelIsAccepted()
    {
        //-1 is both this method's own stacking answer and the server's "you pick a slot". Feeding the method's own
        //output back in has to survive
        var client = ClientHolding(Item("hpot0"));

        client.FindOptimalBankIndex(Indexed("hpot0"), PACK, -1)
              .Should()
              .Be((PACK, -1));
    }

    /// <summary>
    ///     The constructor rejects a null API client and nothing under test calls one.
    /// </summary>
    private sealed class UnusedApiClient : IAlApiClient
    {
        public AuthUser Auth => throw new NotSupportedException();

        public Task DeleteMailAsync(Mail mail) => throw new NotSupportedException();

        public IAsyncEnumerable<Mail> GetMailAsync() => throw new NotSupportedException();

        public IAsyncEnumerable<MerchantInfo> GetMerchantsAsync() => throw new NotSupportedException();

        public Task<ServersAndCharactersResponse> GetServersAndCharactersAsync(bool forceRefresh = false)
            => throw new NotSupportedException();

        public Task ReadMailAsync(Mail mail) => throw new NotSupportedException();

        public Task RenewAuth() => throw new NotSupportedException();
    }
}