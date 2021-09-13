using System;
using System.Threading;
using System.Threading.Tasks;
using AL.Core.Extensions;

namespace AL.Client.Helpers
{
    /// <summary>
    ///     Represents a delay that lasts a varying amount of time. Can be more OR less.
    /// </summary>
    /// <seealso cref="IDisposable" />
    internal class DynamicDelay : IDisposable
    {
        private readonly SemaphoreSlim Sync;
        private CancellationTokenSource Canceller;
        private int? Delay;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicDelay" /> class.
        /// </summary>
        internal DynamicDelay()
        {
            Sync = new SemaphoreSlim(1, 1);
            Canceller = new CancellationTokenSource();
        }

        public void Dispose()
        {
            try
            {
                GC.SuppressFinalize(this);

                Sync.Dispose();

                var canceller = Canceller;
                Canceller = null!;
                // ReSharper disable once ConstantConditionalAccessQualifier
                canceller?.Dispose();
            } catch
            {
                //ignored
            }
        }

        /// <summary>
        ///     Another way of cancelling the delay.
        /// </summary>
        internal void RequestCancellation()
        {
            try
            {
                Canceller.CancelWithAsynchronousContinuations();
            } catch
            {
                //ignored
            }
        }

        /// <summary>
        ///     Asynchronously sets a new delay by cancelling the previous delay and setting a new one.
        /// </summary>
        /// <param name="delay"></param>
        internal async Task SetDelayAsync(int delay)
        {
            var syncTask = Sync.WaitAsync();

            try
            {
                var canceller = Canceller;
                canceller.CancelWithAsynchronousContinuations();
                Canceller = new CancellationTokenSource();

                await syncTask.ConfigureAwait(false);
                Delay = delay;
                canceller.Dispose();
            } finally
            {
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
            token?.Register(RequestCancellation);

            while (true)
            {
                await Sync.WaitAsync().ConfigureAwait(false);

                try
                {
                    if (!Delay.HasValue || token is { IsCancellationRequested: true })
                        return;

                    try
                    {
                        await Task.Delay(Delay.Value, Canceller.Token).ConfigureAwait(false);
                    } catch
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