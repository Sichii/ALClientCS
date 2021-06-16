using System;
using Chaos.Core.Collections.Synchronized.Awaitable;

namespace AL.SocketClient.Objects
{
    internal class ALSocketSubscriptionList : AwaitableList<ALSocketSubscription>
    {
        internal Type Type { get; set; }
        internal ALSocketSubscriptionList(Type type) => Type = type;
    }
}