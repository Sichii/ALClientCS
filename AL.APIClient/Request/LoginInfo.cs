using Newtonsoft.Json;

namespace AL.APIClient.Request
{
    internal record LoginInfo
    {
        [JsonProperty]
        internal string Email { get; init; } = null!;
        [JsonProperty]
        internal string Password { get; init; } = null!;
    }
}