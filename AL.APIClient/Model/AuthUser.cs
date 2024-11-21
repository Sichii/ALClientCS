#region
using System;
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
    ///     The user's id.
    /// </summary>
    public long UserID { get; }

    internal AuthUser(LoginInfo loginInfo, string cookie)
    {
        LoginInfo = loginInfo;

        var match = Regex.Match(cookie, @"^auth=(\d+)-(\w+);");

        if (long.TryParse(match.Groups[1].Value, out var userId))
        {
            UserID = userId;
            AuthKey = match.Groups[2].Value;
        } else
            throw new Exception($"Unexpected cookie value of {cookie}");

        match = Regex.Match(cookie, @"expires=(.+)$");

        var str = match.Groups[1]
                       .Value
                       .Replace("-", " ");

        if (DateTime.TryParse(str, out var expires))
            Expires = expires.ToUniversalTime();
    }

    public override string ToString() => $"{UserID}-{AuthKey}";
}