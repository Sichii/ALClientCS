#region
using System.Text.Json.Serialization;
using AL.APIClient.Interfaces;
using AL.Core.Definitions;
#endregion

namespace AL.APIClient.Model;

/// <summary>
///     Represents an item in a merchant's stand.
/// </summary>
/// <seealso cref="ITradeItem" />
public sealed record TradeItem : ITradeItem
{
    [JsonPropertyName("ach")]
    public string? AchievementName { get; init; }

    [JsonPropertyName("b")]
    public bool Buying { get; init; }

    [JsonPropertyName("giveaway")]
    public float? GiveawayMins { get; init; }

    [JsonPropertyName("list")]
    public IReadOnlyList<string>? GiveawayParticipants { get; init; }

    public float Grace { get; init; }

    [JsonPropertyName("rid")]
    public string Id { get; init; } = null!;

    public int Level { get; init; }
    public string Name { get; init; } = null!;

    [JsonPropertyName("p")]
    public string? Prefix { get; init; }

    public long Price { get; init; }

    [JsonPropertyName("q")]
    public int Quantity { get; init; } = 1;

    [JsonPropertyName("stat_type")]
    public ALAttribute StatType { get; init; }
}