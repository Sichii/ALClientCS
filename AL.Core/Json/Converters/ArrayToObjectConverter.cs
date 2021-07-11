﻿using System;
using System.Linq;
using System.Reflection;
using AL.Core.Json.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.Core.Json.Converters
{
    /// <summary>
    ///     Provides conversion logic for objects that are represented as an array of property values.
    ///     Implements <see cref="Newtonsoft.Json.JsonConverter" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Newtonsoft.Json.JsonConverter" />
    public class ArrayToObjectConverter<T> : JsonConverter
    {
        public static readonly ArrayToObjectConverter<T> Singleton = new();

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType) => objectType == typeof(T);

        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
                return default;

            var array = JArray.Load(reader);

            var propsByIndex = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Concat<MemberInfo>(
                    typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                .Select(p => new
                {
                    p,
                    p.GetCustomAttribute<JsonArrayIndexAttribute>()?.Index
                })
                .Where(set => set.Index.HasValue)
                .ToDictionary(set => set.Index!.Value, set => set.p);

            var obj = new JObject(array
                .Select((jt, i) => propsByIndex.TryGetValue(i, out var prop) ? new JProperty(prop.Name, jt) : null)
                .Where(jp => jp != null));

            return obj.ToObject<T>();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var propsByIndex = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Concat<MemberInfo>(
                    typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                .Select(p => new
                {
                    p,
                    p.GetCustomAttribute<JsonArrayIndexAttribute>()?.Index
                })
                .Where(set => set.Index.HasValue)
                .ToDictionary(set => set.Index!.Value, set => set.p);

            var arr = propsByIndex.OrderBy(kvp => kvp.Key)
                .Select(kvp =>
                {
                    return kvp.Value switch
                    {
                        FieldInfo fieldInfo       => fieldInfo.GetValue(value),
                        PropertyInfo propertyInfo => propertyInfo.GetValue(value),
                        _                         => throw new ArgumentOutOfRangeException(nameof(kvp.Value))
                    };
                })
                .ToArray();

            serializer.Serialize(writer, arr);
        }
    }
}