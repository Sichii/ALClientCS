#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     One encouragement bonus and whether it is paying, as the server reports it.
/// </summary>
/// <remarks>
///     All three appear every time, active or not, so <see cref="Reason" /> is the only thing that says why one is
///     missing.
/// </remarks>
public sealed record EncouragementBonus
{
    /// <summary>Whether this bonus is currently paying.</summary>
    [JsonPropertyName("active")]
    public bool Active { get; init; }

    /// <summary>
    ///     The condition this bonus rides on: <c>encouragement_new</c> , <c>encouragement_lonewolf</c> or
    ///     <c>encouragement_returning</c> .
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;

    /// <summary>
    ///     Why the bonus is not paying, or <c>active</c> when it is.
    /// </summary>
    /// <remarks>
    ///     <c>checking</c> means the account lookup has not landed yet and is worth retrying; <c>character_limit</c> means the
    ///     account holds 25 characters or more and no bonus will ever pay. The rest are settled facts about the character:
    ///     <c>expired</c> , <c>away</c> , <c>merchant</c> , <c>another_character</c> .
    /// </remarks>
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }
}