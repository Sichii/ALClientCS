#region
using AL.APIClient.Interfaces;
using AL.APIClient.Model;
using AL.APIClient.Response;
using AL.Client;
using AL.Client.Helpers;
using AL.Core.Helpers;
using AL.SocketClient;
using Common.Logging;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     A bank door is answered "in progress" and lands seconds later, once the server has loaded the bank. These hold
///     the wait that answer extends, and the magiport refusal that rides on it.
/// </summary>
public class DoorWaitTests
{
    [Test]
    public async Task AnOrdinaryDoorLandsInsideTheNetworkTimeout()
    {
        var landed = new TaskCompletionSource<string>();
        var inProgress = new TaskCompletionSource();
        var crossing = new List<bool>();

        var wait = DoorWait.ForLandingAsync(
            landed.Task,
            inProgress.Task,
            100,
            2000,
            crossing.Add);

        landed.SetResult("main");

        (await wait).Should()
                    .Be("main");

        crossing.Should()
                .BeEmpty();
    }

    [Test]
    public async Task AnUnansweredDoorTimesOutAtTheNetworkTimeout()
    {
        var landed = new TaskCompletionSource<string>();
        var inProgress = new TaskCompletionSource();
        var crossing = new List<bool>();

        var act = () => DoorWait.ForLandingAsync(
            landed.Task,
            inProgress.Task,
            100,
            2000,
            crossing.Add);

        await act.Should()
                 .ThrowAsync<TimeoutException>()
                 .WithMessage("*100ms*");

        crossing.Should()
                .BeEmpty();
    }

    [Test]
    public async Task AnInProgressAnswerExtendsTheWaitAndRaisesTheCrossing()
    {
        var landed = new TaskCompletionSource<string>();
        var inProgress = new TaskCompletionSource();
        var crossing = new List<bool>();

        var wait = DoorWait.ForLandingAsync(
            landed.Task,
            inProgress.Task,
            100,
            5000,
            crossing.Add);

        inProgress.SetResult();

        //well past the network timeout, and still waiting
        await Task.Delay(500);

        wait.IsCompleted.Should()
            .BeFalse();

        crossing.Should()
                .Equal(true);

        landed.SetResult("bank");

        (await wait).Should()
                    .Be("bank");

        crossing.Should()
                .Equal(true, false);
    }

    [Test]
    public async Task ABankThatNeverLandsTimesOutAtTheBankTimeoutAndLowersTheCrossing()
    {
        var landed = new TaskCompletionSource<string>();
        var inProgress = new TaskCompletionSource();
        var crossing = new List<bool>();
        inProgress.SetResult();

        var act = () => DoorWait.ForLandingAsync(
            landed.Task,
            inProgress.Task,
            100,
            300,
            crossing.Add);

        await act.Should()
                 .ThrowAsync<TimeoutException>()
                 .WithMessage("*300ms*");

        crossing.Should()
                .Equal(true, false);
    }

    [Test]
    public async Task AMagiportIsRefusedWhileABankDoorIsStillLanding()
    {
        var client = new Warrior(
            "test",
            new UnusedApiClient(),
            new ALSocketClient(new FormattedLogger("test", LogManager.GetLogger<ALSocketClient>())))
        {
            BankCrossingPending = true
        };

        var act = () => client.AcceptMagiportAsync("barcode");

        await act.Should()
                 .ThrowAsync<InvalidOperationException>()
                 .WithMessage("*bank door*");
    }

    /// <summary>
    ///     The constructor rejects a null API client and nothing under test calls one.
    /// </summary>
    private sealed class UnusedApiClient : IAlApiClient
    {
        public AuthUser Auth => throw new NotSupportedException();

        public IAsyncEnumerable<Mail> GetMailAsync() => throw new NotSupportedException();

        public IAsyncEnumerable<MerchantInfo> GetMerchantsAsync() => throw new NotSupportedException();

        public Task<ServersAndCharactersResponse> GetServersAndCharactersAsync(bool forceRefresh = false) => throw new NotSupportedException();

        public Task ReadMailAsync(Mail mail) => throw new NotSupportedException();

        public Task RenewAuth() => throw new NotSupportedException();
    }
}
