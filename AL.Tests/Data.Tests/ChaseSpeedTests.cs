#region
using AL.Data;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     What a monster chases at once it holds a target. Only the monsters the design data names a charge for carry
///     one on the wire; the rest are filled in when the game loads G, and reading the raw field instead reports
///     every one of them as standing still.
/// </summary>
public class ChaseSpeedTests : GameDataTestBed
{
    /// <summary>The ladder the game applies, restated here rather than read back off the property it pins.</summary>
    [Test]
    public void AMonsterWithNoChargeOfItsOwnChasesAtAMultipleOfItsSpeed()
    {
        //a crab wanders at 6 and nothing in the data says otherwise, so the slowest rung applies: twice its speed
        var crab = GameData.Monsters["crab"]!;

        crab.ChargeSpeed
            .Should()
            .Be(0f);

        crab.ChaseSpeed
            .Should()
            .BeApproximately(crab.Speed * 2f, 0.001f);

        //and the rungs above it, each read off a monster whose speed lands on one
        foreach (var monster in GameData.Monsters.Values.Where(monster => monster.ChargeSpeed <= 0f))
        {
            var multiplier = monster.Speed switch
            {
                >= 60f => 1.20f,
                >= 50f => 1.30f,
                >= 32f => 1.4f,
                >= 20f => 1.6f,
                >= 10f => 1.7f,
                _      => 2f
            };

            monster.ChaseSpeed
                   .Should()
                   .BeApproximately(MathF.Floor((monster.Speed * multiplier) + 0.5f), 0.001f, $"{monster.Accessor} names no charge");
        }
    }

    /// <summary>Where the data does name one, it is used as it stands - the ladder is a fallback, not a scaling.</summary>
    [Test]
    public void AMonsterThatNamesAChargeChasesAtExactlyThat()
    {
        //a mole wanders at 18 and charges at 60, which is neither its speed nor any multiple of it on the ladder
        var mole = GameData.Monsters["mole"]!;

        mole.ChargeSpeed
            .Should()
            .BeGreaterThan(0f);

        mole.ChaseSpeed
            .Should()
            .Be(mole.ChargeSpeed);
    }
}
