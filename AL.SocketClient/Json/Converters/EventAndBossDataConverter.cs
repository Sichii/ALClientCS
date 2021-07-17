using System;
using System.Collections.Generic;
using System.Reflection;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.SocketClient.Json.Converters
{
    public class EventAndBossDataConverter : JsonConverter<EventAndBossData>
    {
        private static readonly PropertyInfo PropertyInfo = typeof(BossInfo).GetProperty(nameof(BossInfo.Id))!;

        public override EventAndBossData? ReadJson(
            JsonReader reader,
            Type objectType,
            EventAndBossData? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var obj = serializer.Deserialize<JObject>(reader);

            if (obj == null)
                return null;

            var eventAndBossData = new EventAndBossData();
            var bossInfoDic = (Dictionary<string, BossInfo>) eventAndBossData!.BossInfo;
            serializer.Populate(obj.CreateReader(), eventAndBossData);

            foreach ((var key, var token) in obj)
                if (token!.Type == JTokenType.Object)
                {
                    var bossInfo = token.ToObject<BossInfo>(serializer)
                                   ?? throw new InvalidOperationException("Failed to deserialize boss info.");

                    PropertyInfo.SetValue(bossInfo, key);
                    bossInfoDic[key] = bossInfo;
                }

            return eventAndBossData;
        }

        public override void WriteJson(JsonWriter writer, EventAndBossData? value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}