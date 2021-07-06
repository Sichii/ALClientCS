using System.Collections.Generic;
using AL.Core.Abstractions;
using AL.Core.Definitions;

namespace AL.Data.NPCs
{
    public record NPC : AttributedRecordBase
    {
        public IReadOnlyDictionary<ALAttribute, float> Aura { get; init; } = new Dictionary<ALAttribute, float>();
        public string Color { get; init; }
        public string Id { get; init; }
        public bool Ignore { get; init; }
        public float Interval { get; init; }
        public string[] Items { get; init; }
        public float Level { get; init; }
        public string Name { get; init; }
        public BankPack Pack { get; init; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public IReadOnlyDictionary<string, int> Places { get; init; } = new Dictionary<string, int>();
        public Quest Quest { get; init; }
        public NPCRole Role { get; init; }
        public string Stand { get; init; }
        public Token Token { get; init; }
    }
}