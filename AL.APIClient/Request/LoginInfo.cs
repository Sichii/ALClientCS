#region
using System.Text.Json.Serialization;
#endregion

namespace AL.APIClient.Request;

public sealed record LoginInfo
{
    [JsonInclude]
    internal string Email { get; init; } = null!;

    [JsonInclude]
    internal string Password { get; init; } = null!;
}