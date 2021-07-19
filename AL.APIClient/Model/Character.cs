using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.APIClient.Model
{
    /// <summary>
    ///     Represents a character this user owns.
    /// </summary>
    /// <seealso cref="IInstancedLocation" />
    public record Character : IInstancedLocation
    {
        /// <summary>
        ///     The id of the character. (this is not the name)
        /// </summary>
        public string Id { get; init; } = null!;

        public string In { get; init; } = null!;

        /// <summary>
        ///     The level of the character.
        /// </summary>
        public int Level { get; init; }
        public string Map { get; init; } = null!;

        /// <summary>
        ///     The name of the character.
        /// </summary>
        public string Name { get; init; } = null!;

        /// <summary>
        ///     Whether or not the character is already logged in.
        /// </summary>
        public bool Online { get; init; }

        /// <summary>
        ///     TODO: unknown
        /// </summary>
        public string? Secret { get; init; }

        /// <summary>
        ///     If the character is <see cref="Online" />, this is the <see cref="AL.APIClient.Definitions.ServerRegion" /> and
        ///     <see cref="AL.APIClient.Definitions.ServerId" />
        /// </summary>
        [JsonProperty("server")]
        public string? ServerKey { get; init; }
        public float X { get; init; }
        public float Y { get; init; }
    }
}