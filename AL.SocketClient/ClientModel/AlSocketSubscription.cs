namespace AL.SocketClient.ClientModel;

public abstract class ALSocketSubscription : IDisposable
{
    internal abstract Delegate Callback { get; }
    protected ALSocketSubscriptionList InvocationList { get; }

    /// <summary>
    ///     Diverges from <see cref="ALSocketSubscriptionList.Type" /> when a later <c>On&lt;T&gt;</c> declared a different T,
    ///     which is what makes the cast in <see cref="InvokeAsync" /> throw.
    /// </summary>
    internal abstract Type SubscriptionType { get; }

    protected ALSocketSubscription(ALSocketSubscriptionList invocationList) => InvocationList = invocationList;

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
    internal override Type SubscriptionType => typeof(T);

    internal AlSocketSubscription(ALSocketSubscriptionList invocationList, Func<T, Task<bool>> callback)
        : base(invocationList)
    {
        Callback = callback;
        InvocationList.Add(this);
    }

    internal override Task<bool> InvokeAsync(object dataObject) => ((Func<T, Task<bool>>)Callback)((T)dataObject);
}