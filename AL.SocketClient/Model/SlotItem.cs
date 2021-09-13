using System;
using System.Collections.Generic;
using AL.APIClient.Interfaces;
using AL.Core.Definitions;
using AL.SocketClient.Interfaces;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents an item in a <see cref="Player" />'s <see cref="Player.Slots" />. <br />
    ///     These items can be either <see cref="ITradeItem" />s or an equipped <see cref="IInventoryItem" />.
    /// </summary>
    /// <seealso cref="ITradeItem" />
    /// <seealso cref="IInventoryItem" />
    public record SlotItem : ITradeItem, IInventoryItem
    {
        public string? AchievementName { get; init; }
        public float AchievementProgress { get; init; }
        public bool Buying { get; init; }
        public DateTime? Expires { get; init; }
        public float Extra { get; init; }
        public float Gift { get; init; }
        public string? GiveawayFrom { get; init; }
        public float? GiveawayMins { get; init; }
        public IReadOnlyList<string>? GiveawayParticipants { get; init; }
        public float Grace { get; init; }
 #pragma warning disable 8766
        /// <summary>
        ///     If populated, this is an <see cref="ITradeItem" />. <br />
        ///     <inheritdoc cref="ITradeItem.Id" />
        /// </summary>
        public string? Id { get; init; }
 #pragma warning restore 8766
        public int Level { get; init; }
        public LockType LockType { get; init; }
        public string Name { get; init; } = null!;
        public IReadOnlyList<string> PossiblePrefixes { get; init; } = new List<string>();
        public string? Prefix { get; init; }
        public long Price { get; init; }
        public int Quantity { get; init; } = 1;
        public ALAttribute StatType { get; init; }
        public string? Volatile { get; init; }
    }
}