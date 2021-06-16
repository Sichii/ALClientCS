using Newtonsoft.Json;

namespace AL.APIClient.Request
{
    internal record LoginInfo
    {
        [JsonProperty]
        internal string Email { get; init; }
        [JsonProperty("only_login")]
        internal bool OnlyLogin { get; set; } = true;
        [JsonProperty]
        internal string Password { get; init; }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}