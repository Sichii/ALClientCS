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
///     These pin the in-place walk that replaced it.
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

    //two hot loops flat out for two seconds is the whole point of this one, and while it runs there is no core left
    //for anything else. ClientTests.ShallowMergeIntoTest asserts a wall-clock budget and fails alongside it
    [Test]
    [NotInParallel]
    public async Task TickAndTryRemoveWhereDoesNotThrowWhenWritersAddDuringTheWalk()
    {
        var map = new ConcurrentDictionary<int, TickItem>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var writer = Task.Run(() =>
        {
            var n = 0;

            while (!cts.IsCancellationRequested)
            {
                map[n] = new TickItem(1000);
                n++;

                if (n > 10_000)
                {
                    map.Clear();
                    n = 0;
                }
            }
        });

        Exception? thrown = null;

        try
        {
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    map.TickAndTryRemoveWhere(TimeSpan.FromMilliseconds(1), item => item.Expired);
                } catch (Exception ex)
                {
                    thrown = ex;

                    break;
                }
            }
        } finally
        {
            await cts.CancelAsync();
            await writer;
        }

        thrown.Should()
              .BeNull();
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
