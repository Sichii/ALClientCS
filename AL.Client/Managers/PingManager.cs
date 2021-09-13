using System;
using System.Linq;
using System.Threading.Tasks;
using AL.Client.Abstractions;
using AL.Core.Collections;
using AL.Core.Helpers;

namespace AL.Client.Managers
{
    internal sealed class PingManager : AsyncDeltaLoop
    {
        private readonly CyclicBuffer<int?> Pings;
        public long PingCount;

        /// <summary>
        ///     The minimum offset value based on ping times.
        /// </summary>
        internal int Offset { get; private set; }

        // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
        protected override float PollingRate { get; } = 1f / 4f;

        internal PingManager(ALClient client)
            : base(client) =>
            Pings = new CyclicBuffer<int?>(50);

        protected override async Task DoWorkAsync()
        {
            var delta = DeltaTime.Value;
            await Client.PingAsync(PingCount++).ConfigureAwait(false);
            var jitter = Convert.ToInt32((DeltaTime.Value - delta) / 2);

            if ((Offset == 0) || (jitter < Offset))
                Offset = jitter;

            var old = Pings.Add(jitter);

            if (old == Offset)
                Offset = Pings.Where(ping => ping.HasValue && (ping.Value != default)).Min(ping => ping!.Value);
        }
    }
}