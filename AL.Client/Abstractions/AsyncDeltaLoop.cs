using System;
using System.Threading;
using System.Threading.Tasks;
using AL.Core.Helpers;

namespace AL.Client.Abstractions
{
    /// <summary>
    ///     Provides a basic implementation of a rate limited loop. <br />
    ///     Utilizes <see cref="DeltaTime" /> to keep the rate consistent.
    /// </summary>
    public abstract class AsyncDeltaLoop
    {
        /// <summary>
        ///     A lot of the data in <see cref="AL.Client.ALClient" /> is immutable, so we need an instance of the client instead
        ///     of passing a data object.
        /// </summary>
        protected readonly ALClient Client;
        private readonly object Sync;

        /// <summary>
        ///     The source of the cancellation token for the currently running loop. <br />
        ///     A new one is created when <see cref="Stop" /> is called.
        /// </summary>
        protected CancellationTokenSource Canceller;

        /// <summary>
        ///     Whether or not the loop is currently running.
        /// </summary>
        protected bool Running;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AsyncDeltaLoop" /> class.
        /// </summary>
        /// <param name="client">The instance of the <see cref="Client" /> this loop is for.</param>
        /// <exception cref="ArgumentNullException">client</exception>
        protected AsyncDeltaLoop(ALClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Canceller = new CancellationTokenSource();
            Sync = new object();
        }

        /// <summary>
        ///     A task representing the work to be done on every loop iteration.
        /// </summary>
        protected abstract Task DoWorkAsync();

        /// <summary>
        ///     Starts the loop.
        /// </summary>
        /// <param name="loopsPerSecond">The number of times per second to run the loop.</param>
        public async void Start(float loopsPerSecond)
        {
            lock (Sync)
            {
                if (Running)
                    return;

                Running = true;
            }

            var timePerLoop = 1000 / loopsPerSecond;

            while (!Canceller.IsCancellationRequested)
            {
                var delta = DeltaTime.Value;

                try
                {
                    await DoWorkAsync();
                } catch (Exception ex)
                {
                    Client.Error(ex);
                } finally
                {
                    var executionTime = DeltaTime.Value - delta;
                    var timeTillNextLoop = (int) (timePerLoop - executionTime);

                    if (timeTillNextLoop > 0)
                        await Task.Delay(timeTillNextLoop);
                }
            }
        }

        /// <summary>
        ///     Indicates to the loop that it should stop after the current iteration completes.
        /// </summary>
        public void Stop()
        {
            lock (Sync)
            {
                Running = false;
                Canceller.Cancel();
                Canceller = new CancellationTokenSource();
            }
        }
    }
}