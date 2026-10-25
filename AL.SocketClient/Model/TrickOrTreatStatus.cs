#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents this character's Trick or Treat day, carried on the character frame while Halloween runs.
/// </summary>
public sealed record TrickOrTreatStatus
{
    /// <summary>
    ///     The indexes into <c>G.events.trickortreat.doors</c> this character has knocked on since UTC midnight.
    /// </summary>
    [JsonPropertyName("doors")]
    public IReadOnlyList<int> Doors { get; init; } = [];

    /// <summary>
    ///     How many more knocks this character can make today: its unknocked doors, capped by the account's knocks left.
    /// </summary>
    [JsonPropertyName("doors_left")]
    public int DoorsLeft { get; init; }

    /// <summary>
    ///     The account's knocks this year. Every tenth pays a Witch's Parcel.
    /// </summary>
    [JsonPropertyName("stamps")]
    public int Stamps { get; init; }
}