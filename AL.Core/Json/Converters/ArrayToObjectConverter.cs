using System;
using System.Linq;
using System.Reflection;
using AL.Core.Json.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.Core.Json.Converters
{
    public class ArrayToObjectConverter<T> : JsonConverter
    {
        public static readonly ArrayToObjectConverter<T> Singleton = new();

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType) => objectType == typeof(T);

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
                return default;

            var array = JArray.Load(reader);

            var propsByIndex = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Concat<MemberInfo>(
                    typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                .Where(p => p.GetCustomAttribute<JsonArrayIndexAttribute>() != null)
                .ToDictionary(p => p.GetCustomAttribute<JsonArrayIndexAttribute>()?.Index);

            var obj = new JObject(array
                .Select((jt, i) => propsByIndex.TryGetValue(i, out var prop) ? new JProperty(prop.Name, jt) : null)
                .Where(jp => jp != null));

            return obj.ToObject<T>();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var propsByIndex = value.GetType()
                .GetProperties()
                .Where(p => p.CanRead && p.GetCustomAttribute<JsonArrayIndexAttribute>() != null)
                .ToDictionary(p => p.GetCustomAttribute<JsonArrayIndexAttribute>()?.Index);

            var arr = propsByIndex.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value.GetValue(value)).ToArray();

            serializer.Serialize(writer, arr);
        }
    }
}