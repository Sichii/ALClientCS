using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    public record QueuedActionInfo : IUpdateable<QueuedActionInfo>
    {
        [JsonProperty]
        public QueuedAction Compound { get; set; }
        [JsonProperty]
        public QueuedAction Exchange { get; set; }

        [JsonProperty]
        public QueuedAction Upgrade { get; set; }

        public void Update(QueuedActionInfo other)
        {
            if (other.Compound != null)
                Compound = other.Compound;

            if (other.Upgrade != null)
                Upgrade = other.Upgrade;

            if (other.Exchange != null)
                Exchange = other.Exchange;
        }
    }
}