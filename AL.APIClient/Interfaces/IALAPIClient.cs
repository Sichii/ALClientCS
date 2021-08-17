using System.Collections.Generic;
using System.Threading.Tasks;
using AL.APIClient.Model;
using AL.APIClient.Response;

namespace AL.APIClient.Interfaces
{
    /// <summary>
    ///     Provides an interface for interacting with the Adventure.Land API. (not the socket server)
    /// </summary>
    public interface IALAPIClient
    {
        /// <summary>
        ///     Authorization data for the logged in user.
        /// </summary>
        AuthUser Auth { get; }

        /// <summary>
        ///     Asynchronously fetches mail from the server.
        /// </summary>
        /// <returns>
        ///     <see cref="IAsyncEnumerable{T}" /> of <see cref="Mail" /> <br />
        ///     Mail is returned paged, if you reach the end of a page, this will automatically request the next page.
        /// </returns>
        IAsyncEnumerable<Mail> GetMailAsync();

        /// <summary>
        ///     Asynchronously fetches merchants from the server.
        /// </summary>
        /// <returns><see cref="IAsyncEnumerable{T}" /> of <see cref="MerchantInfo" /></returns>
        IAsyncEnumerable<MerchantInfo> GetMerchantsAsync();

        /// <summary>
        ///     Asynchronously fetches servers and characters from the API.
        /// </summary>
        /// <returns>
        ///     <see cref="ServersAndCharactersResponse" /> <br />
        ///     The servers and characters available for this authorized user. <br />
        ///     If they have been fetched recently, instead returns a cached instance.
        /// </returns>
        Task<ServersAndCharactersResponse> GetServersAndCharactersAsync();

        /// <summary>
        ///     Asynchronously marks a mail as having been read.
        /// </summary>
        /// <param name="mail">The mail to mark.</param>
        Task ReadMailAsync(Mail mail);

        /// <summary>
        ///     Asynchronously re-logs in and replaces the <see cref="Auth" />.
        /// </summary>
        /// <remarks>Use this if you're nearing the expiry date for this client's <see cref="Auth" />.</remarks>
        Task RenewAuth();
    }
}