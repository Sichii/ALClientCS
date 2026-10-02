#region
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.APIClient.Json.SystemTextJson;
using AL.APIClient.Model;
using AL.APIClient.Request;
using AL.APIClient.Response;
using Chaos.Extensions.Common;
using Common.Logging;
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

    /// <summary>
    ///     The shortest gap between two logins <see cref="PostAsync" /> makes for a dead cookie.
    /// </summary>
    private static readonly TimeSpan RENEW_COOLDOWN = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     The fetched <c>data.js</c> per host, so a caller pointed at a different server is not served the public one's tables.
    /// </summary>
    private static readonly ConcurrentDictionary<string, Lazy<Task<string>>> GameDataCache = new();

    private readonly string BaseUrl;

    /// <summary>
    ///     The REST client holding this account's cookie jar, so several accounts can be logged in side by side.
    /// </summary>
    private readonly IRestClient Client;

    private readonly string CookieDomain;
    private readonly SemaphoreSlim Sync;

    private DateTime LastUpdate;

    /// <summary>
    ///     The time <see cref="PostAsync" /> last logged in again for a dead cookie.
    /// </summary>
    private DateTime LastRenewal = DateTime.MinValue;

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

    public async Task DeleteMailAsync(Mail mail)
    {
        ArgumentNullException.ThrowIfNull(mail);

        Logger.Info($"Deleting mail {mail.Id}");

        //the server looks up mid exactly as pull_mail sent it, unlike read_mail, which prepends "ML_" itself
        var request = new APIRequest(
            Method.Post,
            APIMethod.DeleteMail,
            new
            {
                mid = mail.Id
            },
            Auth,
            CookieDomain);

        await Client.ExecutePostAsync(request);
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

            result = (await PostAsync(APIMethod.PullMail, arguments))
                .Deserialize<MailResponse[]>(ApiJson.Options)![0];

            foreach (var mail in result.Mail)
                yield return mail;

            more = result.More;
        }
    }

    public async IAsyncEnumerable<MerchantInfo> GetMerchantsAsync()
    {
        Logger.Info("Fetching merchants");

        (var merchantList, _) = (await PostAsync(APIMethod.PullMerchants, null))
            .Deserialize<(MerchantList, string)>(ApiJson.Options);

        foreach (var merchant in merchantList.Merchants)
            yield return merchant;
    }

    public async Task<ServersAndCharactersResponse> GetServersAndCharactersAsync(bool forceRefresh = false)
    {
        await Sync.WaitAsync();

        try
        {
            if (!forceRefresh && !ShouldUpdate && (ServersAndCharacters != null))
                return ServersAndCharacters;

            Logger.Info("Fetching servers and characters");

            ServersAndCharacters = (await PostAsync(APIMethod.ServersAndCharacters, null))
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

        //the server prepends "ML_" itself, so a full id would miss as "ML_ML_..." and the mail never be marked read
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

    public async Task RenewAuthAsync()
    {
        Logger.Info("Renewing auth");

        var apiClient = await LoginAsync(Auth.LoginInfo.Email, Auth.LoginInfo.Password, BaseUrl);
        Auth = apiClient.Auth;
    }

    /// <summary>
    ///     Creates a REST client with a raised timeout: <c>data.js</c> is ~2.6MB and the server often trickles it well past
    ///     the 100s default.
    /// </summary>
    /// <param name="baseUrl">
    ///     The host the client talks to.
    /// </param>
    /// <returns>
    ///     A REST client for the host.
    /// </returns>
    private static IRestClient CreateRestClient(string baseUrl)
        => new RestClient(
            new RestClientOptions(baseUrl)
            {
                Timeout = TimeSpan.FromMinutes(5)
            });

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
            var endBracketIndex = response.Content.LastIndexOf('}');

            return response.Content.Substring(startBracketIndex, endBracketIndex - startBracketIndex + 1);
        } catch
        {
            GameDataCache.TryRemove(baseUrl, out _);

            throw;
        }
    }

    /// <summary>
    ///     Asynchronously fetches the <c>G</c> data json from <c>data.js</c>, once per host for the life of the process.
    ///     No login is needed.
    /// </summary>
    /// <param name="baseUrl">
    ///     The host to fetch from. Defaults to the public game host.
    /// </param>
    /// <returns>
    ///     The json of the <c>G</c> data.
    /// </returns>
    /// <remarks>
    ///     A failed fetch evicts itself, so a transient failure does not poison every later caller.
    /// </remarks>
    public static Task<string> GetGameDataAsync(string baseUrl = DEFAULT_BASE_URL)
        => GameDataCache.GetOrAdd(baseUrl, url => new Lazy<Task<string>>(() => FetchGameDataAsync(url)))
                        .Value;

    /// <summary>
    ///     Asynchronously logs in to the API.
    /// </summary>
    /// <param name="email">
    ///     The account's email.
    /// </param>
    /// <param name="password">
    ///     The account's password.
    /// </param>
    /// <param name="baseUrl">
    ///     The host to log into. Defaults to the public game host.
    /// </param>
    /// <returns>
    ///     A client that can fetch account-specific information.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     email
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     password
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     The request failed, the server sent no body, or the login was refused.
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
    ///     Asynchronously posts one api call and unwraps it, logging in again once if the server no longer knows this
    ///     client's cookie.
    /// </summary>
    /// <param name="method">
    ///     The api method to call.
    /// </param>
    /// <param name="arguments">
    ///     The call's arguments, or null for none.
    /// </param>
    /// <returns>
    ///     The response's <c>infs</c> array.
    /// </returns>
    /// <remarks>
    ///     The server keeps 200 cookies per account and clears them all when a login would add one more;
    ///     <c>logout_everywhere</c> clears them outright. Every later call then fails <c>not_logged_in</c>.
    /// </remarks>
    private async Task<JsonArray> PostAsync(APIMethod method, object? arguments)
    {
        var response = await Client.ExecutePostAsync(
            new APIRequest(
                Method.Post,
                method,
                arguments,
                Auth,
                CookieDomain));

        if (!IsNotLoggedIn(response))
            return ReadNotifications(response);

        var now = DateTime.UtcNow;

        //floored so a refused login cannot add a cookie on every call and push the account's list toward its wipe
        if ((now - LastRenewal) >= RENEW_COOLDOWN)
        {
            LastRenewal = now;
            await RenewAuthAsync();
        }

        response = await Client.ExecutePostAsync(
            new APIRequest(
                Method.Post,
                method,
                arguments,
                Auth,
                CookieDomain));

        return ReadNotifications(response);
    }

    private static bool IsNotLoggedIn(RestResponse response)
        => response.IsSuccessful
           && !string.IsNullOrEmpty(response.Content)
           && response.Content.Contains("\"not_logged_in\"", StringComparison.Ordinal);

    /// <summary>
    ///     Unwraps an api response into the <c>infs</c> array its payload lives in.
    /// </summary>
    /// <param name="response">The api response.</param>
    /// <returns>
    ///     The <c>infs</c> array, or an empty array when the response has none.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     The request failed, or the envelope reports <c>failed</c>.
    /// </exception>
    /// <remarks>
    ///     Every response is an object of the form <c>{ success|failed, reason?, infs:[...] }</c>.
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
            throw new InvalidOperationException($"API call failed. {envelope["reason"]?.GetValue<string>() ?? "Unknown"}");

        return envelope["infs"] as JsonArray ?? new JsonArray();
    }
}