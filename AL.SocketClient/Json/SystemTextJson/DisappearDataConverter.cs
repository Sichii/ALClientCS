#region
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Geometry;
using AL.Core.Json.SystemTextJson;
using AL.SocketClient.SocketModel;
#endregion

namespace AL.SocketClient.Json.SystemTextJson;

/// <summary>
///     Deserializes <see cref="DisappearData" />, special-casing the
///     <c>
///         s
///     </c>
///     field, which is either a spawn <see cref="Orientation" /> array or a scalar spawn id. The System.Text.Json
///     replacement for the Newtonsoft
///     <c>
///         DisappearDataConverter
///     </c>
///     . Register in the shared options so the declared-field fill drops this converter and cannot re-enter itself.
/// </summary>
public sealed class DisappearDataConverter : JsonConverter<DisappearData>
{
    public override DisappearData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (JsonNode.Parse(ref reader) is not JsonObject obj)
            return default;

        Orientation? spawn = null;
        int? spawnId = null;

        if (obj.TryGetPropertyValue("s", out var spawnNode) && spawnNode is not null)
        {
            if (spawnNode.GetValueKind() == JsonValueKind.Array)
                spawn = ArrayToObjectConverter<Orientation>.FromArray(spawnNode.AsArray(), options);
            else
                spawnId = spawnNode.Deserialize<int?>(options);
        }

        var result = obj.Deserialize<DisappearData>(RecursionSafeOptions.Without(options, typeof(DisappearDataConverter)))
                     ?? new DisappearData();

        result.ToSpawnId = spawnId;
        result.ToOrientation = spawn;

        return result;
    }

    public override void Write(Utf8JsonWriter writer, DisappearData value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}