#region
using AL.Data;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     The hit boxes every range check is resolved against, pinned to the real dimensions table rather than to the
///     arithmetic that builds them. These are the numbers the server sizes its own boxes from, so a change here is a
///     change to whether attacks land.
/// </summary>
public class BoundingBaseTests : GameDataTestBed
{
    /// <summary>
    ///     Boxes are centred horizontally but rise from the entity's feet, so the whole height sits on one side of the
    ///     centre point and nothing on the other. Getting that backwards halves the vertical reach and leaves the
    ///     horizontal one untouched, which is the sort of error only a fixed number catches.
    /// </summary>
    [Test]
    public void ACharacterIsSizedFromTheFixedPlayerBox()
    {
        //26 wide and 36 tall for everyone, and deliberately not the 35 the dimensions table carries for the same entry
        GameData.DEFAULT_CHARACTER_HIT_BOX
                .HalfWidth
                .Should()
                .BeApproximately(13f, 0.001f);

        GameData.DEFAULT_CHARACTER_HIT_BOX
                .VerticalNorth
                .Should()
                .BeApproximately(36f, 0.001f);

        GameData.DEFAULT_CHARACTER_HIT_BOX
                .VerticalNotNorth
                .Should()
                .Be(0f);
    }

    /// <summary>
    ///     A monster is sized the same way from its own entry, and from the raw width and height rather than the
    ///     scaled-down pair the nav mesh is padded with - those are two different boxes for two different jobs, and
    ///     only this one decides what is in range.
    /// </summary>
    [Test]
    public void AMonsterIsSizedFromItsOwnDimensions()
    {
        //mole is 30 wide and 20 tall
        var mole = GameData.Monsters["mole"];

        mole.Should()
            .NotBeNull();

        mole!.HitBox
             .HalfWidth
             .Should()
             .BeApproximately(15f, 0.001f);

        mole.HitBox
            .VerticalNorth
            .Should()
            .BeApproximately(20f, 0.001f);

        mole.HitBox
            .VerticalNotNorth
            .Should()
            .Be(0f);

        //and its collision foot-print is a different, much smaller box that this must not have disturbed
        mole.BoundingBase
            .HalfWidth
            .Should()
            .BeApproximately(12f, 0.001f);
    }

    /// <summary>
    ///     A monster with no entry in the dimensions table is squared off at 24 rather than left without a box, and
    ///     the handful carrying a size multiplier are scaled and rounded before anything measures them. A crab is half
    ///     size, so this is worth several units on the most common target in the game.
    /// </summary>
    [Test]
    public void AMonsterIsScaledBySizeAndSquaredOffWhenUnlisted()
    {
        //a crab is in neither camp by halves: it has no entry at all, so it squares off at 24, and it carries size
        //0.5, so it then halves to 12 by 12
        var crab = GameData.Monsters["crab"];

        crab!.HitBox
             .HalfWidth
             .Should()
             .BeApproximately(6f, 0.001f);

        crab.HitBox
            .VerticalNorth
            .Should()
            .BeApproximately(12f, 0.001f);

        //every monster ends up with a box, listed or not
        foreach ((_, var monster) in GameData.Monsters.Entries)
            monster.HitBox
                   .HalfWidth
                   .Should()
                   .BeGreaterThan(0f);
    }
}
