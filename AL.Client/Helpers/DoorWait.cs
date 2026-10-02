#region
using System.Runtime.CompilerServices;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Provides the wait for a door's map change. A bank door is answered "in progress" first and lands once the bank has
///     loaded, seconds later, so that answer extends the wait.
/// </summary>
internal static class DoorWait
{
    /// <summary>
    ///     Asynchronously waits for the landing, extending the wait once the server answers "in progress".
    /// </summary>
    /// <param name="landed">Completes with the map change.</param>
    /// <param name="inProgress">
    ///     Completes when the server answers the door "in progress".
    /// </param>
    /// <param name="networkTimeoutMS">
    ///     How long to wait for either answer, in milliseconds.
    /// </param>
    /// <param name="bankTimeoutMS">
    ///     How long to wait for the landing after "in progress", in milliseconds.
    /// </param>
    /// <param name="setCrossingAction">
    ///     Called with true when the extended wait starts and false when it ends either way. While it is up, the server owes
    ///     the character a move to the bank from wherever it stands.
    /// </param>
    /// <param name="caller">
    ///     The calling member, named in the timeout message.
    /// </param>
    /// <returns>The map change.</returns>
    /// <exception cref="TimeoutException">
    ///     Neither answer arrived in time, or the bank did not finish loading in time.
    /// </exception>
    internal static async Task<T> WaitForLandingAsync<T>(
        Task<T> landed,
        Task inProgress,
        int networkTimeoutMS,
        int bankTimeoutMS,
        Action<bool> setCrossingAction,
        [CallerMemberName] string? caller = null)
    {
        var first = await Task.WhenAny(landed, inProgress, Task.Delay(networkTimeoutMS));

        if (first == landed)
            return await landed;

        if (first != inProgress)
            throw new TimeoutException($"Network operation timed out after {networkTimeoutMS}ms. ({caller})");

        setCrossingAction(true);

        try
        {
            if (landed == await Task.WhenAny(landed, Task.Delay(bankTimeoutMS)))
                return await landed;

            throw new TimeoutException($"The bank did not finish loading within {bankTimeoutMS}ms. ({caller})");
        } finally
        {
            setCrossingAction(false);
        }
    }
}