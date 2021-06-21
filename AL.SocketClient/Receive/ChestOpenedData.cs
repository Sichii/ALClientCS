using System.Collections.Generic;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    public record ChestOpenedData
    {
        public int Gold { get; set; }
        [JsonProperty("goldm")]
        public float GoldMod { get; set; }
        public bool Gone { get; set; }
        public string Id { get; set; }
        public IEnumerable<ChestItem> Items { get; set; } = new List<ChestItem>();
        [JsonProperty("opener")]
        public string OpenerName { get; set; }
        public bool Party { get; set; }
    }
}