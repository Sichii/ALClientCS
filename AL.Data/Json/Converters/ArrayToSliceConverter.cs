using System;
using AL.Data.Games;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.Data.Json.Converters
{
    public class ArrayToSliceConverter : JsonConverter<Slice>
    {
        public override Slice ReadJson(
            JsonReader reader,
            Type objectType,
            Slice existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var arr = serializer.Deserialize<JArray>(reader);

            if (arr == null)
                return null;

            var sliceName = arr.Value<string>(0);
            var rewardType = arr.Value<string>(1);
            var amount = 0;
            string rewardName;

            if (arr[2].Type == JTokenType.Integer)
            {
                amount = arr.Value<int>(2);
                rewardName = "gold";
            } else
                rewardName = arr.Value<string>(2);

            var description = arr.Value<string>(3);

            return new Slice
            {
                SliceName = sliceName,
                RewardType = rewardType,
                Amount = amount,
                RewardName = rewardName,
                Description = description
            };
        }

        public override void WriteJson(JsonWriter writer, Slice value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}