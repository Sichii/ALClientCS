using System;
using System.Threading.Tasks;
using Chaos.Core.Collections.Synchronized.Awaitable;

namespace AL.SocketClient.ClientModel
{
    internal class ALSocketSubscriptionList : AwaitableList<ALSocketSubscription>
    {
        internal Type Type { get; }
        internal ALSocketSubscriptionList(Type type) => Type = type;

        internal async Task InvokeAsync(object dataObject)
        {
            await foreach (var subscription in this.ConfigureAwait(false))
            {
                var handled = await subscription.InvokeAsync(dataObject).ConfigureAwait(false);

                if (handled)
                    return;
            }
        }
    }
}