#region
using System.Diagnostics;
using AL.Client.Managers;
using AL.SocketClient.Definitions;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     The client's half of the server's rate limiter: what an emit costs, and how the window apportions it. The
///     costs themselves are the server's own table (node/server.js:144), so a wrong figure here is a bot that runs
///     confidently into a disconnect it could have seen coming.
/// </summary>
public class CallMeterTests
{
    [Test]
    public void AnEmitCostsItsRowInTheServersTablePlusWhatItsHandlerResends()
    {
        //no CC row, but commence_attack resends u+cid at modifier 1: a unit for u, a unit for the stats pass
        CallCost.Of(ALSocketEmitType.Attack)
                .Should()
                .Be(2d);

        //the same resend at the skill modifier of 0.05 - which is why a base of one per call reads a ranger at double
        CallCost.Of(ALSocketEmitType.Skill)
                .Should()
                .Be(0.1d);

        //the wrapper's add_call_cost(-1) lands on the module's false_socket, never on the player, so a method with
        //neither a CC row nor a resend costs nothing at all
        CallCost.Of(ALSocketEmitType.Use)
                .Should()
                .Be(0d);

        //a CC row and no resend
        CallCost.Of(ALSocketEmitType.Move)
                .Should()
                .Be(1.5d);

        //cruise is the expensive one a movement lane can emit on a loop: its row of 10 and a u+cid on top
        CallCost.Of(ALSocketEmitType.Cruise)
                .Should()
                .Be(12d);

        CallCost.Of(ALSocketEmitType.Tracker)
                .Should()
                .Be(50d);
    }

    [Test]
    public void TheSnapshotSplitsTheSameWindowTwoWays()
    {
        var meter = new CallMeter();
        var source = "movement";

        meter.SourceResolver = () => source;

        meter.Record(ALSocketEmitType.Cruise);
        meter.Record(ALSocketEmitType.Move);

        source = "attack";
        meter.Record(ALSocketEmitType.Attack);

        var snapshot = meter.Snapshot(serverCost: 42d);

        snapshot.ServerCost
                .Should()
                .Be(42d);

        snapshot.Emits
                .Should()
                .Be(3);

        //12 + 1.5 + 2
        snapshot.Cost
                .Should()
                .Be(15.5d);

        //both groupings partition the same window, so either one sums back to the total
        snapshot.BySource
                .Sum(row => row.Cost)
                .Should()
                .Be(snapshot.Cost);

        snapshot.ByEmitType
                .Sum(row => row.Cost)
                .Should()
                .Be(snapshot.Cost);

        //ordered by cost, so the row that is about to disconnect the character is the one at the top
        snapshot.BySource[0]
                .Should()
                .BeEquivalentTo(new CallBudgetRow("movement", 13.5d, 2));

        snapshot.ByEmitType[0]
                .Name
                .Should()
                .Be(nameof(ALSocketEmitType.Cruise));
    }

    [Test]
    public void AnEmitWithNoResolverIsStillCounted()
    {
        var meter = new CallMeter();

        meter.Record(ALSocketEmitType.Attack);

        var snapshot = meter.Snapshot(0d);

        //filed rather than dropped: login and the attach hooks emit before any caller exists to name them, and that
        //spend is exactly as real as a loop's
        snapshot.BySource
                .Should()
                .ContainSingle()
                .Which.Emits
                .Should()
                .Be(1);
    }

    /// <summary>
    ///     The server's add_call_cost folds a charge into its last entry when the method matches and keeps that
    ///     entry's date, so a run of one method expires as a block dated from its first call. Restated here rather
    ///     than read off the meter, so a meter that slid every entry on its own would go red.
    /// </summary>
    [Test]
    public void ARunOfOneMethodExpiresAsABlockDatedFromItsFirstCall()
    {
        var now = 0L;

        var meter = new CallMeter
        {
            Timestamp = () => now
        };

        meter.Record(ALSocketEmitType.Move);

        //free, so it pushes nothing on the server and the run carries on through it
        now = Ticks(1);
        meter.Record(ALSocketEmitType.Use);

        now = Ticks(3);
        meter.Record(ALSocketEmitType.Move);

        //the second move is a second and a half old, and dated with the first it is gone all the same
        now = Ticks(4.5);

        meter.Snapshot(0d)
             .Cost
             .Should()
             .Be(0d);

        //a different charged method in between is a fresh entry with its own date - the sliding window proper
        meter.Record(ALSocketEmitType.Move);
        now = Ticks(7.5);
        meter.Record(ALSocketEmitType.Attack);
        now = Ticks(9);

        meter.Snapshot(0d)
             .Cost
             .Should()
             .Be(CallCost.Of(ALSocketEmitType.Attack));
    }

    /// <summary>
    ///     The transport handler bills the bank mount as 32 and the unmount as 16 under their own name, beside the
    ///     8 every transport pays (node/server.js:5569, :5580). Between two bank floors it is a door like any other.
    /// </summary>
    [Test]
    public void ABankCrossingIsBilledBesideTheDoor()
    {
        CallCost.OfBankCrossing(fromBank: false, toBank: true)
                .Should()
                .Be(32d);

        CallCost.OfBankCrossing(fromBank: true, toBank: false)
                .Should()
                .Be(16d);

        CallCost.OfBankCrossing(fromBank: true, toBank: true)
                .Should()
                .Be(0d);

        CallCost.OfBankCrossing(fromBank: false, toBank: false)
                .Should()
                .Be(0d);
    }

    [Test]
    public void AChargeFoldsIntoTheEmitItBelongsToRatherThanCountingAsOne()
    {
        var meter = new CallMeter();

        meter.Record(ALSocketEmitType.Transport);
        meter.Charge(ALSocketEmitType.Transport, 32d);

        var snapshot = meter.Snapshot(0d);

        snapshot.Emits
                .Should()
                .Be(1);

        snapshot.Cost
                .Should()
                .Be(CallCost.Of(ALSocketEmitType.Transport) + 32d);

        //a refund comes off the same entry and stops at nothing
        meter.Charge(ALSocketEmitType.Transport, -100d);

        meter.Snapshot(0d)
             .Cost
             .Should()
             .Be(0d);
    }

    /// <summary>
    ///     A chest opened in a party resends every member at the open_chest modifier and bills the opener for all
    ///     of it: a tenth for a member who got nothing, four tenths for one who got an item, nothing for the opener's
    ///     own item (node/server.js:10460).
    /// </summary>
    [Test]
    public void AChestOpenedInAPartyBillsTheOpenerForEveryMember()
    {
        CallCost.OfChestOpen(partySize: 1, othersWithItems: 0, openerGotItem: false)
                .Should()
                .Be(CallCost.Of(ALSocketEmitType.OpenChest));

        CallCost.OfChestOpen(partySize: 1, othersWithItems: 0, openerGotItem: true)
                .Should()
                .Be(0d);

        CallCost.OfChestOpen(partySize: 4, othersWithItems: 0, openerGotItem: false)
                .Should()
                .BeApproximately(0.4d, 1e-9);

        CallCost.OfChestOpen(partySize: 4, othersWithItems: 3, openerGotItem: true)
                .Should()
                .BeApproximately(1.2d, 1e-9);

        //three empty resends, the opener's among them, and one reopen of somebody else
        CallCost.OfChestOpen(partySize: 4, othersWithItems: 1, openerGotItem: false)
                .Should()
                .BeApproximately(0.7d, 1e-9);
    }

    private static long Ticks(double seconds) => (long)(seconds * Stopwatch.Frequency);
}
