using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.Json.Converters
{
    public class PlayerConverter<T> : AttributedObjectConverter<T> where T: Player, new()
    {
        public override T ReadJson(
            JsonReader reader,
            Type objectType,
            T? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var player = base.ReadJson(reader, objectType, existingValue, hasExistingValue, serializer)
                         ?? throw new InvalidOperationException("Failed to deserialize player.");

            var playerSlots = (Dictionary<Slot, SlotItem?>)player.Slots;

            foreach (var member in Enum.GetValues<Slot>())
                if (!playerSlots.ContainsKey(member))
                    playerSlots[member] = null;

            return player;
        }
    }
}