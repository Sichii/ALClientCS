#region
using System.Collections.Generic;
using AL.APIClient.Interfaces;
using AL.Core.Definitions;
using Newtonsoft.Json;
#endregion

namespace AL.APIClient.Model;

/// <summary>
///     Represents an item in a merchant's stand.
/// </summary>
/// <seealso cref="ITradeItem" />
public sealed record TradeItem : ITradeItem
{
    [JsonProperty("ach")]
    public string? AchievementName { get; init; }

    [JsonProperty("b")]
    public bool Buying { get; init; }

    [JsonProperty("giveaway")]
    public float? GiveawayMins { get; init; }

    [JsonProperty("list")]
    public IReadOnlyList<string>? GiveawayParticipants { get; init; }

    public float Grace { get; init; }

    [JsonProperty("rid")]
    public string Id { get; init; } = null!;
    public int Level { get; init; }
    public string Name { get; init; } = null!;

    [JsonProperty("p")]
    public string? Prefix { get; init; }

    public long Price { get; init; }

    [JsonProperty("q")]
    public int Quantity { get; init; } = 1;

    [JsonProperty("stat_type")]
    public ALAttribute StatType { get; init; }
}