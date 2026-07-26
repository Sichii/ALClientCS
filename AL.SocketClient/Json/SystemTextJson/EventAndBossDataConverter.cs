#region
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Json.SystemTextJson;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
#endregion

namespace AL.SocketClient.Json.SystemTextJson;

/// <summary>
///     Deserializes <see cref="EventAndBossData" />: fills the declared fields, then treats every remaining object-valued
///     property as a <see cref="BossInfo" /> keyed by (and stamped with) its property name. The System.Text.Json
///     replacement for the Newtonsoft
///     <c>
///         EventAndBossDataConverter
///     </c>
///     . Register in the shared options so the declared-field fill drops this converter and cannot re-enter itself.
/// </summary>
public sealed class EventAndBossDataConverter : JsonConverter<EventAndBossData>
{
    private static readonly Action<BossInfo, string> SetId;

    static EventAndBossDataConverter()
    {
        var bossInfoParam = Expression.Parameter(typeof(BossInfo), "bossInfo");
        var bossIdParam = Expression.Parameter(typeof(string), "bossId");

        var property = Expression.Property(bossInfoParam, typeof(BossInfo).GetProperty(nameof(BossInfo.Id))!);

        SetId = Expression.Lambda<Action<BossInfo, string>>(Expression.Assign(property, bossIdParam), bossInfoParam, bossIdParam)
                          .Compile();
    }

    public override EventAndBossData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (JsonNode.Parse(ref reader) is not JsonObject obj)
            return null;

        var value = obj.Deserialize<EventAndBossData>(RecursionSafeOptions.Without(options, typeof(EventAndBossDataConverter)))
                    ?? new EventAndBossData();

        var bossInfoDic = (Dictionary<string, BossInfo>)value.BossInfo;

        foreach ((var key, var child) in obj)
            if (child?.GetValueKind() == JsonValueKind.Object)
            {
                var bossInfo = child.Deserialize<BossInfo>(options) ?? throw new JsonException("Failed to deserialize boss info.");

                SetId(bossInfo, key);
                bossInfoDic[key] = bossInfo;
            }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, EventAndBossData value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}