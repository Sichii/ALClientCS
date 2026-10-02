#region
using AL.APIClient.Model;
using AL.APIClient.Response;
#endregion

namespace AL.APIClient.Interfaces;

/// <summary>
///     Provides an interface for interacting with the Adventure.Land API. (not the socket server)
/// </summary>
public interface IAlApiClient
{
    /// <summary>The authorization data of the logged in account.</summary>
    AuthUser Auth { get; }

    /// <summary>
    ///     Asynchronously deletes a mail. The server allows this for a mail whose attached item was never taken, and deleting
    ///     one destroys the item.
    /// </summary>
    /// <param name="mail">The mail to delete.</param>
    Task DeleteMailAsync(Mail mail);

    /// <summary>
    ///     Asynchronously fetches mail from the server, requesting each next page as the previous one runs out.
    /// </summary>
    /// <returns>Every mail of the account.</returns>
    IAsyncEnumerable<Mail> GetMailAsync();

    /// <summary>
    ///     Asynchronously fetches merchants from the server.
    /// </summary>
    /// <returns>Every merchant the server lists.</returns>
    IAsyncEnumerable<MerchantInfo> GetMerchantsAsync();

    /// <summary>
    ///     Asynchronously fetches servers and characters from the API, or a cached copy if fetched recently.
    /// </summary>
    /// <param name="forceRefresh">
    ///     Specifies whether to read from the API even when a recent copy is cached, such as to see whether a logout has
    ///     landed.
    /// </param>
    /// <returns>
    ///     The servers and characters available to this account.
    /// </returns>
    Task<ServersAndCharactersResponse> GetServersAndCharactersAsync(bool forceRefresh = false);

    /// <summary>Asynchronously marks a mail as read.</summary>
    /// <param name="mail">The mail to mark.</param>
    Task ReadMailAsync(Mail mail);

    /// <summary>
    ///     Asynchronously re-logs in and replaces the <see cref="Auth" />.
    /// </summary>
    /// <remarks>
    ///     Use this if you're nearing the expiry date for this client's <see cref="Auth" />.
    /// </remarks>
    Task RenewAuthAsync();
}