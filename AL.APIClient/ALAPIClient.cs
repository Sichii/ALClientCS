#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.APIClient.Model;
using AL.APIClient.Request;
using AL.APIClient.Response;
using AL.Core.Json.Converters;
using Chaos.Extensions.Common;
using Common.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers;
using RestSharp.Serializers.NewtonsoftJson;
#endregion

namespace AL.APIClient;

/// <summary>
///     Provides easy access to the Adventure.Land API. (not the socket server)
/// </summary>
public sealed class ALAPIClient : IALAPIClient
{
    private static readonly IRestClient Client;
    private static readonly ILog Logger;
    private readonly SemaphoreSlim Sync;

    private DateTime LastUpdate;

    private ServersAndCharactersResponse? ServersAndCharacters;

    public AuthUser Auth { get; private set; }

    private bool ShouldUpdate
        => DateTime.UtcNow.Subtract(LastUpdate)
                   .TotalMinutes
           > 1;

    static ALAPIClient()
    {
        Logger = LogManager.GetLogger<ALAPIClient>();

        Client = new RestClient(
            "http://adventure.land",
            configureSerialization: cfgs => cfgs.UseJson()
                                                .UseNewtonsoftJson());
    }

    private ALAPIClient(AuthUser auth)
    {
        LastUpdate = DateTime.MinValue;
        Auth = auth;
        Sync = new SemaphoreSlim(1, 1);
    }

    public async IAsyncEnumerable<Mail> GetMailAsync()
    {
        MailResponse? result = null;
        var more = true;

        Logger.Info("Fetching mail");

        while (more)
        {
            var arguments = result == null
                ? null
                : new
                {
                    result.Cursor
                };

            var request = new APIRequest(
                Method.Post,
                APIMethod.PullMail,
                arguments,
                Auth);

            var response = await Client.ExecutePostAsync(request);
            result = JsonConvert.DeserializeObject<MailResponse[]>(response.Content!)![0];

            foreach (var mail in result.Mail)
                yield return mail;

            more = result.More;
        }
    }

    public async IAsyncEnumerable<MerchantInfo> GetMerchantsAsync()
    {
        Logger.Info("Fetching merchants");

        var request = new APIRequest(
            Method.Post,
            APIMethod.PullMerchants,
            null,
            Auth);

        var response = await Client.ExecutePostAsync(request);

        (var merchantList, _) = JsonConvert.DeserializeObject<(MerchantList, string)>(
            response.Content!,
            new ArrayToTupleConverter<MerchantList, string>());

        foreach (var merchant in merchantList.Merchants)
            yield return merchant;
    }

    public async Task<ServersAndCharactersResponse> GetServersAndCharactersAsync()
    {
        await Sync.WaitAsync();

        try
        {
            if (!ShouldUpdate && (ServersAndCharacters != null))
                return ServersAndCharacters;

            Logger.Info("Fetching servers and characters");

            var request = new APIRequest(
                Method.Post,
                APIMethod.ServersAndCharacters,
                null,
                Auth);

            var response = await Client.ExecutePostAsync(request);

            ServersAndCharacters = JsonConvert.DeserializeObject<ServersAndCharactersResponse[]>(response.Content!)![0];

            LastUpdate = DateTime.UtcNow;

            return ServersAndCharacters;
        } finally
        {
            Sync.Release();
        }
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    ///     mail
    /// </exception>
    public async Task ReadMailAsync(Mail mail)
    {
        ArgumentNullException.ThrowIfNull(mail);

        Logger.Info($"Marking mail {mail.Id} as read");

        var request = new APIRequest(
            Method.Post,
            APIMethod.ReadMail,
            new
            {
                mail = mail.Id
            },
            Auth);

        await Client.ExecutePostAsync(request);
    }

    public async Task RenewAuth()
    {
        Logger.Info("Renewing auth");

        var apiClient = await LoginAsync(Auth.LoginInfo.Email, Auth.LoginInfo.Password);
        Auth = apiClient.Auth;
    }

    /// <summary>
    ///     Asynchronously fetches the "G" data json.
    ///     <br />
    ///     You do not need to be logged in to fetch this data.
    /// </summary>
    /// <returns>
    ///     <see cref="string" />
    ///     <br />
    ///     A json string of the "G" data.
    /// </returns>
    public static async Task<string> GetGameDataAsync()
    {
        Logger.Info("Fetching game data...");

        var request = new RestRequest("data.js");

        var response = await Client.ExecuteGetAsync(request);
        var startBracketIndex = response.Content!.IndexOf('{');
        var endBrackedIndex = response.Content.LastIndexOf('}');

        return response.Content.Substring(startBracketIndex, endBrackedIndex - startBracketIndex + 1);
    }

    /// <summary>
    ///     Asynchronously logs in to the API.
    /// </summary>
    /// <param name="email">
    ///     The user's email.
    /// </param>
    /// <param name="password">
    ///     The user's password.
    /// </param>
    /// <returns>
    ///     <see cref="ALAPIClient" />
    ///     <br />
    ///     An ALAPIClient that can be used to fetch user-specific information.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     email
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     password
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to log in. No response from server.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to log in. {data.Message ?? "Unknown"}
    /// </exception>
    public static async Task<ALAPIClient> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        var loginInfo = new LoginInfo
        {
            Email = email,
            Password = password
        };

        Logger.Info($"Logging in as {email}:{password}");

        var request = new APIRequest(
            Method.Post,
            APIMethod.SignupOrLogin,
            new
            {
                email,
                password,
                only_login = true
            });

        var response = await Client.ExecutePostAsync(request); //.WithTimeout(60000);
        var setCookieHeader = response.Headers!.FirstOrDefault(header => header.Name.EqualsI("set-cookie"));
        var data = JsonConvert.DeserializeObject<LoginResponse>(response.Content!);

        Logger.Debug($"Login: Message: {data?.Message}");
        Logger.Debug($"Login: Set-Cookie: {setCookieHeader?.Value}");

        if (data == null)
            throw new InvalidOperationException("Failed to log in. No response from server.");

        if ((setCookieHeader?.Value != null) && data.Message!.EqualsI("Logged In!"))
            return new ALAPIClient(new AuthUser(loginInfo, setCookieHeader.Value));

        throw new InvalidOperationException($@"Failed to log in. {data.Message ?? "Unknown"}");
    }
}