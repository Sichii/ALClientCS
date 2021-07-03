using AL.Core.Helpers;
using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    public record QueuedActionInfo : IMutable<QueuedActionInfo>
    {
        [JsonProperty]
        public QueuedAction Compound { get; set; }
        [JsonProperty]
        public QueuedAction Exchange { get; set; }

        [JsonProperty]
        public QueuedAction Upgrade { get; set; }
        public string Id { get; } = UniqueId.NextId.ToString();

        public virtual bool Equals(QueuedActionInfo other) => Id.Equals(other?.Id);

        public override int GetHashCode() => Id.GetHashCode();

        public void Mutate(QueuedActionInfo other)
        {
            if (other.Compound != null)
                Compound = other.Compound;

            if (other.Upgrade != null)
                Upgrade = other.Upgrade;

            if (other.Exchange != null)
                Exchange = other.Exchange;
        }

        public void Mutate(object other)
        {
            if (other is QueuedActionInfo info)
                Mutate(info);
        }
    }
}