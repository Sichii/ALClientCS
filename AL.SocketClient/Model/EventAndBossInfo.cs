using System.Collections.Generic;
using AL.SocketClient.Json.Converters;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents all of the data for ongoing events and bosses for a server.
    /// </summary>
    [JsonConverter(typeof(EventAndBossDataConverter))]
    public record EventAndBossInfo
    {
        [JsonProperty]
        public bool EggHunt { get; init; }

        [JsonProperty]
        public bool HolidaySeason { get; init; }

        [JsonProperty]
        public bool LunaryNewYear { get; init; }

        [JsonProperty]
        public bool Valentines { get; init; }

        /// <summary>
        ///     Contains information about bosses on this server.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<string, BossInfo> BossInfo { get; } = new Dictionary<string, BossInfo>();
    }
}