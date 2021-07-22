using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.SocketClient.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.SocketClient.Json.Converters
{
    public class BankDataConverter : JsonConverter<BankInfo>
    {
        public override BankInfo? ReadJson(
            JsonReader reader,
            Type objectType,
            BankInfo? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var obj = serializer.Deserialize<JObject>(reader);

            if (obj == null)
                return null;

            var dic = new Dictionary<BankPack, IReadOnlyList<InventoryItem?>>();
            var gold = 0L;

            foreach ((var key, var token) in obj)
                if (key == "gold")
                    gold = token!.Value<long>();
                else if (EnumHelper.TryParse(key, out BankPack bankPack))
                    dic[bankPack] = token!.ToObject<InventoryItem[]>(serializer)
                                    ?? throw new InvalidOperationException("Failed to deserialize items.");

            return new BankInfo
            {
                Gold = gold,
                Items = new ReadOnlyDictionary<BankPack, IReadOnlyList<InventoryItem?>>(dic)
            };
        }

        public override void WriteJson(JsonWriter writer, BankInfo? value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}