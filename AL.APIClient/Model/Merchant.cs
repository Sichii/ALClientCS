using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Interfaces;

namespace AL.APIClient.Model
{
    public record Merchant : ILocation
    {
        public int Level { get; init; }
        public string Map { get; init; }
        public string Name { get; init; }
        public string Server { get; init; }
        public IReadOnlyDictionary<TradeSlot, TradeItem> Slots { get; init; }
        public string Stand { get; init; }
        public float X { get; init; }
        public float Y { get; init; }
    }
}