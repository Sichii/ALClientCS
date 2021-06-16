using System;
using AL.Core.Definitions;
using AL.Core.Interfaces;

namespace AL.SocketClient.Model
{
    public record SlotItem : ITradeItem, IItem
    {
        public string AchievementName { get; init; }
        public float AchievementProgress { get; init; }
        public bool Buying { get; init; }
        public DateTime Expires { get; init; }
        public float Extra { get; init; }
        public float Gift { get; init; }
        public string GiveawayFrom { get; init; }
        public float GiveawayMins { get; init; }
        public string[] GiveawayParticipants { get; init; }
        public float Grace { get; init; }
        public float Grave { get; init; }
        public string Id { get; init; }
        public bool IsElixir { get; init; }
        public int Level { get; init; }
        public LockType LockType { get; init; }
        public string Name { get; init; }
        public string[] PossiblePrefixes { get; init; }
        public string Prefix { get; init; }
        public long Price { get; init; }
        public int Quantity { get; init; }
        public ALAttribute StatType { get; init; }
        public string Volatile { get; init; }
    }
}