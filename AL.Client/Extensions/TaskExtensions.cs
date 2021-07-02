using System;
using System.Threading.Tasks;

namespace AL.Client.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<T> WithTimeout<T>(
            this Task<T> task,
            int timeoutMS = ALClientSettings.NETWORK_TIMEOUT_MS)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeoutMS)))
                return await task;

            throw new TimeoutException($"Operation timed out after {timeoutMS}ms");
        }
        
        public static async Task WithTimeout(this Task task, int timeoutMS = ALClientSettings.NETWORK_TIMEOUT_MS)
        {
            if (task != await Task.WhenAny(task, Task.Delay(timeoutMS)))
                throw new TimeoutException($"Operation timed out after {timeoutMS}ms");
        }
    }
}