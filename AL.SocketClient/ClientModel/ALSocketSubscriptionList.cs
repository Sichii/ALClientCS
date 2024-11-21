#region
using System;
using System.Threading.Tasks;
using Chaos.Collections.Synchronized;
#endregion

namespace AL.SocketClient.ClientModel;

public sealed class ALSocketSubscriptionList : SynchronizedList<ALSocketSubscription>
{
    internal Type Type { get; }
    internal ALSocketSubscriptionList(Type type) => Type = type;

    internal async Task InvokeAsync(object dataObject)
    {
        foreach (var subscription in this)
        {
            var handled = await subscription.InvokeAsync(dataObject)
                                            .ConfigureAwait(false);

            if (handled)
                return;
        }
    }
}