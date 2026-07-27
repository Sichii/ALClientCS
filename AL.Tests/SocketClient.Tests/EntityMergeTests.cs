#region
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     The entity merge is a key-presence merge: a delta only overwrites the fields it actually carried. The server's
///     encoder is sparse (a soft property equal to the G default is omitted), so a whitelist merge that copies every
///     property unconditionally zeros a live monster's hp/speed on the next position-only frame.
/// </summary>
public class EntityMergeTests
{
    /// <summary>
    ///     The server sends level only when it exceeds 1, so an undamaged level-1 monster omits it. Absent must resolve to 1
    ///     (the monster level floor), not the int default 0 - the phase goal names level explicitly.
    /// </summary>
    [Test]
    public void AbsentLevelBackfillsToOneNotZero()
    {
        var monster = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""type"":""goo"", ""x"":0, ""y"":0 }");

        monster.Should()
               .NotBeNull();

        monster.Level
               .Should()
               .Be(0);

        monster.BackfillSoftDefault(EntityUpdateField.Level, 1);

        monster.Level
               .Should()
               .Be(1);
    }

    [Test]
    public void BackfillFillsAStatTheFrameOmitted()
    {
        //a freshly-sighted, undamaged monster carries no hp key because it equals the def
        var monster = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""type"":""goo"", ""x"":0, ""y"":0 }");

        monster.Should()
               .NotBeNull();

        monster.HP
               .Should()
               .Be(0f);

        monster.BackfillSoftDefault(EntityUpdateField.HP, 500);
        monster.BackfillSoftDefault(EntityUpdateField.Speed, 40);

        monster.HP
               .Should()
               .Be(500f);

        monster.Speed
               .Should()
               .Be(40f);
    }

    [Test]
    public void BackfillNeverOverridesAValueTheFrameCarried()
    {
        //a damaged monster DOES carry hp; the def must not clobber it back to full
        var monster = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""type"":""goo"", ""hp"":137 }");

        monster.Should()
               .NotBeNull();

        monster.BackfillSoftDefault(EntityUpdateField.HP, 500);

        monster.HP
               .Should()
               .Be(137f);
    }

    [Test]
    public void MergingAcrossIdsThrows()
    {
        var a = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""type"":""goo"" }");
        var b = TestJson.Socket<Monster>(@"{ ""id"":""goo2"", ""type"":""goo"" }");

        a.Should()
         .NotBeNull();

        b.Should()
         .NotBeNull();

        FluentActions.Invoking(() => a.Update(b))
                     .Should()
                     .ThrowExactly<InvalidOperationException>();
    }

    /// <summary>
    ///     The regression the unconditional whitelist caused: a bare position delta must leave hp/speed intact.
    /// </summary>
    [Test]
    public void PositionOnlyDeltaLeavesLiveStatsUntouched()
    {
        var live = TestJson.Socket<Monster>(
            @"{ ""id"":""goo1"", ""type"":""goo"", ""x"":10, ""y"":20, ""hp"":500, ""max_hp"":500, ""speed"":40, ""attack"":18 }");
        var delta = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""x"":12, ""y"":20 }");

        live.Should()
            .NotBeNull();

        delta.Should()
             .NotBeNull();

        live.Update(delta);

        //position adopted from the delta
        live.X
            .Should()
            .Be(12f);

        //stats the delta omitted are preserved, not zeroed
        live.HP
            .Should()
            .Be(500f);

        live.Speed
            .Should()
            .Be(40f);

        live.Attack
            .Should()
            .Be(18f);

        live.MaxHP
            .Should()
            .Be(500f);
    }

    [Test]
    public void PresentFieldsRecordsExactlyTheKeysTheFrameCarried()
    {
        const string FULL
            = @"{ ""id"":""goo1"", ""type"":""goo"", ""x"":10, ""y"":20, ""hp"":500, ""max_hp"":500, ""speed"":40, ""attack"":18 }";

        var monster = TestJson.Socket<Monster>(FULL);

        monster.Should()
               .NotBeNull();

        ((monster.PresentFields & EntityUpdateField.HP) != 0).Should()
                                                             .BeTrue();

        ((monster.PresentFields & EntityUpdateField.Speed) != 0).Should()
                                                                .BeTrue();

        ((monster.PresentFields & EntityUpdateField.X) != 0).Should()
                                                            .BeTrue();

        //keys the frame did not carry must not be flagged
        ((monster.PresentFields & EntityUpdateField.MP) != 0).Should()
                                                             .BeFalse();

        ((monster.PresentFields & EntityUpdateField.Level) != 0).Should()
                                                                .BeFalse();
    }

    [Test]
    public void PresentLevelIsNotOverwrittenByTheBackfillFloor()
    {
        //a level-8 monster sends level (>1), so the floor must not stomp it back to 1
        var monster = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""type"":""goo"", ""level"":8 }");

        monster.Should()
               .NotBeNull();

        monster.BackfillSoftDefault(EntityUpdateField.Level, 1);

        monster.Level
               .Should()
               .Be(8);
    }

    [Test]
    public void PresentStatIsOverwrittenByTheDelta()
    {
        var live = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""type"":""goo"", ""x"":10, ""y"":20, ""hp"":500 }");
        var delta = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""hp"":320 }");

        live.Should()
            .NotBeNull();

        delta.Should()
             .NotBeNull();

        live.Update(delta);

        //hp WAS in the delta, so it is adopted
        live.HP
            .Should()
            .Be(320f);
    }
}