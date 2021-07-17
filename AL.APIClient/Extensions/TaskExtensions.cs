using System;
using System.Threading.Tasks;

namespace AL.APIClient.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for <see cref="Task" />s.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        ///     Applies a timeout to the task.
        /// </summary>
        /// <param name="task">The task to apply the timeout to.</param>
        /// <param name="timeoutMS">The maximum number of milliseconds to wait before timing out.</param>
        /// <typeparam name="T">
        ///     <inheritdoc cref="Task{T}" />
        /// </typeparam>
        /// <returns>The result of the <paramref name="task" />, or a <see cref="TimeoutException" /> if it times out.</returns>
        /// <exception cref="TimeoutException">Operation timed out after {timeoutMS}ms</exception>
        public static async Task<T> WithTimeout<T>(this Task<T> task, int timeoutMS)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeoutMS)))
                return await task;

            throw new TimeoutException($"Operation timed out after {timeoutMS}ms");
        }

        /// <summary>
        ///     Applies a timeout to the task.
        /// </summary>
        /// <param name="task">The task to apply the timeout to.</param>
        /// <param name="timeoutMS">The maximum number of milliseconds to wait before timing out.</param>
        /// <returns>The result of the <paramref name="task" />, or a <see cref="TimeoutException" /> if it times out.</returns>
        /// <exception cref="TimeoutException">Operation timed out after {timeoutMS}ms</exception>
        public static async Task WithTimeout(this Task task, int timeoutMS)
        {
            if (task != await Task.WhenAny(task, Task.Delay(timeoutMS)))
                throw new TimeoutException($"Operation timed out after {timeoutMS}ms");
        }
    }
}