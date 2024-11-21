#region
using System.Collections.Generic;
using AL.SocketClient.Model;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when the server updates us on the status of the party. (this happens periodically)
/// </summary>
public sealed record PartyUpdateData
{
    /// <summary>
    ///     A list of the names of everyone in your party.
    /// </summary>
    [JsonProperty("list")]
    public IReadOnlyList<string> MemberNames { get; init; } = new List<string>();

    /// <summary>
    ///     A collection of basic information for each person in the party.
    /// </summary>
    [JsonProperty("party")]
    public IReadOnlyDictionary<string, PartyMember> Members { get; init; } = new Dictionary<string, PartyMember>();

    /// <summary>
    ///     If populated, describes what caused this party update.
    /// </summary>
    public string? Message { get; init; }
}