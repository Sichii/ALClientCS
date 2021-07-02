using System.Threading.Tasks;
using AL.Client.Abstractions;
using AL.Core.Collections;
using AL.Core.Helpers;

namespace AL.Client.Managers
{
    public class PingManager : AsyncManagerBase
    {
        private readonly CyclicBuffer<int> Pings;
        private long PingCount;

        public PingManager(ALClient client)
            : base(client) =>
            Pings = new CyclicBuffer<int>(50);

        protected override async Task DoWorkAsync()
        {
            var delta = DeltaTime.Value;
            await Client.PingAsync(PingCount++);
            Pings.Add((int) (DeltaTime.Value - delta) / 2);
        }
    }
}