using System.Linq;
using System.Threading.Tasks;
using AL.Client.Abstractions;
using AL.Core.Collections;
using AL.Core.Helpers;

namespace AL.Client.Managers
{
    internal sealed class PingManager : AsyncDeltaLoop
    {
        private readonly CyclicBuffer<int> Pings;
        private int _offset;
        public long PingCount;

        internal int Offset => Pings.Count < 10 ? 0 : _offset;

        internal PingManager(ALClient client)
            : base(client) =>
            Pings = new CyclicBuffer<int>(50);

        protected override async Task DoWorkAsync()
        {
            var delta = DeltaTime.Value;
            await Client.PingAsync(PingCount++);
            var jitter = (int) (DeltaTime.Value - delta) / 2;

            if (jitter > _offset)
                _offset = jitter;

            var old = Pings.Add(jitter);

            if (old == _offset)
                _offset = Pings.Max();
        }
    }
}