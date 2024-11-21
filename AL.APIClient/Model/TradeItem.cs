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
    public string? AchievementName { get; init; }

    public bool Buying { get; init; }

    public float? GiveawayMins { get; init; }

    public IReadOnlyList<string>? GiveawayParticipants { get; init; }

    public float Grace { get; init; }

    public string Id { get; init; } = null!;
    public int Level { get; init; }
    public string Name { get; init; } = null!;

    [JsonProperty("p")]
    public string? Prefix { get; init; }

    public long Price { get; init; }
    public int Quantity { get; init; } = 1;

    public ALAttribute StatType { get; init; }
}