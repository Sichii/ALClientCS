using System;
using System.Threading;
using System.Threading.Tasks;
using AL.Core.Extensions;
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
        ///     A lot of the data in <see cref="AL.Client.ALClient" /> is immutable, an instance of the client is needed instead
        ///     of passing a data object.
        /// </summary>
        protected readonly ALClient Client;
        private readonly SemaphoreSlim Sync;

        /// <summary>
        ///     The source of the cancellation token for the currently running loop. <br />
        ///     A new one is created when <see cref="StopAsync" /> is called.
        /// </summary>
        protected CancellationTokenSource Canceller;

        /// <summary>
        ///     Whether or not the loop is currently running.
        /// </summary>
        protected bool Running;

        protected abstract float PollingRate { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AsyncDeltaLoop" /> class.
        /// </summary>
        /// <param name="client">The instance of the <see cref="Client" /> this loop is for.</param>
        /// <exception cref="ArgumentNullException">client</exception>
        protected AsyncDeltaLoop(ALClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Canceller = new CancellationTokenSource();
            Sync = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        ///     A task representing the work to be done on every loop iteration.
        /// </summary>
        protected abstract Task DoWorkAsync();

        /// <summary>
        ///     Starts the loop.
        /// </summary>
        public async void Start()
        {
            await Sync.WaitAsync().ConfigureAwait(false);

            try
            {
                if (Running)
                    return;

                Running = true;
            } finally
            {
                Sync.Release();
            }

            while (!Canceller.Token.IsCancellationRequested)
            {
                var delta = DeltaTime.Value;

                try
                {
                    await DoWorkAsync().ConfigureAwait(false);
                } catch (Exception ex)
                {
                    Client.Logger.Error(ex);
                } finally
                {
                    var executionTime = (int)(DeltaTime.Value - delta);
                    var timeTillNextLoop = Convert.ToInt32(1000 / PollingRate) - executionTime;

                    if (timeTillNextLoop > 0)
                        await Task.Delay(timeTillNextLoop).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        ///     Asynchronously stops the loop, returning after cancellation is requested on any running tasks, but not waiting for
        ///     cancellation to complete.
        /// </summary>
        public async Task StopAsync()
        {
            await Sync.WaitAsync().ConfigureAwait(false);

            try
            {
                Running = false;
                var canceller = Canceller;
                canceller.CancelWithAsynchronousContinuations();
                Canceller = new CancellationTokenSource();
                canceller.Dispose();
            } finally
            {
                Sync.Release();
            }
        }
    }
}