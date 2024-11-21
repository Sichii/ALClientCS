#region
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chaos.Collections.Synchronized;
#endregion

namespace AL.SocketClient.ClientModel;

public abstract class ALSocketSubscription : IDisposable
{
    internal abstract Delegate Callback { get; }
    protected SynchronizedList<ALSocketSubscription> InvocationList { get; }

    protected ALSocketSubscription(IEnumerable<ALSocketSubscription> invocationList)
        => InvocationList = new SynchronizedList<ALSocketSubscription>(invocationList);

    public void Dispose()
    {
        InvocationList.Remove(this);

        GC.SuppressFinalize(this);
    }

    internal abstract Task<bool> InvokeAsync(object dataObject);
}

public sealed class AlSocketSubscription<T> : ALSocketSubscription
{
    internal override Delegate Callback { get; }

    internal AlSocketSubscription(IEnumerable<ALSocketSubscription> invocationList, Func<T, Task<bool>> callback)
        : base(invocationList)
    {
        Callback = callback;
        InvocationList.Add(this);
    }

    internal override Task<bool> InvokeAsync(object dataObject) => ((Func<T, Task<bool>>)Callback)((T)dataObject);
}