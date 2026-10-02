#region
using System.Text.Json.Serialization;
#endregion

namespace AL.APIClient.Response;

/// <summary>
///     Represents the data received when trying to log in.
/// </summary>
public sealed record LoginResponse
{
    /// <summary>
    ///     Whether the server rejected the login. <see cref="Reason" /> says why.
    /// </summary>
    [JsonPropertyName("failed")]
    public bool Failed { get; init; }

    /// <summary>
    ///     The game, even on the electron client, is basically a website.
    ///     <br />
    ///     If this is populated, you successfully logged in and this is the html response sent back.
    /// </summary>
    [JsonPropertyName("html")]
    public string? Html { get; init; }

    /// <summary>
    ///     If something went wrong when trying to log in, this is the error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>
    ///     The machine readable failure code, such as <c>wrong_password</c>, <c>email_not_found</c> or
    ///     <c>cant_login_inside_bank</c>.
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    /// <summary>
    ///     The kind of message this is, such as <c>message</c>, <c>content</c>, <c>eval</c> or <c>refresh</c>.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }
}