using AL.APIClient.Definitions;
using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents the data received when first loading into the server.
    /// </summary>
    public record WelcomeData : IInstancedLocation
    {
        /// <summary>
        ///     If populated, you logged into a character and this is it's data.
        /// </summary>
        public CharacterData? Character { get; init; }

        /// <summary>
        ///     The type of server.
        ///     TODO: make an enum for this (normal, hardcore)
        /// </summary>
        public string GamePlay { get; init; } = null!;

        [JsonProperty("name")]
        public ServerId Identifier { get; init; }

        public string In { get; init; } = null!;

        public string Map { get; init; } = null!;

        /// <summary>
        ///     Whether or not this is a PvP server.
        /// </summary>
        public bool PvP { get; init; }

        public ServerRegion Region { get; init; }

        public float X { get; init; }

        public float Y { get; init; }
        public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

        public virtual bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);
    }
}