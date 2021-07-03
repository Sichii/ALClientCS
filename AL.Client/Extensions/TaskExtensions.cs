using System;
using System.Threading.Tasks;

namespace AL.Client.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<T> WithNetworkTimeout<T>(this Task<T> task)
        {
            var timeoutMS = ALClientSettings.NETWORK_TIMEOUT_MS;

            if (task == await Task.WhenAny(task, Task.Delay(timeoutMS)))
                return await task;

            throw new TimeoutException($"Network operation timed out after {timeoutMS}ms");
        }

        public static async Task WithNetworkTimeout(this Task task)
        {
            var timeoutMS = ALClientSettings.NETWORK_TIMEOUT_MS;

            if (task != await Task.WhenAny(task, Task.Delay(timeoutMS)))
                throw new TimeoutException($"Network operation timed out after {timeoutMS}ms");
        }
    }
}