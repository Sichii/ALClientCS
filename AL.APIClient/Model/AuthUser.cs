using System;
using System.Text.RegularExpressions;
using AL.APIClient.Request;

namespace AL.APIClient.Model
{
    public record AuthUser
    {
        public string AuthKey { get; }
        public DateTime Expires { get; }
        internal LoginInfo LoginInfo { get; }
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
            var str = match.Groups[1].Value.Replace("-", " ");

            if (DateTime.TryParse(str, out var expires))
                Expires = expires.ToUniversalTime();
        }

        public override string ToString() => $"{UserID}-{AuthKey}";
    }
}