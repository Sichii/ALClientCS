using System.Collections.Generic;
using AL.SocketClient.Json.Converters;
using AL.SocketClient.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.SocketClient.SocketModel
{
    /// <seealso cref="Character" />
    /// <inheritdoc cref="Character" />
    [JsonConverter(typeof(CharacterConverter<CharacterData>))]
    public class CharacterData : Character
    {
        /// <summary>
        ///     When receiving character information, the action(s) that caused the character data to be re-sent will be attached
        ///     to the character data here.
        /// </summary>
        [JsonProperty("hitchhikers")]
        public IReadOnlyList<JArray> ExtraEvents { get; init; } = new List<JArray>();
    }
}