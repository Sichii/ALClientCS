#region
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
        var currentDelay = delay;

        while (true)
        {
            CancellationTokenSource localCtx;

            await using (await Sync.WaitAsync())
            {
                localCtx = token.HasValue ? CancellationTokenSource.CreateLinkedTokenSource(token.Value) : new CancellationTokenSource();
                Ctx = localCtx;

                //consume any delay set while we were waiting, so each iteration restarts from now
                currentDelay = Delay ?? currentDelay;
                Delay = null;
                NewDelay = false;
            }

            //being cancelled is this delay's ordinary outcome rather than a fault: SetDelayAsync cancels and
            //replaces it on every position update, so awaiting it directly threw and caught about six times a
            //second across a squad in motion. A cancelled task carries its cancellation as status and only
            //materializes the exception when awaited, so reading that status through a continuation never throws
            var elapsed = await Task.Delay(currentDelay, localCtx.Token)
                                    .ContinueWith(
                                        static delayed => delayed.IsCompletedSuccessfully,
                                        TaskContinuationOptions.ExecuteSynchronously);

            if (elapsed)
                break;

            await using (await Sync.WaitAsync())
            {
                //the delay was cancelled and nothing replaced it, so the wait is over
                if (!NewDelay)
                    break;
            }
        }
    }
}