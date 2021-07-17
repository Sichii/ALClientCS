using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AL.APIClient.Definitions;
using AL.APIClient.Extensions;
using AL.APIClient.Model;
using AL.APIClient.Request;
using AL.APIClient.Response;
using AL.Core.Json.Converters;
using Chaos.Core.Extensions;
using Common.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace AL.APIClient
{
    /// <summary>
    /// Provides easy access to the Adventure.Land API. (not the socket server)
    /// </summary>
    public class ALAPIClient
    {
        private static readonly RestClient CLIENT;
        private static readonly ILog Logger;
        
        /// <summary>
        /// Authorization data for the logged in user.
        /// </summary>
        public AuthUser AuthUser;
        
        /// <summary>
        /// If populated, this is a list of characters belonging to the user. <br/>
        /// Use <see cref="UpdateServersAndCharactersAsync"/> to populated this.
        /// </summary>
        public IReadOnlyList<Character>? Characters { get; internal set; }
        
        /// <summary>
        /// Whether or not the user has mail.
        /// </summary>
        public bool HasMail { get; internal set; }
        
        /// <summary>
        /// If populated, this is a list of servers the user can log characters onto. <br/>
        /// Use <see cref="UpdateServersAndCharactersAsync"/> to populated this.
        /// </summary>
        public IReadOnlyList<Server>? Servers { get; internal set; }

        static ALAPIClient()
        {
            Logger = LogManager.GetLogger<ALAPIClient>();
            CLIENT = new RestClient("http://adventure.land/").UseJson();
            CLIENT.UseNewtonsoftJson();
        }

        private ALAPIClient(AuthUser authUser) => AuthUser = authUser;

        /// <summary>
        /// Asynchronously fetches the "G" data json. <br/>
        /// You do not need to be logged in to fetch this data.
        /// </summary>
        /// <returns><see cref="string"/> <br/>
        /// A json string of the "G" data.</returns>
        public static async Task<string> GetGameDataAsync()
        {
            Logger.Info("Retreiving game data...");

            var request = new RestRequest("data.js", Method.GET);
            var response = await CLIENT.ExecuteGetAsync(request);

            return response.Content.Substring(6, response.Content.Length - 8);
        }

        /// <summary>
        /// Asynchronously fetches mail from the server.
        /// </summary>
        /// <returns><see cref="IAsyncEnumerable{T}"/> of <see cref="Mail"/> <br/>
        /// Mail is returned paged, if you reach the end of a page, this will automatically request the next page.</returns>
        public async IAsyncEnumerable<Mail> GetMailAsync()
        {
            MailResponse? result = null;
            var more = true;

            Logger.Info("Fetching mail");
            while (more)
            {
                var arguments = result == null ? null : new { result.Cursor };
                var request = new APIRequest(Method.POST, APIMethod.PullMail, arguments, AuthUser);
                var response = await CLIENT.ExecutePostAsync(request);
                result = JsonConvert.DeserializeObject<MailResponse[]>(response.Content)![0];

                foreach (var mail in result.Mail)
                    yield return mail;

                more = result.More;
            }
        }

        /// <summary>
        /// Asynchronously fetches merchants from the server.
        /// </summary>
        /// <returns><see cref="IAsyncEnumerable{T}"/> of <see cref="Merchant"/></returns>
        public async IAsyncEnumerable<Merchant> GetMerchantsAsync()
        {
            Logger.Info("Fetching merchants");
            var request = new APIRequest(Method.POST, APIMethod.PullMerchants, null, AuthUser);
            var response = await CLIENT.ExecutePostAsync(request);
            (var merchantList, _) = JsonConvert.DeserializeObject<(MerchantList, string)>(response.Content,
                new ArrayToTupleConverter<MerchantList, string>());

            foreach (var merchant in merchantList.Merchants)
                yield return merchant;
        }

        /// <summary>
        /// Asynchronously logs in to the API.
        /// </summary>
        /// <param name="email">The user's email.</param>
        /// <param name="password">The user's password.</param>
        /// <returns><see cref="ALAPIClient"/> <br/>
        /// An ALAPIClient that can be used to fetch user-specific information.</returns>
        /// <exception cref="ArgumentNullException">email</exception>
        /// <exception cref="ArgumentNullException">password</exception>
        /// <exception cref="InvalidOperationException">Failed to log in. No response from server.</exception>
        /// <exception cref="InvalidOperationException">Failed to log in. {data.Message ?? "Unknown"}</exception>
        public static async Task<ALAPIClient> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));
            
            var arguments = new LoginInfo
            {
                Email = email,
                Password = password
            };

            Logger.Info($"Logging in as {email}:{password}");
            var request = new APIRequest(Method.POST, APIMethod.SignupOrLogin, arguments);
            var response = await CLIENT.ExecutePostAsync(request).WithTimeout(2000);
            var setCookieHeader = response.Headers.FirstOrDefault(header => header.Name.EqualsI("set-cookie"));
            var data = JsonConvert.DeserializeObject<LoginResponse>(response.Content);

            Logger.Debug($"Message: {data?.Message}");
            Logger.Debug($"Set-Cookie: {setCookieHeader?.Value}");

            if (data == null)
                throw new InvalidOperationException("Failed to log in. No response from server.");

            if ((setCookieHeader?.Value != null) && data.Message.EqualsI("Logged In!"))
                return new ALAPIClient(new AuthUser(arguments, setCookieHeader.Value.ToString() ?? string.Empty));

            throw new InvalidOperationException($@"Failed to log in. {data.Message ?? "Unknown"}");
        }

        /// <summary>
        /// Asynchronously marks a mail as having been read.
        /// </summary>
        /// <param name="mail">The mail to mark.</param>
        /// <exception cref="ArgumentNullException">mail</exception>
        public async Task ReadMailAsync(Mail mail)
        {
            if (mail == null)
                throw new ArgumentNullException(nameof(mail));
            
            Logger.Info($"Marking mail {mail.Id} as read");
            var request = new APIRequest(Method.POST, APIMethod.ReadMail, new { mail = mail.Id }, AuthUser);
            await CLIENT.ExecutePostAsync(request);
        }

        /// <summary>
        /// Asynchronously re-logs in and replaces the <see cref="AuthUser"/>.
        /// </summary>
        /// <remarks>Use this if you're nearing the expiry date for this client's <see cref="AuthUser"/>.</remarks>
        public async Task RenewAuth()
        {
            Logger.Info("Renewing auth");
            var apiClient = await LoginAsync(AuthUser.LoginInfo.Email, AuthUser.LoginInfo.Password);
            AuthUser = apiClient.AuthUser;
        }

        /// <summary>
        /// Asynchronously fetches servers and characters from the API, and populates <see cref="Servers"/> and <see cref="Characters"/>.
        /// </summary>
        public async Task UpdateServersAndCharactersAsync()
        {
            Logger.Info("Fetching servers and characters");
            var request = new APIRequest(Method.POST, APIMethod.ServersAndCharacters, null, AuthUser);
            var response = await CLIENT.ExecutePostAsync(request);

            var result = JsonConvert.DeserializeObject<ServersAndCharactersResponse[]>(response.Content)![0];

            Servers = result.Servers;
            Characters = result.Characters;
            HasMail = result.Mail != 0;
        }
    }
}