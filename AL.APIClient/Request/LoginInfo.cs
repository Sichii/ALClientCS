#region
using Newtonsoft.Json;
#endregion

namespace AL.APIClient.Request;

public sealed record LoginInfo
{
    [JsonProperty]
    internal string Email { get; init; } = null!;

    [JsonProperty]
    internal string Password { get; init; } = null!;
}