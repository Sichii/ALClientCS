#region
using System;
using Chaos.Collections.Synchronized;
#endregion

namespace AL.SocketClient.ClientModel;

public sealed class ALSocketSubscriptionList : SynchronizedList<ALSocketSubscription>
{
    internal Type Type { get; }
    internal ALSocketSubscriptionList(Type type) => Type = type;
}