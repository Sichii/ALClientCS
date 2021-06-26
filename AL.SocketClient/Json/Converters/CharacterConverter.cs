using System;
using System.Linq;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Json.Converters
{
    public class CharacterConverter<T> : AttributedObjectConverter<T> where T : Character, IAttributed, new()
    {
        public override T ReadJson(
            JsonReader reader,
            Type objectType,
            T existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var character = base.ReadJson(reader, objectType, existingValue, hasExistingValue, serializer);
            var missing = character.InventorySize - character.Items.Count;

            if (missing > 0)
            {
                var newInventory = new Item[character.InventorySize];

                for (var i = 0; i < character.Items.Count; i++)
                    newInventory[i] = character.Items[i];
            }

            return character;
        }
    }
}