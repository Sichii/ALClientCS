﻿#region
using System;
using AL.Core.Definitions;
using AL.Core.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace AL.Core.Json.Converters;

/// <summary>
///     Provides conversion logic specifically for <see cref="ALClass" />.
/// </summary>
/// <seealso cref="JsonConverter" />
public sealed class ALClassConverter : JsonConverter<ALClass>
{
    public override ALClass ReadJson(
        JsonReader reader,
        Type objectType,
        ALClass existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType is JsonToken.Null or JsonToken.Boolean)
            return ALClass.None;

        var obj = serializer.Deserialize<JToken>(reader);

        return EnumHelper.TryParse(obj?.Value<string>(), out ALClass @class) ? @class : ALClass.NPC;
    }

    public override void WriteJson(JsonWriter writer, ALClass value, JsonSerializer serializer) => throw new NotImplementedException();
}