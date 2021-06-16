using System;
using System.Collections.Generic;
using AL.SocketClient.Model;
using AL.SocketClient.Receive;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.SocketClient.Json.Converters
{
    public class ServerInfoDataConverter : JsonConverter<ServerInfoData>
    {
        public override ServerInfoData ReadJson(JsonReader reader, Type objectType, ServerInfoData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var obj = serializer.Deserialize<JObject>(reader);

            if (obj == null)
                return null;

            var info = new ServerInfoData { BossInfo = new Dictionary<string, BossInfo>(StringComparer.OrdinalIgnoreCase) };
            dynamic dObj = info;

            foreach ((var key, var token) in obj)
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (token.Type == JTokenType.Boolean)
                    dObj[key] = token.Value<bool>();
                else if (token.Type == JTokenType.Object)
                    dObj.BossInfo[key] = token.ToObject<BossInfo>(serializer);

            return info;
        }

        public override void WriteJson(JsonWriter writer, ServerInfoData value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}