#region
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AL.SocketClient.Model;
using StjConverters = AL.Core.Json.SystemTextJson;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when the server updates us on the status of the party. (this happens periodically)
/// </summary>
public sealed record PartyUpdateData
{
    /// <summary>
    ///     True when this update was caused by a member leaving the party; absent (false) otherwise.
    /// </summary>
    /// <remarks>
    ///     The server sends the integer
    ///     <c>
    ///         1
    ///     </c>
    ///     on the wire; Newtonsoft coerces it to
    ///     <c>
    ///         true
    ///     </c>
    ///     .
    /// </remarks>
    [JsonPropertyName("leave")]
    public bool Leave { get; init; }

    /// <summary>
    ///     A list of the names of everyone in your party.
    /// </summary>
    /// <remarks>
    ///     Arrives as
    ///     <c>
    ///         false
    ///     </c>
    ///     rather than an empty array when the party dissolves.
    /// </remarks>
    [JsonPropertyName("list")]
    [JsonConverter(typeof(StjConverters.FalsyListConverter<string>))]
    public IReadOnlyList<string> MemberNames { get; init; } = new List<string>();

    /// <summary>
    ///     A collection of basic information for each person in the party.
    /// </summary>
    [JsonPropertyName("party")]
    public IReadOnlyDictionary<string, PartyMember> Members { get; init; } = new Dictionary<string, PartyMember>();

    /// <summary>
    ///     If populated, describes what caused this party update.
    /// </summary>
    public string? Message { get; init; }
}