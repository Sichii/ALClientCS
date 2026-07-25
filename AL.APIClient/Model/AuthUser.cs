#region
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using AL.APIClient.Request;
#endregion

namespace AL.APIClient.Model;

/// <summary>
///     Represents a logged in user.
/// </summary>
public sealed record AuthUser
{
    /// <summary>
    ///     The user's authorization key.
    /// </summary>
    public string AuthKey { get; }

    /// <summary>
    ///     When the authorization expires.
    /// </summary>
    public DateTime Expires { get; }

    internal LoginInfo LoginInfo { get; }

    /// <summary>
    ///     The user's id. A "US_" prefixed alphanumeric string.
    /// </summary>
    public string UserID { get; }

    internal AuthUser(LoginInfo loginInfo, string cookie)
    {
        LoginInfo = loginInfo;

        //the server strips quotes and then splits the value on the first '-'
        //so neither the id nor the token can itself contain one
        var match = Regex.Match(cookie.Replace("\"", string.Empty), @"^auth=([^-;]+)-([^;]+)(?:;|$)", RegexOptions.IgnoreCase);

        //never put the cookie value itself in the message, it is a live credential
        if (!match.Success)
            throw new InvalidOperationException($"Unexpected auth cookie format. (length {cookie.Length})");

        UserID = match.Groups[1].Value;
        AuthKey = match.Groups[2].Value;

        match = Regex.Match(cookie, "expires=([^;]+)", RegexOptions.IgnoreCase);

        //the header is always RFC-1123 in English regardless of the machine's locale
        if (match.Success
            && DateTime.TryParse(
                match.Groups[1].Value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var expires))
            Expires = expires;
    }

    public override string ToString() => $"{UserID}-{AuthKey}";
}
