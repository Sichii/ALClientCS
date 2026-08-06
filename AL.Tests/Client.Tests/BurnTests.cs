#region
using AL.Client.Extensions;
using AL.Data;
using AL.SocketClient.Model;
using AL.Tests.Characterization;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     A burn deals <c>ceil(intensity / 5)</c> per tick, and that divisor has nothing to do with the interval beside
///     it - the server hardcodes the one where the other is data. Deriving the tick from the interval instead reads
///     as equivalent, because the condition's own text calls it "damage equal to its intensity per second" and at
///     210ms that is very nearly true.
///     <br />
///     The interval is not the tick rate either. The server polls conditions once per instance update rather than
///     scheduling them, and an instance updates no faster than 75ms, so a burn fires on every third update - 225ms,
///     not 210. Both mistakes overstate a burn, and an overstated burn is a monster the combat lanes decline to
///     shoot and then have to kill anyway.
///     <br />
///     Pinned on a <see cref="Player" /> so it needs no monster data: the <c>Monster</c> arm of the check adds only
///     the 1hp and self-healing bail-outs on top of this arithmetic.
/// </summary>
public class BurnTests : GameDataTestBed
{
    //1100ms of a 1000 intensity burn. Written as literals rather than through the production formula - a probe
    //that computes its own expectation agrees with itself whatever it does
    private static Player Burning(int hp)
        => TestJson.Socket<Player>(@"{""id"":""m1"",""hp"":" + hp + @",""s"":{""burned"":{""ms"":1100,""intensity"":1000}}}")!;

    [Test]
    public void ABurnTicksAFifthOfItsIntensityOnTheInstanceUpdateGrid()
    {
        //the arithmetic below is written out against this, so a change to it should fail here rather than silently
        //move what the combat lanes decline to shoot
        GameData.Conditions
                .Burned
                .IntervalMS
                .Should()
                .Be(210);

        //210ms first clears on the third 75ms update, so the real tick is 225 and 1100ms holds 4 whole ones at
        //ceil(1000 / 5) = 200 apiece - exactly 800 damage still to come
        Burning(799)
            .WillBurnToDeath()
            .Should()
            .BeTrue();

        //level with the total is not dead - the comparison is strictly greater
        Burning(800)
            .WillBurnToDeath()
            .Should()
            .BeFalse();

        //the band a 210ms tick got wrong: it fits a fifth tick into the same 1100ms and calls this one a corpse
        Burning(999)
            .WillBurnToDeath()
            .Should()
            .BeFalse();

        //and the band the interval-derived damage got wrong on top of it: 5 ticks of 210 came to 1050
        Burning(1020)
            .WillBurnToDeath()
            .Should()
            .BeFalse();
    }
}
