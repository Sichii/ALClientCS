#region
using System.Collections.Concurrent;
using AL.Core.Extensions;
using Chaos.Time.Abstractions;
using FluentAssertions;
#endregion

namespace AL.Tests.Core.Tests;

/// <summary>
///     LINQ ToList on a ConcurrentDictionary reads Count then CopyTo as two operations. A writer between them
///     throws ArgumentException - the crash in ALClient.Update when a skill_timeout lands during the cooldown tick.
///     That is the ICollection&lt;KeyValuePair&gt; path only; the projectile sweep went through Values, which copies
///     under lock and was never at risk. These pin the in-place walk that replaced both.
/// </summary>
public class ConcurrentDictionaryExtensionsTests
{
    [Test]
    public void TickAndTryRemoveWhereDropsExpiredEntriesAndTicksTheRest()
    {
        var map = new ConcurrentDictionary<string, TickItem>
        {
            ["keep"] = new(100),
            ["drop"] = new(5)
        };

        map.TickAndTryRemoveWhere(TimeSpan.FromMilliseconds(10), item => item.Expired);

        map.ContainsKey("keep")
           .Should()
           .BeTrue();

        map.ContainsKey("drop")
           .Should()
           .BeFalse();

        map["keep"]
           .RemainingMs
           .Should()
           .Be(90);
    }

    [Test]
    public void TryRemoveWhereDropsMatchingEntriesWithoutASnapshot()
    {
        var map = new ConcurrentDictionary<string, int>
        {
            ["keep"] = 1,
            ["drop"] = 0
        };

        map.TryRemoveWhere(value => value == 0);

        map.Should()
           .ContainKey("keep");

        map.Should()
           .NotContainKey("drop");
    }

    /// <summary>
    ///     The walk has to tolerate the map growing and shrinking under it, which is what a socket write landing
    ///     mid-tick does. Driven from inside the predicate rather than from a second thread: a racing writer proves
    ///     the same thing only some of the time, and the two-second version of this cost a core and broke
    ///     <c>ClientTests.ShallowMergeIntoTest</c>'s wall-clock budget alongside it.
    /// </summary>
    [Test]
    public void TickAndTryRemoveWhereToleratesTheMapChangingDuringTheWalk()
    {
        var map = new ConcurrentDictionary<int, TickItem>();

        for (var i = 0; i < 64; i++)
            map[i] = new TickItem(1000);

        var written = 0;

        var walk = () => map.TickAndTryRemoveWhere(
            TimeSpan.FromMilliseconds(1),
            item =>
            {
                //bounded, or every entry the walk reaches adds another one and it never ends
                if (written < 64)
                {
                    map[10_000 + written] = new TickItem(1000);
                    map.TryRemove(written, out _);
                    written++;
                }

                return item.Expired;
            });

        walk.Should()
            .NotThrow();

        written.Should()
               .Be(64);
    }

    /// <summary>
    ///     TryRemove(kvp) compares the value, so what it protects is TValue's equality and not the walk's. Both
    ///     spellings are pinned because the production callers are split across them - Cooldowns is a class,
    ///     Conditions and Projectiles are records - and the remark on the extension says so.
    /// </summary>
    [Test]
    public void AReferenceEqualValueIsOnlyDroppedWhenItIsTheInstanceTicked()
    {
        var map = new ConcurrentDictionary<string, TickItem>
        {
            ["burn"] = new(5)
        };

        var replaced = new TickItem(5);

        map.TickAndTryRemoveWhere(
            TimeSpan.FromMilliseconds(10),
            item =>
            {
                //stands in for a socket write landing mid-walk: an equal-looking replacement under the same key
                map["burn"] = replaced;

                return item.Expired;
            });

        map.TryGetValue("burn", out var survivor)
           .Should()
           .BeTrue("a class compares by reference, so the replacement is not the instance that expired");

        survivor.Should()
                .BeSameAs(replaced);
    }

    [Test]
    public void ARecordValueIsDroppedByValueEqualityEvenWhenItIsADifferentInstance()
    {
        var map = new ConcurrentDictionary<string, TickRecord>
        {
            ["burn"] = new() { RemainingMs = 5 }
        };

        //what the ticked instance will look like after the walk subtracts 10 from it. Value equality cannot tell the
        //two apart, which is the whole point
        var replaced = new TickRecord { RemainingMs = -5 };

        map.TickAndTryRemoveWhere(
            TimeSpan.FromMilliseconds(10),
            item =>
            {
                map["burn"] = replaced;

                return item.Expired;
            });

        map.ContainsKey("burn")
           .Should()
           .BeFalse(
               "a record compares by value, so the replacement goes with the original - the extension's remark says "
               + "this, and the two record callers are safe only because neither writes single entries into a live map");
    }

    private sealed class TickItem(float remainingMs) : IDeltaUpdatable
    {
        public float RemainingMs { get; private set; } = remainingMs;

        public bool Expired => RemainingMs <= 0;

        public void Update(TimeSpan delta) => RemainingMs -= (float)delta.TotalMilliseconds;
    }

    private sealed record TickRecord : IDeltaUpdatable
    {
        public required float RemainingMs { get; set; }

        public bool Expired => RemainingMs <= 0;

        public void Update(TimeSpan delta) => RemainingMs -= (float)delta.TotalMilliseconds;
    }
}
