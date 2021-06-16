using System.Collections.Generic;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    public record PartyUpdateData
    {
        [JsonProperty("list")]
        public string MemberNames { get; init; }
        [JsonProperty("party")]
        public Dictionary<string, PartyMember> Members { get; init; }
        public string Message { get; init; }
    }
}