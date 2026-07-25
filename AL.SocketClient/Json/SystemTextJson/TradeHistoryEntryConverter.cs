#region
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.APIClient.Model;
using AL.SocketClient.SocketModel;
#endregion

namespace AL.SocketClient.Json.SystemTextJson;

/// <summary>
///     Binds the server's positional <c>[event, name, item, price]</c> trade-history tuple to a
///     <see cref="TradeHistoryEntry" />. The 4th element is <c>null</c> for giveaways. The System.Text.Json
///     replacement for the Newtonsoft <c>TradeHistoryEntryConverter</c>.
/// </summary>
public sealed class TradeHistoryEntryConverter : JsonConverter<TradeHistoryEntry>
{
    public override TradeHistoryEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var array = JsonNode.Parse(ref reader)?.AsArray()
                    ?? throw new InvalidOperationException("Failed to deserialize trade-history entry.");

        return new TradeHistoryEntry
        {
            Event = array[0]!.GetValue<string>(),
            PartnerName = array[1]!.GetValue<string>(),
            Item = array[2].Deserialize<TradeItem>(options)!,

            //giveaways send a JSON null price (null node -> null). Deserialize (not GetValue<long>) so a
            //stringified price coerces via NumberHandling, matching Newtonsoft's lenient Value<long?>()
            Price = array[3] is { } price ? price.Deserialize<long?>(options) : null
        };
    }

    public override void Write(Utf8JsonWriter writer, TradeHistoryEntry value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}
