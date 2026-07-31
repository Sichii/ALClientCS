#region
using Chaos.Common.Synchronization;
using Chaos.Time;
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
    ///     Cancelled by <see cref="StopAsync" /> and replaced by <see cref="Start" />, so a loop that has been
    ///     stopped can be started again.
    /// </summary>
    protected CancellationTokenSource Ctx { get; private set; }

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
        ArgumentNullException.ThrowIfNull(client);

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
            //StopAsync cancels this source and nothing else replaces it, so a restart - which is what a reconnect
            //does, having stopped the loops on the way in - would otherwise fall straight out of the while below.
            //The stale source is left to the GC rather than disposed: a StopAsync may still be inside CancelAsync
            if (Ctx.IsCancellationRequested)
                Ctx = new CancellationTokenSource();

            //captured, so a later restart swapping the field cannot resurrect this iteration
            var ctx = Ctx;
            var deltaTime = new DeltaTime();
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000 / PollingRate));

            while (!ctx.Token.IsCancellationRequested)
            {
                await timer.WaitForNextTickAsync(ctx.Token);
                var delta = deltaTime.GetDelta;

                try
                {
                    await using var @lock = await Sync.WaitAsync();

                    if (ctx.IsCancellationRequested)
                        return;

                    await DoWorkAsync(delta, ctx.Token);
                } catch (Exception ex)
                {
                    Client.Logger.Error(ex);
                }
            }
        } catch
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