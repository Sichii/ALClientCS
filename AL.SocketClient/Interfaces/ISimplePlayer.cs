using AL.Core.Definitions;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface ISimplePlayer
    {
        /// <summary>
        ///     Whether or not the player is AFK. "CODE" results in <c>true</c> here.
        /// </summary>
        [JsonConverter(typeof(AfkConverter))]
        bool AFK { get; }

        /// <summary>
        ///     The age of the character in days.
        /// </summary>
        [JsonProperty]
        int Age { get; }

        [JsonProperty("type")]
        ALClass Class { get; }

        [JsonProperty]
        int Level { get; }

        /// <summary>
        ///     The map this player is on.
        /// </summary>
        [JsonProperty]
        string Map { get; }

        [JsonProperty]
        string Name { get; }

        /// <summary>
        ///     If populated, this player is in a party. <br />
        ///     This is the name of the player who created the party.
        /// </summary>
        [JsonProperty("party")]
        string? PartyLeader { get; }
    }
}