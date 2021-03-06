using System;
using System.Threading.Tasks;
using Chaos.Core.Collections.Synchronized.Awaitable;

namespace AL.SocketClient.ClientModel
{
    internal abstract class ALSocketSubscription : IAsyncDisposable
    {
        private readonly AwaitableList<ALSocketSubscription> InvocationList;
        internal abstract Delegate Callback { get; }

        protected ALSocketSubscription(AwaitableList<ALSocketSubscription> invocationList) =>
            InvocationList = invocationList;

        public async ValueTask DisposeAsync() => await InvocationList.RemoveAsync(this).ConfigureAwait(false);

        internal abstract Task<bool> InvokeAsync(object dataObject);
    }

    internal class AlSocketSubscription<T> : ALSocketSubscription
    {
        internal override Delegate Callback { get; }

        internal AlSocketSubscription(AwaitableList<ALSocketSubscription> invocationList, Func<T, Task<bool>> callback)
            : base(invocationList) =>
            Callback = callback;

        internal static ALSocketSubscription Create(AwaitableList<ALSocketSubscription> invocationList, Func<T, Task<bool>> callback)
        {
            var subscription = new AlSocketSubscription<T>(invocationList, callback);
            //dont need to await this because all we care about is that invocationList is synchronized
            // ReSharper disable once CA2012
 #pragma warning disable CA2012
            _ = invocationList.AddAsync(subscription);
 #pragma warning restore CA2012
            return subscription;
        }

        internal override Task<bool> InvokeAsync(object dataObject) =>
            ((Func<T, Task<bool>>)Callback)((T)dataObject);
    }
}