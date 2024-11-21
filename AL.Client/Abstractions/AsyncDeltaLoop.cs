#region
using System;
using System.Threading;
using System.Threading.Tasks;
using AL.Core.Helpers;
using Chaos.Common.Synchronization;
using Chaos.Time;
using Chaos.Time.Abstractions;
#endregion

namespace AL.Client.Abstractions;

/// <summary>
///     Provides a basic implementation of a rate limited loop.
///     <br />
///     Utilizes <see cref="DeltaTime" /> to keep the rate consistent.
/// </summary>
public abstract class AsyncDeltaLoop
{
    /// <summary>
    ///     A lot of the data in <see cref="AL.Client.ALClient" /> is immutable, an instance of the client is needed instead of
    ///     passing a data object.
    /// </summary>
    protected readonly ALClient Client;

    private readonly FifoAutoReleasingSemaphoreSlim Sync;

    /// <summary>
    ///     The source of the cancellation token for the currently running loop.
    ///     <br />
    ///     A new one is created when <see cref="StopAsync" /> is called.
    /// </summary>
    protected CancellationTokenSource Ctx { get; }

    protected abstract float PollingRate { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncDeltaLoop" /> class.
    /// </summary>
    /// <param name="client">
    ///     The instance of the <see cref="Client" /> this loop is for.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     client
    /// </exception>
    protected AsyncDeltaLoop(ALClient client)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));

        Client = client;
        Ctx = new CancellationTokenSource();
        Sync = new FifoAutoReleasingSemaphoreSlim(1, 1);
    }

    /// <summary>
    ///     A task representing the work to be done on every loop iteration.
    /// </summary>
    protected abstract Task DoWorkAsync(TimeSpan delta, CancellationToken cancellationToken);

    /// <summary>
    ///     Starts the loop.
    /// </summary>
    public async void Start()
    {
        try
        {
            var deltaTime = new DeltaTime();
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000 / PollingRate));

            while (!Ctx.Token.IsCancellationRequested)
            {
                await timer.WaitForNextTickAsync(Ctx.Token);
                var delta = deltaTime.GetDelta;

                try
                {
                    await using var @lock = await Sync.WaitAsync();

                    if (Ctx.IsCancellationRequested)
                        return;

                    await DoWorkAsync(delta, Ctx.Token);
                } catch (Exception ex)
                {
                    Client.Logger.Error(ex);
                }
            }
        } catch (Exception e)
        {
            //ignored
        }
    }

    /// <summary>
    ///     Asynchronously stops the loop, returning after cancellation is requested on any running tasks, but not waiting for
    ///     cancellation to complete.
    /// </summary>
    public async Task StopAsync()
    {
        await using var @lock = await Sync.WaitAsync();

        await Ctx.CancelAsync();
    }
}