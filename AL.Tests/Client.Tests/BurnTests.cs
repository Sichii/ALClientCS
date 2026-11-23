#region
using AL.Client.Extensions;
using AL.Data;
using AL.SocketClient.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     A burn deals <c>ceil(intensity / 5)</c> per tick, and that divisor has nothing to do with the interval beside it -
///     the server hardcodes the one where the other is data. Deriving the tick from the interval instead reads as
///     equivalent, because the condition's own text calls it "damage equal to its intensity per second" and at 210ms that
///     is very nearly true.
///     <br />
///     The interval is not the tick rate either. The server polls conditions once per instance update rather than
///     scheduling them, and an instance updates no faster than 75ms, so a burn fires on every third update - 225ms, not
///     210. Both mistakes overstate a burn, and an overstated burn is a monster the combat lanes decline to shoot and then
///     have to kill anyway.
///     <br />
///     Pinned on a <see cref="Player" /> so it needs no monster data: the <c>Monster</c> arm of the check adds only the
///     1hp and self-healing bail-outs on top of this arithmetic.
/// </summary>
public class BurnTests : GameDataTestBed
{
    [Test]
    public void ABurnTicksAFifthOfItsIntensityAWholeUpdatePastItsInterval()
    {
        //the arithmetic below is written out against this, so a change to it should fail here rather than silently
        //move what the combat lanes decline to shoot
        GameData.Conditions
                .Burned
                .IntervalMS
                .Should()
                .Be(210);

        //210ms plus a 75ms update is 285, so 1100ms holds 3 whole ticks at ceil(1000 / 5) = 200 apiece - 600 to come
        Burning(599)
            .WillBurnToDeath()
            .Should()
            .BeTrue();

        //level with the total is not dead - the comparison is strictly greater
        Burning(600)
            .WillBurnToDeath()
            .Should()
            .BeFalse();

        //the band a 225ms tick got wrong: the server's updates run slower than 75ms, so its fourth tick may not come
        Burning(799)
            .WillBurnToDeath()
            .Should()
            .BeFalse();
    }

    /// <summary>
    ///     1100ms of a 1000 intensity burn. Written as literals rather than through the production formula - a probe that
    ///     computes its own expectation agrees with itself whatever it does.
    /// </summary>
    private static Player Burning(int hp)
        => TestJson.Socket<Player>(@"{""id"":""m1"",""hp"":" + hp + @",""s"":{""burned"":{""ms"":1100,""intensity"":1000}}}")!;
}