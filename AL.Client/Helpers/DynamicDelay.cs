#region
using System;
using System.Threading;
using System.Threading.Tasks;
using Chaos.Common.Synchronization;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Represents a delay that lasts a varying amount of time. Can be more OR less.
/// </summary>
/// <seealso cref="IDisposable" />
public sealed class DynamicDelay
{
    private readonly FifoAutoReleasingSemaphoreSlim Sync;
    private CancellationTokenSource Ctx;
    private TimeSpan? Delay;
    private bool NewDelay;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DynamicDelay" /> class.
    /// </summary>
    internal DynamicDelay()
    {
        Sync = new FifoAutoReleasingSemaphoreSlim(1, 1);
        Ctx = new CancellationTokenSource();
    }

    /// <summary>
    ///     Another way of cancelling the delay.
    /// </summary>
    internal void RequestCancellation() => Ctx.Cancel();

    /// <summary>
    ///     Asynchronously sets a new delay by cancelling the previous delay and setting a new one.
    /// </summary>
    /// <param name="delay">
    /// </param>
    internal async Task SetDelayAsync(TimeSpan delay)
    {
        await using var @lock = await Sync.WaitAsync();

        await Ctx.CancelAsync();
        Delay = delay;
        NewDelay = true;
    }

    /// <summary>
    ///     Asynchronously waits for the specified amount of time. Change that amount by calling <see cref="SetDelayAsync" />.
    /// </summary>
    /// <param name="delay">
    ///     The initial delay to wait for.
    /// </param>
    /// <param name="token">
    ///     A token to cancel the delay.
    /// </param>
    internal async Task WaitAsync(TimeSpan delay, CancellationToken? token = null)
    {
        while (true)
        {
            CancellationTokenSource localCtx;

            await using (await Sync.WaitAsync())
            {
                localCtx = token.HasValue ? CancellationTokenSource.CreateLinkedTokenSource(token.Value) : new CancellationTokenSource();
                Ctx = localCtx;
                Delay ??= delay;
                NewDelay = false;
            }

            try
            {
                await Task.Delay(delay, localCtx.Token);

                break;
            } catch (TaskCanceledException)
            {
                await using var @lock = await Sync.WaitAsync();

                //if the delay was canceled and no delay was set, we're done
                if (!NewDelay)
                    break;
            }
        }
    }
}