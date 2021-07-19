using System;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.Json.Converters
{
    public class CharacterConverter<T> : AttributedObjectConverter<T> where T: Character, IAttributed, new()
    {
        public override T ReadJson(
            JsonReader reader,
            Type objectType,
            T? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var character = base.ReadJson(reader, objectType, existingValue, hasExistingValue, serializer)
                            ?? throw new InvalidOperationException("Failed to deserialize character.");

            character.Inventory.SetCapacity(character.InventorySize);

            return character;
        }
    }
}