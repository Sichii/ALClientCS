#region
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Reads a list the server sends as literal <c>false</c> (JavaScript's <c>a &amp;&amp; a</c> idiom) instead
///     of an empty array. The System.Text.Json replacement for the Newtonsoft <c>FalsyListConverter</c>.
/// </summary>
public sealed class FalsyListConverter<T> : JsonConverter<IReadOnlyList<T>>
{
    //a JSON null must reach Read so it maps to an empty list instead of a null reference
    public override bool HandleNull => true;

    public override IReadOnlyList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            //consume an unexpected container so the reader stays aligned; a no-op on a scalar like false
            reader.Skip();

            return new List<T>();
        }

        return JsonSerializer.Deserialize<List<T>>(ref reader, options) ?? new List<T>();
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<T> value, JsonSerializerOptions options) => throw new NotSupportedException();
}
