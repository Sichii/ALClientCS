using System;
using AL.SocketClient.Definitions;
using AL.SocketClient.Receive;
using Newtonsoft.Json;

namespace AL.SocketClient.Json.Converters
{
    public class GameResponseDataConverter : JsonConverter<GameResponseData>
    {
        public override GameResponseData ReadJson(
            JsonReader reader,
            Type objectType,
            GameResponseData existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            GameResponseData result;

            switch (reader.TokenType)
            {
                case JsonToken.String:
                    result = new GameResponseData { ResponseType = serializer.Deserialize<GameResponseType>(reader) };
                    break;
                case JsonToken.StartObject:
                {
                    result = new GameResponseData
                    {
                        ContainsData = true
                    };

                    serializer.Populate(reader, result);
                    break;
                }
                default:
                    return default;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, GameResponseData value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}