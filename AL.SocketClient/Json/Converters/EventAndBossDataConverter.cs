#region
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace AL.SocketClient.Json.Converters;

public sealed class EventAndBossDataConverter : JsonConverter<EventAndBossData>
{
    private static readonly Action<BossInfo, string> SetValue;

    static EventAndBossDataConverter()
    {
        var bossInfoParam = Expression.Parameter(typeof(BossInfo), "bossInfo");
        var bossIdParam = Expression.Parameter(typeof(string), "bossId");

        var propertyInfo = typeof(BossInfo).GetProperty(nameof(BossInfo.Id))!;

        var bossIdProperty = Expression.Property(bossInfoParam, propertyInfo);
        var assignBossId = Expression.Assign(bossIdProperty, bossIdParam);

        SetValue = Expression.Lambda<Action<BossInfo, string>>(assignBossId, bossInfoParam, bossIdParam)
                             .Compile();
    }

    public override EventAndBossData? ReadJson(
        JsonReader reader,
        Type objectType,
        EventAndBossData? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var obj = serializer.Deserialize<JObject>(reader);

        if (obj == null)
            return null;

        var eventAndBossData = new EventAndBossData();
        var bossInfoDic = (Dictionary<string, BossInfo>)eventAndBossData.BossInfo;
        serializer.Populate(obj.CreateReader(), eventAndBossData);

        foreach ((var key, var token) in obj)
            if (token!.Type == JTokenType.Object)
            {
                var bossInfo = token.ToObject<BossInfo>(serializer)
                               ?? throw new InvalidOperationException("Failed to deserialize boss info.");

                SetValue(bossInfo, key);
                bossInfoDic[key] = bossInfo;
            }

        return eventAndBossData;
    }

    public override void WriteJson(JsonWriter writer, EventAndBossData? value, JsonSerializer serializer)
        => throw new NotImplementedException();
}