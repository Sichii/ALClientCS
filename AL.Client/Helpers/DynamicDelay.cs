using System;
using System.Threading;
using System.Threading.Tasks;

namespace AL.Client.Helpers
{
    /// <summary>
    ///     Represents a delay that lasts a varying amount of time. Can be more OR less.
    /// </summary>
    /// <seealso cref="IDisposable" />
    internal class DynamicDelay : IDisposable
    {
        private readonly SemaphoreSlim Sync;
        private CancellationTokenSource? Canceller;
        private int? Delay;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicDelay" /> class.
        /// </summary>
        internal DynamicDelay() => Sync = new SemaphoreSlim(1, 1);

        /// <summary>
        ///     Another way of cancelling the delay.
        /// </summary>
        internal void Cancel()
        {
            try
            {
                Canceller?.Cancel();
            } catch
            {
                //ignored
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            Sync.Dispose();

            var canceller = Canceller;
            Canceller = null;
            canceller?.Dispose();
        }

        /// <summary>
        ///     Asynchronously sets a new delay by cancelling the previous delay and setting a new one.
        /// </summary>
        /// <param name="delay"></param>
        internal async Task SetDelayAsync(int delay)
        {
            var syncTask = Sync.WaitAsync();
            Cancel();
            await syncTask.ConfigureAwait(false);

            try
            {
                Delay = delay;
            } finally
            {
                var canceller = Canceller;
                Canceller = null;
                canceller?.Dispose();
                Sync.Release();
            }
        }

        /// <summary>
        ///     Asynchronously waits for the specified amount of time. Change that amount by calling <see cref="SetDelayAsync" />.
        /// </summary>
        /// <param name="initialDelay">The initial delay to wait for.</param>
        /// <param name="token">A token to cancel the delay.</param>
        internal async Task WaitAsync(int initialDelay, CancellationToken? token = null)
        {
            Delay = initialDelay;
            token?.Register(Cancel);

            while (true)
            {
                await Sync.WaitAsync().ConfigureAwait(false);

                try
                {
                    if (!Delay.HasValue)
                        return;

                    Canceller = new CancellationTokenSource();

                    try
                    {
                        await Task.Delay(Delay.Value, Canceller.Token).ConfigureAwait(false);
                    } catch (TaskCanceledException)
                    {
                        //ignored
                    }
                } finally
                {
                    Delay = null;
                    Sync.Release();
                }
            }
        }
    }
}