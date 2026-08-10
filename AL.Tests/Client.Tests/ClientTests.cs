#region
using System.Diagnostics;
using System.Reflection;
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.Core.Attributes;
using AL.Core.Geometry;
using AL.Core.Helpers;
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using AL.Tests.Characterization;
using FluentAssertions;
using PATHFINDING_CONSTANTS = AL.Pathfinding.Definitions.CONSTANTS;
#endregion

namespace AL.Tests.Client.Tests;

public class ClientTests
{
    //a character frame carrying every movement key, plus two ordinary stats to prove the exclusion is the block
    //rather than the whole frame
    private const string MOVING_CHARACTER_FRAME
        = @"{""id"":""sichi"",""x"":500,""y"":-700,""going_x"":600,""going_y"":-700,""angle"":180,""move_num"":9,"
          + @"""moving"":true,""map"":""main"",""in"":""main"",""speed"":45,""max_hp"":7826}";

    //the filter RangerCombat picks its multishot targets through - a monster an in-flight arrow has already
    //killed is not worth a shot, because every shot spends the whole shared attack cooldown. Pinned on a plain
    //EntityBase so it needs no game data: the Monster arm adds only the _1hp bail-out on top of this arithmetic.
    [Test]
    public void WillDieToProjectilesCountsOnlyTheProjectilesAimedAtThatEntity()
    {
        static ActionData Shot(string targetId, float damage)
            => new()
            {
                Target = targetId,
                Damage = damage
            };

        var target = TestJson.Socket<Player>(@"{""id"":""m1"",""hp"":100}")!;

        //120 of committed damage against 100 hp, and the 0.95 haircut still clears it
        target.WillDieToProjectiles([Shot("m1", 60), Shot("m1", 60)])
              .Should()
              .BeTrue();

        //one arrow short - 57 does not, so the ranger keeps shooting it
        target.WillDieToProjectiles([Shot("m1", 60)])
              .Should()
              .BeFalse();

        //aimed at something else, so it counts for nothing here
        target.WillDieToProjectiles([Shot("m2", 500)])
              .Should()
              .BeFalse();
    }

    [Test]
    public void CalculateDamageMultiplierTest()
    {
        var defense = -160;
        var points = new List<Point>();

        for (; defense < 2000; defense += 5)
        {
            var damageMult = Utilities.CalculateDamageMultiplier(defense);
            points.Add(new Point(defense, Convert.ToInt32(damageMult * 100f)));
        }

        (points[0] == new Point(-160, 112)).Should()
                                           .BeTrue();

        (points.Last() == new Point(1995, 5)).Should()
                                             .BeTrue();
    }

    [Test]
    public async Task DynamicDelayTest()
    {
        var delay = new DynamicDelay();

        var delayTask = delay.WaitAsync(TimeSpan.FromMilliseconds(5000));

        await Task.Delay(2000);
        await delay.SetDelayAsync(TimeSpan.FromMilliseconds(10000));

        var ts = Stopwatch.GetTimestamp();
        await delayTask;
        var elapsed = Stopwatch.GetElapsedTime(ts);

        (elapsed.TotalMilliseconds > 9000).Should()
                                          .BeTrue();
    }

    [Test]
    public void ShallowMergeIntoTest()
    {
        var emptyCharacters = Enumerable.Range(0, 100000)
                                        .Select(_ => new Character())
                                        .ToArray();
        var obj = TestJson.Socket<CharacterData>(Fixture.ReadCommittedSnapshot("character-frame.json")!);

        var timer = Stopwatch.StartNew();
        var defaultBase = PATHFINDING_CONSTANTS.DEFAULT_BOUNDING_BASE;

        foreach (var emptyChar in emptyCharacters)
        {
            emptyChar.SetBoundingBase(defaultBase);
            ShallowMerge<Character>.Merge(obj!, emptyChar);

            //cast to object: Character is IEnumerable<IPoint>, so an uncast Should() binds to collection assertions.
            //EntityBase.Equals is id-only, so this says the merge carried the id across and nothing more
            ((object)emptyChar).Should()
                               .Be(obj);

            //the movement block is excluded, so the frame's position stays behind. ShallowMergeLeavesTheMovementBlockAlone
            //is where that is actually pinned; here it only keeps the perf loop honest about what it merges
            emptyChar.X
                     .Should()
                     .Be(0f);

            defaultBase.HalfWidth
                       .Should()
                       .Be(emptyChar.HalfWidth);

            defaultBase.VerticalNorth
                       .Should()
                       .Be(emptyChar.VerticalNorth);

            defaultBase.VerticalNotNorth
                       .Should()
                       .Be(emptyChar.VerticalNotNorth);
        }

        timer.Stop();
        var elapsed = timer.ElapsedMilliseconds;

        //this takes like 60ms on my machine. if this goes above 500 on any machine, there must be a problem.
        (elapsed < 500).Should()
                       .BeTrue();
    }

    /// <summary>
    ///     No member of the movement block is assignable by the shallow merge, so that every write to one goes through
    ///     the single locked path on <see cref="EntityBase" />. What excludes them today is the accessor rather than
    ///     the attribute: the merge reflects
    ///     <c>
    ///         typeof(Character)
    ///     </c>
    ///     , and a private setter declared on <see cref="EntityBase" /> is not inherited, so
    ///     <c>
    ///         CanWrite
    ///     </c>
    ///     already reads false from there. <see cref="ShallowMergeIgnoreAttribute" /> is the guard for the two cases
    ///     the accessor cannot cover - a block property declared on <see cref="Character" /> itself, and a setter
    ///     someone later widens back.
    /// </summary>
    /// <remarks>
    ///     The merge's selection rule is restated here rather than called, because a wrong rule asked about itself
    ///     agrees with itself. The block is read off <see cref="MovementBlock" />'s own members, so a value added
    ///     there is guarded the moment it is added.
    /// </remarks>
    [Test]
    public void ShallowMergeLeavesTheMovementBlockAlone()
    {
        var block = typeof(MovementBlock).GetProperties()
                                         .Select(property => property.Name)
                                         .ToArray();

        block.Should()
             .NotBeEmpty("a block nobody can name is a guard that cannot fail");

        //ShallowMerge<T>.IsMergeable, restated
        var mergeable = typeof(Character).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                         .Where(
                                             property => property.CanRead
                                                         && property.CanWrite
                                                         && !property.IsDefined(typeof(ShallowMergeIgnoreAttribute), true))
                                         .Select(property => property.Name)
                                         .ToArray();

        mergeable.Should()
                 .NotBeEmpty("the merge still has to carry everything outside the block")
                 .And.NotIntersectWith(
                     block,
                     "a movement property the merge can assign is a write to the block that never takes the lock");

        //and the outcome that rule exists for
        var frame = TestJson.Socket<CharacterData>(MOVING_CHARACTER_FRAME)!;

        var target = new Character();
        target.UpdateLocation(new Point(10, -20));

        ShallowMerge<Character>.Merge(frame, target);

        target.X
              .Should()
              .Be(10f, "the frame's position must not reach the entity through the merge");

        target.Y
              .Should()
              .Be(-20f);

        target.GoingX
              .Should()
              .Be(0f);

        target.GoingY
              .Should()
              .Be(0f);

        target.Angle
              .Should()
              .Be(0f);

        target.MoveNum
              .Should()
              .Be(0UL);

        target.Moving
              .Should()
              .BeFalse();

        target.Map
              .Should()
              .BeNull("a position is only coherent with the map it was read beside, so the map is in the block too");

        target.In
              .Should()
              .BeNull();

        //everything outside the block still merges, so the exclusion is the block and not the frame
        target.Speed
              .Should()
              .Be(45f);

        target.MaxHP
              .Should()
              .Be(7826f);
    }

    /// <summary>
    ///     The other half of the exclusion above: a character frame's position still reaches the live character,
    ///     through the choke point instead of the merge. Pinned on
    ///     <see cref="EntityBase.AcceptMovement" /> directly because the call site
    ///     (<c>
    ///         ALClient.OnCharacterAsync
    ///     </c>
    ///     ) needs a live socket connection.
    /// </summary>
    [Test]
    public void AcceptMovementTakesTheFrameAsSent()
    {
        var frame = TestJson.Socket<CharacterData>(MOVING_CHARACTER_FRAME)!;

        var target = new Character();
        target.UpdateLocation(new Point(10, -20));
        target.SetMoving(new Point(11, -20));

        target.AcceptMovement(frame);

        //no arbitration - the frame wins outright, exactly as the merge used to make it
        target.X
              .Should()
              .Be(500f);

        target.Y
              .Should()
              .Be(-700f);

        target.GoingX
              .Should()
              .Be(600f);

        target.GoingY
              .Should()
              .Be(-700f);

        target.Angle
              .Should()
              .Be(180f);

        target.MoveNum
              .Should()
              .Be(9UL);

        target.Moving
              .Should()
              .BeTrue();

        target.Map
              .Should()
              .Be("main");

        target.In
              .Should()
              .Be("main");
    }

    /// <summary>
    ///     Why <see cref="EntityBase.AcceptMovement" /> takes a character frame wholesale where
    ///     <see cref="EntityBase.Update(EntityBase)" /> gates an entities frame on
    ///     <see cref="EntityBase.PresentFields" />. The server sends a key only where its player object has one
    ///     (<c>player_to_client</c>, <c>node/server.js:778</c>), and a player object has no walking keys at all until
    ///     its first move of the session - so a login or reconnect frame carries the position and the map and none of
    ///     the walk. Gated, such a frame would leave a persistent character still walking to the destination of a leg
    ///     on the server it just left.
    /// </summary>
    /// <remarks>
    ///     Both committed captures are real character frames, so this also pins the half the gate would rest on:
    ///     presence is captured on this route at all.
    /// </remarks>
    [Test]
    [Arguments("character-frame.json")]
    [Arguments("t11-start-frame.json")]
    public void ACharacterFrameCarriesNoWalkUntilTheCharacterHasWalked(string snapshot)
    {
        var frame = TestJson.Socket<CharacterData>(Fixture.ReadCommittedSnapshot(snapshot)!)!;

        frame.PresentFields
             .Should()
             .HaveFlag(EntityUpdateField.X)
             .And.HaveFlag(EntityUpdateField.Y)
             .And.HaveFlag(EntityUpdateField.Map)
             .And.HaveFlag(EntityUpdateField.In);

        frame.PresentFields
             .Should()
             .NotHaveFlag(EntityUpdateField.Moving)
             .And.NotHaveFlag(EntityUpdateField.GoingX)
             .And.NotHaveFlag(EntityUpdateField.GoingY)
             .And.NotHaveFlag(EntityUpdateField.Angle)
             .And.NotHaveFlag(EntityUpdateField.MoveNum);
    }

    /// <summary>
    ///     A character frame reaches <c>OnCharacterAsync</c> by three routes - the <c>player</c> subscription, the
    ///     <c>start</c> frame, and the character nested inside the welcome - and only the first two are a root
    ///     deserialize. The nested one is the route worth pinning: presence is captured by the converter the
    ///     attributed factory produces, and the factory has to still claim a member of an outer object it is not
    ///     itself claiming.
    /// </summary>
    [Test]
    public void PresenceIsCapturedOnACharacterNestedInsideAWelcome()
    {
        var welcome = TestJson.Socket<WelcomeData>($@"{{""map"":""main"",""in"":""main"",""character"":{MOVING_CHARACTER_FRAME}}}")!;

        welcome.Character
               .Should()
               .NotBeNull();

        welcome.Character!.PresentFields
               .Should()
               .HaveFlag(EntityUpdateField.X)
               .And.HaveFlag(EntityUpdateField.Moving)
               .And.HaveFlag(EntityUpdateField.MoveNum)
               .And.HaveFlag(EntityUpdateField.Map);
    }
}