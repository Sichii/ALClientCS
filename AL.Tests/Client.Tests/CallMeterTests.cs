#region
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
    public void AnEmitCostsTheBaseChargePlusItsSurcharge()
    {
        //every handler is charged 1 for the call itself before its method is looked up at all
        CallCost.Of(ALSocketEmitType.Attack)
                .Should()
                .Be(CallCost.BASE);

        //cruise is the expensive one a movement lane can emit on a loop, so it is the one worth pinning by name
        CallCost.Of(ALSocketEmitType.Cruise)
                .Should()
                .Be(11d);

        CallCost.Of(ALSocketEmitType.Move)
                .Should()
                .Be(2.5d);

        CallCost.Of(ALSocketEmitType.Tracker)
                .Should()
                .Be(51d);
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

        //11 + 2.5 + 1
        snapshot.Cost
                .Should()
                .Be(14.5d);

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
}
