using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AL.APIClient.Definitions;
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
    public class ALAPIClient
    {
        private static readonly RestClient CLIENT;
        private static readonly ILog Logger;
        private AuthUser AuthUser;
        public Character[] Characters { get; internal set; }
        public bool HasMail { get; internal set; }
        public Server[] Servers { get; internal set; }

        static ALAPIClient()
        {
            Logger = LogManager.GetLogger<ALAPIClient>();
            CLIENT = new RestClient("http://adventure.land/").UseJson();
            CLIENT.UseNewtonsoftJson();
        }

        private ALAPIClient(AuthUser authUser) => AuthUser = authUser;

        public static async Task<string> GetGameDataAsync()
        {
            Logger.Info("Retreiving game data...");

            var request = new RestRequest("data.js", Method.GET);
            var response = await CLIENT.ExecuteGetAsync(request);

            return response.Content.Substring(6, response.Content.Length - 8);
        }

        public async IAsyncEnumerable<Mail> GetMailAsync()
        {
            MailResponse result = null;
            var more = true;

            Logger.Debug("Fetching mail");
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

        public async IAsyncEnumerable<Merchant> GetMerchantsAsync()
        {
            Logger.Debug("Fetching merchants");
            var request = new APIRequest(Method.POST, APIMethod.PullMerchants, null, AuthUser);
            var response = await CLIENT.ExecutePostAsync(request);
            (var merchantList, _) = JsonConvert.DeserializeObject<(MerchantList, string)>(response.Content,
                new ArrayToTupleConverter<MerchantList, string>());

            foreach (var merchant in merchantList.Merchants)
                yield return merchant;
        }

        public static async Task<ALAPIClient> LoginAsync(string email, string password)
        {
            var arguments = new LoginInfo
            {
                Email = email,
                Password = password
            };

            Logger.Debug($"Logging in as {email}:{password}");
            var request = new APIRequest(Method.POST, APIMethod.SignupOrLogin, arguments);
            var response = await CLIENT.ExecutePostAsync(request);
            var setCookieHeader = response.Headers.FirstOrDefault(header => header.Name.EqualsI("set-cookie"));
            var data = JsonConvert.DeserializeObject<LoginResponse>(response.Content);

            Logger.Trace($"Message: {data?.Message}");
            Logger.Trace($"Set-Cookie: {setCookieHeader?.Value}");
            
            if (data == null)
                throw new Exception("Failed to log in. No response from server.");

            if (setCookieHeader?.Value != null && data.Message.EqualsI("Logged In!"))
                return new ALAPIClient(new AuthUser(arguments, setCookieHeader.Value.ToString()));

            throw new Exception($@"Failed to log in. {data.Message ?? "Unknown"}");
        }

        public async Task ReadMailAsync(Mail mail)
        {
            Logger.Debug($"Marking mail {mail.Id} as read");
            var request = new APIRequest(Method.POST, APIMethod.ReadMail, new { mail = mail.Id }, AuthUser);
            await CLIENT.ExecutePostAsync(request);
        }

        public async Task RenewAuth()
        {
            Logger.Debug("Renewing auth");
            var apiClient = await LoginAsync(AuthUser.LoginInfo.Email, AuthUser.LoginInfo.Password);
            AuthUser = apiClient.AuthUser;
        }

        public async Task UpdateServersAndCharactersAsync()
        {
            Logger.Debug("Fetching servers and characters");
            var request = new APIRequest(Method.POST, APIMethod.ServersAndCharacters, null, AuthUser);
            var response = await CLIENT.ExecutePostAsync(request);

            var result = JsonConvert.DeserializeObject<ServersAndCharactersResponse[]>(response.Content)![0];

            Servers = result.Servers;
            Characters = result.Characters;
            HasMail = result.Mail != 0;
        }
    }
}