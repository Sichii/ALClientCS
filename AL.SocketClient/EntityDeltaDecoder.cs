#region
using System.Text.Json.Nodes;
#endregion

namespace AL.SocketClient;

/// <summary>
///     Rebuilds the full entity packets the server sends as deltas to a socket that connected with <c>entity_delta=1</c>.
/// </summary>
/// <remarks>
///     A delta carries the entity's <c>id</c>, its changed fields, and the names of its removed fields under <c>~</c>. It
///     applies to the last full packet held for that id, so every frame must pass through here in arrival order.
/// </remarks>
internal sealed class EntityDeltaDecoder
{
    private const string REMOVED_KEY = "~";

    /// <summary>
    ///     The last full packet received for each entity id, the base the next delta applies to.
    /// </summary>
    private readonly Dictionary<string, JsonObject> Packets = new(StringComparer.Ordinal);

    /// <summary>
    ///     Rewrites every delta in a frame into the full packet it stands for, in place.
    /// </summary>
    /// <param name="eventName">The frame's event name.</param>
    /// <param name="data">The frame's payload.</param>
    /// <returns>
    ///     <c>false</c> if a delta arrived without its base and was dropped from the frame; otherwise, <c>true</c> .
    /// </returns>
    internal bool Decode(string eventName, JsonNode? data)
    {
        switch (eventName)
        {
            case "entities":
                return DecodeEntities(data as JsonObject);
            case "new_map" or "start":
                return DecodeEntities(data?["entities"] as JsonObject);
            case "disappear" or "death":
                if (data?["id"] is JsonValue idValue && idValue.TryGetValue(out string? id))
                    Packets.Remove(id);

                return true;
            default:
                return true;
        }
    }

    private bool DecodeEntities(JsonObject? entities)
    {
        if (entities is null)
            return true;

        if (entities["type"] is JsonValue typeValue && typeValue.TryGetValue(out string? type) && (type == "all"))
            Packets.Clear();

        var complete = DecodeList(entities["players"] as JsonArray);

        return DecodeList(entities["monsters"] as JsonArray) && complete;
    }

    private bool DecodeList(JsonArray? list)
    {
        if (list is null)
            return true;

        var complete = true;

        for (var i = 0; i < list.Count; i++)
        {
            if (list[i] is not JsonObject entry || entry["id"] is not JsonValue idValue || !idValue.TryGetValue(out string? id))
                continue;

            if (entry[REMOVED_KEY] is JsonArray removed)
            {
                if (!Packets.TryGetValue(id, out var held))
                {
                    list.RemoveAt(i--);
                    complete = false;

                    continue;
                }

                var full = held.DeepClone()
                               .AsObject();

                foreach ((var key, var value) in entry)
                    if (key != REMOVED_KEY)
                        full[key] = value?.DeepClone();

                foreach (var name in removed)
                    if (name is JsonValue nameValue && nameValue.TryGetValue(out string? removedKey))
                        full.Remove(removedKey);

                list[i] = entry = full;
            }

            //held by reference; nothing writes to a frame's nodes once it is decoded
            Packets[id] = entry;
        }

        return complete;
    }
}