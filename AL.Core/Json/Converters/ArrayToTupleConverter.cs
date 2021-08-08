using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.Core.Json.Converters
{
    public class ArrayToTupleConverter<T1, T2> : JsonConverter<ValueTuple<T1?, T2?>>
    {
        public override ValueTuple<T1?, T2?> ReadJson(
            JsonReader reader,
            Type objectType,
            ValueTuple<T1?, T2?> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            var arr = serializer.Deserialize<JArray>(reader);

            return arr == null
                ? default
                : arr.Count switch
                {
                    1 => new ValueTuple<T1?, T2?>(arr[0].ToObject<T1>(serializer), default),
                    2 => new ValueTuple<T1?, T2?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer)),
                    _ => default
                };
        }

        public override void WriteJson(JsonWriter writer, ValueTuple<T1?, T2?> value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }

    public class ArrayToTupleConverter<T1, T2, T3> : JsonConverter<ValueTuple<T1?, T2?, T3?>>
    {
        public override ValueTuple<T1?, T2?, T3?> ReadJson(
            JsonReader reader,
            Type objectType,
            ValueTuple<T1?, T2?, T3?> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            var arr = serializer.Deserialize<JArray>(reader);

            return arr == null
                ? default
                : arr.Count switch
                {
                    1 => new ValueTuple<T1?, T2?, T3?>(arr[0].ToObject<T1>(serializer), default, default),
                    2 => new ValueTuple<T1?, T2?, T3?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer), default),
                    3 => new ValueTuple<T1?, T2?, T3?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer)),
                    _ => default
                };
        }

        public override void WriteJson(JsonWriter writer, ValueTuple<T1?, T2?, T3?> value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }

    public class ArrayToTupleConverter<T1, T2, T3, T4> : JsonConverter<ValueTuple<T1?, T2?, T3?, T4?>>
    {
        public override ValueTuple<T1?, T2?, T3?, T4?> ReadJson(
            JsonReader reader,
            Type objectType,
            ValueTuple<T1?, T2?, T3?, T4?> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            var arr = serializer.Deserialize<JArray>(reader);

            return arr == null
                ? default
                : arr.Count switch
                {
                    1 => new ValueTuple<T1?, T2?, T3?, T4?>(arr[0].ToObject<T1>(serializer), default, default, default),
                    2 => new ValueTuple<T1?, T2?, T3?, T4?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer), default,
                        default),
                    3 => new ValueTuple<T1?, T2?, T3?, T4?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), default),
                    4 => new ValueTuple<T1?, T2?, T3?, T4?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), arr[3].ToObject<T4>(serializer)),
                    _ => default
                };
        }

        public override void WriteJson(JsonWriter writer, ValueTuple<T1?, T2?, T3?, T4?> value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }

    public class ArrayToTupleConverter<T1, T2, T3, T4, T5> : JsonConverter<ValueTuple<T1?, T2?, T3?, T4?, T5?>>
    {
        public override ValueTuple<T1?, T2?, T3?, T4?, T5?> ReadJson(
            JsonReader reader,
            Type objectType,
            ValueTuple<T1?, T2?, T3?, T4?, T5?> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            var arr = serializer.Deserialize<JArray>(reader);

            return arr == null
                ? default
                : arr.Count switch
                {
                    1 => new ValueTuple<T1?, T2?, T3?, T4?, T5?>(arr[0].ToObject<T1>(serializer), default, default, default, default),
                    2 => new ValueTuple<T1?, T2?, T3?, T4?, T5?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer), default,
                        default, default),
                    3 => new ValueTuple<T1?, T2?, T3?, T4?, T5?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), default, default),
                    4 => new ValueTuple<T1?, T2?, T3?, T4?, T5?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), arr[3].ToObject<T4>(serializer), default),
                    5 => new ValueTuple<T1?, T2?, T3?, T4?, T5?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), arr[3].ToObject<T4>(serializer), arr[4].ToObject<T5>(serializer)),
                    _ => default
                };
        }

        public override void WriteJson(JsonWriter writer, ValueTuple<T1?, T2?, T3?, T4?, T5?> value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }

    public class ArrayToTupleConverter<T1, T2, T3, T4, T5, T6> : JsonConverter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>
    {
        public override ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> ReadJson(
            JsonReader reader,
            Type objectType,
            ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            var arr = serializer.Deserialize<JArray>(reader);

            return arr == null
                ? default
                : arr.Count switch
                {
                    1 => new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>(arr[0].ToObject<T1>(serializer), default, default, default, default,
                        default),
                    2 => new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        default, default, default, default),
                    3 => new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), default, default, default),
                    4 => new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), arr[3].ToObject<T4>(serializer), default, default),
                    5 => new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), arr[3].ToObject<T4>(serializer), arr[4].ToObject<T5>(serializer), default),
                    6 => new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), arr[3].ToObject<T4>(serializer), arr[4].ToObject<T5>(serializer),
                        arr[5].ToObject<T6>(serializer)),
                    _ => default
                };
        }

        public override void WriteJson(JsonWriter writer, ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }

    public class ArrayToTupleConverter<T1, T2, T3, T4, T5, T6, T7> : JsonConverter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
    {
        public override ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> ReadJson(
            JsonReader reader,
            Type objectType,
            ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            var arr = serializer.Deserialize<JArray>(reader);

            return arr == null
                ? default
                : arr.Count switch
                {
                    1 => new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(arr[0].ToObject<T1>(serializer), default, default, default,
                        default, default, default),
                    2 => new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        default, default, default, default, default),
                    3 => new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), default, default, default, default),
                    4 => new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), arr[3].ToObject<T4>(serializer), default, default, default),
                    5 => new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), arr[3].ToObject<T4>(serializer), arr[4].ToObject<T5>(serializer), default,
                        default),
                    6 => new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), arr[3].ToObject<T4>(serializer), arr[4].ToObject<T5>(serializer),
                        arr[5].ToObject<T6>(serializer), default),
                    7 => new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(arr[0].ToObject<T1>(serializer), arr[1].ToObject<T2>(serializer),
                        arr[2].ToObject<T3>(serializer), arr[3].ToObject<T4>(serializer), arr[4].ToObject<T5>(serializer),
                        arr[5].ToObject<T6>(serializer), arr[6].ToObject<T7>()),
                    _ => default
                };
        }

        public override void WriteJson(JsonWriter writer, ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}