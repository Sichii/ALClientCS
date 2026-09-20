#region
using System.Runtime.CompilerServices;
#endregion

namespace AL.Client.Extensions;

internal static class TaskExtensions
{
    internal static Task<T> WithNetworkTimeout<T>(this Task<T> task, [CallerMemberName] string? caller = null)
        => task.WithTimeout(ALClientSettings.NetworkTimeoutMS, caller);

    /// <summary>
    ///     For the few operations the server is allowed longer on than a network round trip.
    /// </summary>
    internal static async Task<T> WithTimeout<T>(this Task<T> task, int timeoutMS, [CallerMemberName] string? caller = null)
    {
        if (task == await Task.WhenAny(task, Task.Delay(timeoutMS)))
            return await task;

        throw new TimeoutException($"Network operation timed out after {timeoutMS}ms. ({caller})");
    }

    internal static async Task WithNetworkTimeout(this Task task, [CallerMemberName] string? caller = null)
    {
        var timeoutMS = ALClientSettings.NetworkTimeoutMS;

        if (task != await Task.WhenAny(task, Task.Delay(timeoutMS)))
            throw new TimeoutException($"Network operation timed out after {timeoutMS}ms. ({caller})");
    }
}