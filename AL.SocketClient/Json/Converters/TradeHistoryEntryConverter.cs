#region
using System;
using AL.APIClient.Model;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace AL.SocketClient.Json.Converters;

/// <summary>
///     Binds the server's positional <c>[event, name, item, price]</c> trade-history tuple to a
///     <see cref="TradeHistoryEntry" />. The 4th element is <c>null</c> for giveaways.
/// </summary>
public sealed class TradeHistoryEntryConverter : JsonConverter<TradeHistoryEntry>
{
    public override TradeHistoryEntry ReadJson(
        JsonReader reader,
        Type objectType,
        TradeHistoryEntry? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var array = JArray.Load(reader);

        return new TradeHistoryEntry
        {
            Event = array[0].Value<string>()!,
            PartnerName = array[1].Value<string>()!,
            Item = array[2].ToObject<TradeItem>(serializer)!,
            Price = array[3].Value<long?>()
        };
    }

    public override void WriteJson(JsonWriter writer, TradeHistoryEntry? value, JsonSerializer serializer)
        => throw new NotImplementedException();
}
