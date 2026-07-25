#region
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
#endregion

namespace AL.Data.Tokens
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class TokensDatum : DatumBase<IReadOnlyDictionary<string, float>>
    {
        [JsonProperty("friendtoken")]
        [JsonPropertyName("friendtoken")]
        public IReadOnlyDictionary<string, float> Friendtoken { get; init; } = null!;

        [JsonProperty("funtoken")]
        [JsonPropertyName("funtoken")]
        public IReadOnlyDictionary<string, float> Funtoken { get; init; } = null!;

        [JsonProperty("monstertoken")]
        [JsonPropertyName("monstertoken")]
        public IReadOnlyDictionary<string, float> Monstertoken { get; init; } = null!;

        [JsonProperty("pvptoken")]
        [JsonPropertyName("pvptoken")]
        public IReadOnlyDictionary<string, float> Pvptoken { get; init; } = null!;
    }
}