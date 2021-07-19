using System;
using System.Threading.Tasks;

namespace AL.Client.Extensions
{
    internal static class TaskExtensions
    {
        internal static async Task<T> WithNetworkTimeout<T>(this Task<T> task)
        {
            var timeoutMS = ALClientSettings.NetworkTimeoutMS;

            if (task == await Task.WhenAny(task, Task.Delay(timeoutMS)))
                return await task;

            throw new TimeoutException($"Network operation timed out after {timeoutMS}ms");
        }

        internal static async Task WithNetworkTimeout(this Task task)
        {
            var timeoutMS = ALClientSettings.NetworkTimeoutMS;

            if (task != await Task.WhenAny(task, Task.Delay(timeoutMS)))
                throw new TimeoutException($"Network operation timed out after {timeoutMS}ms");
        }
    }
}