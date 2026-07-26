#region
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.SocketClient.Model;
#endregion

namespace AL.SocketClient.SocketModel;

/// <seealso cref="Character" />
/// <inheritdoc cref="Character" />
public class CharacterData : Character
{
    /// <summary>
    ///     When receiving character information, the action(s) that caused the character data to be re-sent will be attached
    ///     to the character data here.
    /// </summary>
    [JsonPropertyName("hitchhikers")]
    public IReadOnlyList<JsonArray> ExtraEvents { get; init; } = new List<JsonArray>();
}