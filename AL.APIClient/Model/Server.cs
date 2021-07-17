using AL.APIClient.Definitions;
using Newtonsoft.Json;

namespace AL.APIClient.Model
{
    /// <summary>
    /// Represents a server that a character could log into.
    /// </summary>
    public record Server
    {
        /// <summary>
        /// The server identifier.
        /// </summary>
        [JsonProperty("name")]
        public ServerId Identifier { get; set; }

        /// <summary>
        /// The IP address of the server.
        /// </summary>
        [JsonProperty("addr")]
        public string IPAddress { get; init; } = null!;

        /// <summary>
        /// A combination of <see cref="AL.APIClient.Definitions.ServerRegion"/> and <see cref="AL.APIClient.Definitions.ServerId"/>
        /// </summary>
        public string Key { get; init; } = null!;
        
        /// <summary>
        /// The number of players currently on this server.
        /// </summary>
        public int Players { get; init; }
        
        /// <summary>
        /// The port this server is on. (combine with IP address)
        /// </summary>
        public int Port { get; init; }
        
        /// <summary>
        /// The region this server is for.
        /// </summary>
        public ServerRegion Region { get; set; }
    }
}