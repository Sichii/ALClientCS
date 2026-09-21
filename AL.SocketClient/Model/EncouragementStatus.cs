#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     The account-wide reward bonuses this character is earning, carried on its own character frame.
/// </summary>
/// <remarks>
///     The bonuses are account-wide but the payout is per character and follows contribution: the server keeps a damage,
///     tanking and healing ledger on each monster and settles the extra at loot time, in a chest reserved to the
///     contributor. So <see cref="Totals" /> is what this character would earn on a monster it killed alone, not a
///     guaranteed rate.
/// </remarks>
public sealed record EncouragementStatus
{
    /// <summary>
    ///     Whether no bonus can pay right now. Read <see cref="EncouragementBonus.Reason" /> for which of the two causes it
    ///     is: an account lookup still in flight, or an account holding too many characters to qualify at all.
    /// </summary>
    [JsonPropertyName("blocked")]
    public bool Blocked { get; init; }

    /// <summary>
    ///     If populated, how many characters the account holds. Null until the lookup lands.
    /// </summary>
    [JsonPropertyName("characterCount")]
    public int? CharacterCount { get; init; }

    /// <summary>
    ///     Whether the account lookup behind these figures has landed.
    /// </summary>
    [JsonPropertyName("groupReady")]
    public bool GroupReady { get; init; }

    /// <summary>
    ///     Every bonus and whether it is paying, in a fixed order the server does not vary.
    /// </summary>
    [JsonPropertyName("statuses")]
    public IReadOnlyList<EncouragementBonus> Statuses { get; init; } = new List<EncouragementBonus>();

    /// <summary>
    ///     What the active bonuses multiply together to, per reward.
    /// </summary>
    [JsonPropertyName("totals")]
    public EncouragementTotals Totals { get; init; } = new();
}