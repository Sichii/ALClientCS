#region
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.APIClient.Model;
using AL.APIClient.Json.SystemTextJson;
using AL.APIClient.Request;
using AL.APIClient.Response;
using Chaos.Extensions.Common;
using Common.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using RestSharp;
#endregion

namespace AL.APIClient;

/// <summary>
///     Provides easy access to the Adventure.Land API. (not the socket server)
/// </summary>
public sealed class AlApiClient : IAlApiClient
{
    /// <summary>
    ///     The public game host. Must be https - the auth cookie is set with the "secure" flag.
    /// </summary>
    public const string DEFAULT_BASE_URL = "https://adventure.land";

    private static readonly ILog Logger = LogManager.GetLogger<AlApiClient>();

    //keyed by host so a caller pointed at a different server is not served the public one's tables
    private static readonly ConcurrentDictionary<string, Lazy<Task<string>>> GameDataCache = new();

    private readonly string BaseUrl;

    //each client owns its cookie jar so several accounts can be logged in side by side
    private readonly IRestClient Client;
    private readonly string CookieDomain;
    private readonly SemaphoreSlim Sync;

    private DateTime LastUpdate;

    private ServersAndCharactersResponse? ServersAndCharacters;

    public AuthUser Auth { get; private set; }

    private bool ShouldUpdate
        => DateTime.UtcNow.Subtract(LastUpdate)
                   .TotalMinutes
           > 1;

    private AlApiClient(AuthUser auth, IRestClient client, string baseUrl)
    {
        LastUpdate = DateTime.MinValue;
        Auth = auth;
        Client = client;
        BaseUrl = baseUrl;
        CookieDomain = new Uri(baseUrl).Host;
        Sync = new SemaphoreSlim(1, 1);
    }

    public async IAsyncEnumerable<Mail> GetMailAsync()
    {
        MailResponse? result = null;
        var more = true;

        Logger.Info("Fetching mail");

        while (more)
        {
            //the server reads a lowercase "cursor"; sending "Cursor" pages forever
            var arguments = result == null
                ? null
                : new
                {
                    cursor = result.Cursor
                };

            var request = new APIRequest(
                Method.Post,
                APIMethod.PullMail,
                arguments,
                Auth,
                CookieDomain);

            var response = await Client.ExecutePostAsync(request);

            result = ReadNotifications(response)
                .Deserialize<MailResponse[]>(ApiJson.Options)![0];

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
            Auth,
            CookieDomain);

        var response = await Client.ExecutePostAsync(request);

        (var merchantList, _) = ReadNotifications(response)
            .Deserialize<(MerchantList, string)>(ApiJson.Options);

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
                Auth,
                CookieDomain);

            var response = await Client.ExecutePostAsync(request);

            ServersAndCharacters = ReadNotifications(response)
                .Deserialize<ServersAndCharactersResponse[]>(ApiJson.Options)![0];

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

        //the server unconditionally prepends "ML_" (api.js:886); mail.Id already carries it, so strip it here or
        //the lookup misses ("ML_ML_...") and the mail is silently never marked read
        var request = new APIRequest(
            Method.Post,
            APIMethod.ReadMail,
            new
            {
                mail = mail.Id.StartsWith("ML_", StringComparison.Ordinal) ? mail.Id["ML_".Length..] : mail.Id
            },
            Auth,
            CookieDomain);

