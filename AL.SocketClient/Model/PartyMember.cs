using AL.Core.Definitions;
using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents a member of your party.
    /// </summary>
    /// <seealso cref="ILocation" />
    public record PartyMember : IInstancedLocation
    {
        [JsonProperty("type")]
        public ALClass Class { get; init; }
        public int Gold { get; init; }
        public string In { get; init; } = null!;
        public int Level { get; init; }
        public int Luck { get; init; }
        public string Map { get; init; } = null!;

        /// <summary>
        ///     The maximum party size this member can be apart of.
        /// </summary>
        [JsonProperty("l")]
        public int PartyLimit { get; init; }
        public float Share { get; init; }
        public string Skin { get; init; } = null!;
        public float X { get; init; }
        public int XP { get; init; }
        public float Y { get; init; }
        public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

        public virtual bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);
    }
}