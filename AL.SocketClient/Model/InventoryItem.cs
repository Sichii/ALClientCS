using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.SocketClient.Interfaces;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     <inheritdoc cref="IInventoryItem" />
    /// </summary>
    /// <seealso cref="IInventoryItem" />
    public record InventoryItem : IInventoryItem
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

        public string? Prefix { get; init; }

        public int Quantity { get; init; }

        public ALAttribute StatType { get; init; }

        public string? Volatile { get; init; }
    }
}