        await Client.ExecutePostAsync(request);
    }

    public async Task RenewAuth()
    {
        Logger.Info("Renewing auth");

        var apiClient = await LoginAsync(Auth.LoginInfo.Email, Auth.LoginInfo.Password, BaseUrl);
        Auth = apiClient.Auth;
    }

    // data.js is ~2.6MB and the server often trickles it well past the 100s default.
    private static IRestClient CreateRestClient(string baseUrl)
        => new RestClient(new RestClientOptions(baseUrl) { Timeout = TimeSpan.FromMinutes(5) });

    /// <summary>
    ///     Asynchronously fetches the "G" data json.
    ///     <br />
    ///     You do not need to be logged in to fetch this data.
    /// </summary>
    /// <param name="baseUrl">
    ///     The host to fetch from. Defaults to the public game host.
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    ///     <br />
    ///     A json string of the "G" data.
    /// </returns>
    /// <summary>
    ///     Fetches <c>data.js</c>, once per host for the life of the process.
    /// </summary>
    /// <remarks>
    ///     The body is multiple megabytes and the server can spend minutes sending it, so a second caller downloading
    ///     it again is the most expensive thing a boot can do. The data is static for a server session anyway. A failed
    ///     fetch evicts itself, so a transient one does not poison every later caller.
    /// </remarks>
    public static Task<string> GetGameDataAsync(string baseUrl = DEFAULT_BASE_URL)
        => GameDataCache.GetOrAdd(baseUrl, url => new Lazy<Task<string>>(() => FetchGameDataAsync(url)))
                        .Value;

    private static async Task<string> FetchGameDataAsync(string baseUrl)
    {
        Logger.Info("Fetching game data...");

        try
        {
            using var client = CreateRestClient(baseUrl);
            var request = new RestRequest("data.js");

            var response = await client.ExecuteGetAsync(request);

            if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
                throw new InvalidOperationException($"Failed to fetch game data. ({response.StatusCode}) {response.ErrorMessage}");

            var startBracketIndex = response.Content.IndexOf('{');
            var endBrackedIndex = response.Content.LastIndexOf('}');

            return response.Content.Substring(startBracketIndex, endBrackedIndex - startBracketIndex + 1);
        } catch
        {
            GameDataCache.TryRemove(baseUrl, out _);

            throw;
        }
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
    /// <param name="baseUrl">
    ///     The host to log into. Defaults to the public game host.
    /// </param>
    /// <returns>
    ///     <see cref="AlApiClient" />
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
    ///     Failed to log in. {reason}
    /// </exception>
    public static async Task<AlApiClient> LoginAsync(string email, string password, string baseUrl = DEFAULT_BASE_URL)
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

        Logger.Info($"Logging in as {email}");

        var client = CreateRestClient(baseUrl);

        var request = new APIRequest(
            Method.Post,
            APIMethod.SignupOrLogin,
            new
            {
                email,
                password,
                only_login = true
            });

        var response = await client.ExecutePostAsync(request);

        //without this a transport failure surfaces as an unrelated null reference further down
        if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
            throw new InvalidOperationException($"Failed to log in. ({response.StatusCode}) {response.ErrorMessage}");

        //the response can carry several Set-Cookie headers; only one of them is the auth pair
        var authCookie = response.Headers
                                 ?.Where(header => header.Name.EqualsI("set-cookie"))
                                 .Select(header => header.Value.ToString())
                                 .FirstOrDefault(value => value.StartsWithI("auth="));

        var data = JsonSerializer.Deserialize<LoginResponse>(response.Content!, ApiJson.Options);

        Logger.Debug($"Login: Message: {data?.Message}, Reason: {data?.Reason}");

        if (data == null)
            throw new InvalidOperationException("Failed to log in. No response from server.");

        //the cookie is the only real proof of a successful login; the message text is cosmetic
        if (data.Failed || string.IsNullOrEmpty(authCookie))
            throw new InvalidOperationException($"Failed to log in. {data.Reason ?? data.Message ?? "Unknown"}");

        return new AlApiClient(new AuthUser(loginInfo, authCookie), client, baseUrl);
    }

    /// <summary>
    ///     Unwraps an api response into the notification array the payload actually lives in.
    /// </summary>
    /// <remarks>
    ///     Every response is an object of the form
    ///     <c>
    ///         { success|failed, reason?, infs:[...] }
    ///     </c>
    ///     . Handlers push their real payload into
    ///     <c>
    ///         infs
    ///     </c>
    ///     and return only a status on the envelope.
    /// </remarks>
    private static JsonArray ReadNotifications(RestResponse response)
    {
        if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
            throw new InvalidOperationException($"API call failed. ({response.StatusCode}) {response.ErrorMessage}");

        var body = JsonNode.Parse(response.Content);

        if (body is not JsonObject envelope)
            return body as JsonArray ?? new JsonArray();

        if (envelope["failed"]
                ?.GetValue<bool>()
            ?? false)
            throw new InvalidOperationException($@"API call failed. {envelope["reason"]?.GetValue<string>() ?? "Unknown"}");

        return envelope["infs"] as JsonArray ?? new JsonArray();
    }
}