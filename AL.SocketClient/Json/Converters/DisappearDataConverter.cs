using System;
using AL.Core.Geometry;
using AL.Core.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.SocketClient.Json.Converters
{
    public class DisappearDataConverter : JsonConverter<DisappearData>
    {
        public override DisappearData? ReadJson(
            JsonReader reader,
            Type objectType,
            DisappearData? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            var obj = serializer.Deserialize<JObject>(reader);

            if (obj == null)
                return default;

            var spawn = default(Orientation);
            var spawnId = default(int?);

            if (obj.TryGetValue("s", out var token))
            {
                if (token.Type == JTokenType.Array)
                {
                    var converter = ArrayToObjectConverter<Orientation>.Singleton;
                    var tokenReader = token.CreateReader();

                    if (tokenReader.TokenType == JsonToken.None)
                        tokenReader.Read();

                    spawn = (Orientation?)converter.ReadJson(tokenReader, typeof(Orientation), null, serializer);
                } else
                    spawnId = spawn == default ? obj.Value<int?>("s") : null;
            }

            var result = new DisappearData();
            serializer.Populate(obj.CreateReader(), result);
            result.ToSpawnId = spawnId;
            result.ToOrientation = spawn;

            return result;
        }

        public override void WriteJson(JsonWriter writer, DisappearData? value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}