#region
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
#endregion

namespace AL.Core.Json.Converters;

/// <summary>
///     Reads a list that the server sends as <c>false</c> rather than as an empty array.
/// </summary>
/// <typeparam name="T">
///     The element type.
/// </typeparam>
/// <remarks>
///     Javascript's <c>a &amp;&amp; a</c> idiom yields <c>false</c> instead of an empty collection, so several
///     payloads flip between an array and a bare boolean depending on whether anything is in them.
/// </remarks>
/// <seealso cref="JsonConverter" />
public sealed class FalsyListConverter<T> : JsonConverter<IReadOnlyList<T>>
{
    public override IReadOnlyList<T>? ReadJson(
        JsonReader reader,
        Type objectType,
        IReadOnlyList<T>? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.StartArray)
        {
            //consumes an unexpected container so the reader stays aligned; no-op on a scalar
            reader.Skip();

            return new List<T>();
        }

        return serializer.Deserialize<List<T>>(reader) ?? new List<T>();
    }

    public override void WriteJson(JsonWriter writer, IReadOnlyList<T>? value, JsonSerializer serializer)
        => throw new NotImplementedException();
}
