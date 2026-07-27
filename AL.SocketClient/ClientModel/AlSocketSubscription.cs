namespace AL.SocketClient.ClientModel;

public abstract class ALSocketSubscription : IDisposable
{
    internal abstract Delegate Callback { get; }
    protected ALSocketSubscriptionList InvocationList { get; }

    //diverges from the list's Type when a later On<T> declared a different T, which is what makes the cast below throw
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