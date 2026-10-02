#region
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Json.SystemTextJson;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
#endregion

namespace AL.SocketClient.Json.SystemTextJson;

/// <summary>
///     Deserializes <see cref="EventAndBossData" />: fills the declared fields, then reads every object-valued property as
///     a <see cref="BossInfo" /> keyed by and stamped with its property name.
/// </summary>
/// <remarks>
///     Register it in the shared options, so the declared-field fill can drop it and not re-enter itself.
/// </remarks>
public sealed class EventAndBossDataConverter : JsonConverter<EventAndBossData>
{
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

                //the property name is the only place the id exists on the wire
                bossInfoDic[key] = bossInfo with
                {
                    Id = key
                };
            }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, EventAndBossData value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}