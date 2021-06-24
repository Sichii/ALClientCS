using System;
using System.Threading;
using System.Threading.Tasks;
using AL.Core.Helpers;

namespace ALClientCS.Abstractions
{
    public abstract class AsyncManagerBase
    {
        protected readonly ALClient Client;
        protected bool Running;
        protected CancellationTokenSource Canceller; 
        
        protected AsyncManagerBase(ALClient client) => Client = client;

        public async void Start(int loopsPerSecond)
        {
            if (Running)
                return;
            
            Running = true;
            var timePerLoop = 1000 / loopsPerSecond;
            var delta = DeltaTime.Value;

            while (!Canceller.IsCancellationRequested)
            {
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

        protected abstract ValueTask DoWorkAsync();

        public void Stop()
        {
            Running = false;
            Canceller.Cancel();
            Canceller = new CancellationTokenSource();
        }
    }
}