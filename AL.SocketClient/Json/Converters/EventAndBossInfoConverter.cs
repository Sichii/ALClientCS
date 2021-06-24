using System;
using System.Collections.Generic;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.SocketClient.Json.Converters
{
    public class EventAndBossInfoConverter : JsonConverter<EventAndBossInfo>
    {
        public override EventAndBossInfo ReadJson(
            JsonReader reader,
            Type objectType,
            EventAndBossInfo existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var obj = serializer.Deserialize<JObject>(reader);

            if (obj == null)
                return null;

            var info = new EventAndBossInfo();
            var bossInfo = (Dictionary<string, BossInfo>) info.BossInfo;
            serializer.Populate(obj.CreateReader(), info);

            foreach ((var property, var token) in obj)
                if (token.Type == JTokenType.Object)
                    bossInfo[property] = token.ToObject<BossInfo>(serializer);

            return info;
        }

        public override void WriteJson(JsonWriter writer, EventAndBossInfo value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}