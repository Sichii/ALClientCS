#region
using System.Diagnostics;
using AL.Client.Helpers;
using AL.Core.Geometry;
using AL.Core.Helpers;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using AL.Tests.Characterization;
using FluentAssertions;
using PATHFINDING_CONSTANTS = AL.Pathfinding.Definitions.CONSTANTS;
#endregion

namespace AL.Tests.Client.Tests;

public class ClientTests
{
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

            //cast to object: Character is IEnumerable<IPoint>, so an uncast Should() binds to collection assertions
            ((object)emptyChar).Should()
                               .Be(obj);

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
}