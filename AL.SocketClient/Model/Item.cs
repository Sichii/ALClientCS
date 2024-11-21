#region
using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.SocketClient.Interfaces;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     <inheritdoc cref="IInventoryItem" />
/// </summary>
/// <seealso cref="IInventoryItem" />
public sealed record Item : IInventoryItem
{
    public string? AchievementName { get; init; }

    public float AchievementProgress { get; init; }

    public DateTime? Expires { get; init; }

    public float Extra { get; init; }

    public float Gift { get; init; }

    public string? GiveawayFrom { get; init; }

    public float Grace { get; init; }

    public int Level { get; init; }

    public LockType LockType { get; init; }

    public string Name { get; init; } = null!;

    public IReadOnlyList<string> PossiblePrefixes { get; init; } = new List<string>();

    /// <summary>
    ///     If populated, this item is in the process of being upgraded or compounded, and this contains details about the
    ///     chance of it succeeding.
    /// </summary>
    [JsonProperty("p")]
    public Prediction? Prediction { get; init; }

    public int Quantity { get; init; } = 1;

    public ALAttribute StatType { get; init; }

    public string? Volatile { get; init; }
}