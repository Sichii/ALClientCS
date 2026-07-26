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
    ///     Set when the server rejected the login. <see cref="Reason" /> says why.
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
    ///     The machine readable failure code. e.g. "wrong_password", "email_not_found", "cant_login_inside_bank".
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    /// <summary>
    ///     The kind of message this is. e.g. "message", "content", "eval", "refresh".
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }
}