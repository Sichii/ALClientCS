#region
using System.Runtime.CompilerServices;
#endregion

namespace AL.Client.Extensions;

internal static class TaskExtensions
{
    internal static Task<T> WithNetworkTimeout<T>(this Task<T> task, [CallerMemberName] string? caller = null)
        => task.WithTimeout(ALClientSettings.NetworkTimeoutMS, caller);

    internal static async Task WithNetworkTimeout(this Task task, [CallerMemberName] string? caller = null)
    {
        var timeoutMS = ALClientSettings.NetworkTimeoutMS;

        if (task != await Task.WhenAny(task, Task.Delay(timeoutMS)))
            throw new TimeoutException($"Network operation timed out after {timeoutMS}ms. ({caller})");
    }

    /// <summary>
    ///     Asynchronously waits for the task, for operations the server is allowed longer on than a network round trip.
    /// </summary>
    /// <param name="task">The task to wait for.</param>
    /// <param name="timeoutMS">How long to wait, in milliseconds.</param>
    /// <param name="caller">
    ///     The calling member, named in the timeout message.
    /// </param>
    /// <returns>The task's result.</returns>
    /// <exception cref="TimeoutException">The task did not complete in time.</exception>
    internal static async Task<T> WithTimeout<T>(this Task<T> task, int timeoutMS, [CallerMemberName] string? caller = null)
    {
        if (task == await Task.WhenAny(task, Task.Delay(timeoutMS)))
            return await task;

        throw new TimeoutException($"Network operation timed out after {timeoutMS}ms. ({caller})");
    }
}