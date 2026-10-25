#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents the account's Witch's Cipher day, carried on the character frame while a round runs.
/// </summary>
public sealed record WitchCipherStatus
{
    /// <summary>
    ///     How many more mixes the account can brew today on this server. Zero once the day's mixes were brewed on another.
    /// </summary>
    [JsonPropertyName("mixes_left")]
    public int MixesLeft { get; init; }

    /// <summary>
    ///     The Witch's Parcels the account has been paid today.
    /// </summary>
    [JsonPropertyName("parcels")]
    public int Parcels { get; init; }

    /// <summary>
    ///     If populated, the server the account brewed on today, when that is not this one.
    /// </summary>
    [JsonPropertyName("server")]
    public string? Server { get; init; }
}