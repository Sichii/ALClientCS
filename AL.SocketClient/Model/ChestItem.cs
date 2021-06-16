using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    public class ChestItem
    {
        public int Level { get; set; }
        [JsonProperty("looter")]
        public string LooterName { get; set; }
        public string Name { get; set; }
        [JsonProperty("q")]
        public int Quantity { get; set; }
    }
}