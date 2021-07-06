using System;
using System.Threading;
using System.Threading.Tasks;
using AL.Core.Helpers;

namespace AL.Client.Abstractions
{
    public abstract class AsyncManagerBase
    {
        protected readonly ALClient Client;
        protected CancellationTokenSource Canceller;
        protected bool Running;

        protected AsyncManagerBase(ALClient client)
        {
            Canceller = new CancellationTokenSource();
            Client = client;
        }

        protected abstract Task DoWorkAsync();

        public async void Start(float loopsPerSecond)
        {
            if (Running)
                return;

            Running = true;
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

        public void Stop()
        {
            Running = false;
            Canceller.Cancel();
            Canceller = new CancellationTokenSource();
        }
    }
}