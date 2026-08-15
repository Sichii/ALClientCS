#region
using AL.APIClient.Interfaces;
using AL.APIClient.Model;
using AL.APIClient.Response;
using AL.Client;
using AL.Client.Model;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.SocketClient;
using AL.SocketClient.Model;
using Common.Logging;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     FindOptimalBankIndex is public, and the contract of its explicit-slot branch is what any bank-organize routine
///     builds on. None of these needs a socket or game data: the explicit branch returns before it reads either.
/// </summary>
public class BankIndexTests
{
    private const BankPack PACK = BankPack.Items0;

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
    public void AnEmptySlotPastTheEndOfATrimmedPackIsStillAValidTarget()
    {
        //a pack arrives trimmed to its highest occupied slot, so the array is shorter than the 42 slots the server
        //counts. Bounding on the array's length rejects free slots
        var client = ClientHolding(Item("hpot0"));

        client.FindOptimalBankIndex(Indexed("cscroll0"), PACK, 41)
              .Should()
              .Be((PACK, 41));
    }

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
    public void APackThisCharacterCannotReachIsRefused()
    {
        var client = ClientHolding(Item("hpot0"));

        client.FindOptimalBankIndex(Indexed("cscroll0"), BankPack.Items7, 0)
              .Should()
              .BeNull();
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
        typeof(ALClient).GetProperty(nameof(ALClient.Bank))!
                        .SetValue(client, bank);

        return client;
    }

    private static InventoryIndexer Indexed(string name)
        => new()
        {
            Index = 0,
            Item = Item(name)
        };

    private static Item Item(string name)
        => new()
        {
            Name = name,
            Quantity = 1
        };

    /// <summary>
    ///     The constructor rejects a null API client and nothing under test calls one.
    /// </summary>
    private sealed class UnusedApiClient : IAlApiClient
    {
        public AuthUser Auth => throw new NotSupportedException();

        public IAsyncEnumerable<Mail> GetMailAsync() => throw new NotSupportedException();

        public IAsyncEnumerable<MerchantInfo> GetMerchantsAsync() => throw new NotSupportedException();

        public Task<ServersAndCharactersResponse> GetServersAndCharactersAsync() => throw new NotSupportedException();

        public Task ReadMailAsync(Mail mail) => throw new NotSupportedException();

        public Task RenewAuth() => throw new NotSupportedException();
    }
}
