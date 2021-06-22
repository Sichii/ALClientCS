using System.Collections.Generic;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    public record PartyUpdateData
    {
        [JsonProperty("list")]
        public string MemberNames { get; init; }
        [JsonProperty("party")]
        public IReadOnlyDictionary<string, PartyMember> Members { get; init; } = new Dictionary<string, PartyMember>();
        public string Message { get; init; }
    }
}