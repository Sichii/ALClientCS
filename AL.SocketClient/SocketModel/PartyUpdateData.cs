#region
using System.Collections.Generic;
using AL.Core.Json.Converters;
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
    /// <remarks>
    ///     Arrives as <c>false</c> rather than an empty array when the party dissolves.
    /// </remarks>
    [JsonProperty("list")]
    [JsonConverter(typeof(FalsyListConverter<string>))]
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

    /// <summary>
    ///     True when this update was caused by a member leaving the party; absent (false) otherwise.
    /// </summary>
    /// <remarks>
    ///     The server sends the integer <c>1</c> on the wire; Newtonsoft coerces it to <c>true</c>.
    /// </remarks>
    [JsonProperty("leave")]
    public bool Leave { get; init; }
}