using System.Collections.Generic;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    public record PartyUpdateData
    {
        [JsonProperty("list")]
        public IReadOnlyList<string> MemberNames { get; init; }
        [JsonProperty("party")]
        public IReadOnlyDictionary<string, PartyMember> Members { get; init; } = new Dictionary<string, PartyMember>();
        public string Message { get; init; }
    }
}