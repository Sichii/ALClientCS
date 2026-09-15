#region
using System.Runtime.CompilerServices;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     The wait for a door's map change. An ordinary door lands inside the network timeout. A bank door is answered "in
///     progress" first and lands once the bank has loaded, seconds later, so that answer extends the wait.
/// </summary>
internal static class DoorWait
{
    /// <summary>
    ///     Completes with the landing, or throws <see cref="TimeoutException" />. <paramref name="crossing" /> is raised
    ///     while the extended wait runs and lowered when it ends either way: for as long as it is up, the server owes the
    ///     character a move to the bank from wherever it stands.
    /// </summary>
    internal static async Task<T> ForLandingAsync<T>(
        Task<T> landed,
        Task inProgress,
        int networkTimeoutMS,
        int bankTimeoutMS,
        Action<bool> crossing,
        [CallerMemberName] string? caller = null)
    {
        var first = await Task.WhenAny(landed, inProgress, Task.Delay(networkTimeoutMS));

        if (first == landed)
            return await landed;

        if (first != inProgress)
            throw new TimeoutException($"Network operation timed out after {networkTimeoutMS}ms. ({caller})");

        crossing(true);

        try
        {
            if (landed == await Task.WhenAny(landed, Task.Delay(bankTimeoutMS)))
                return await landed;

            throw new TimeoutException($"The bank did not finish loading within {bankTimeoutMS}ms. ({caller})");
        } finally
        {
            crossing(false);
        }
    }
}
