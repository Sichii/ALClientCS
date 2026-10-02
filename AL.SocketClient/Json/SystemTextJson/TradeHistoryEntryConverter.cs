#region
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.APIClient.Model;
using AL.SocketClient.SocketModel;
#endregion

namespace AL.SocketClient.Json.SystemTextJson;

/// <summary>
///     Binds the server's positional <c>[event, name, item, price]</c> trade-history tuple to a
///     <see cref="TradeHistoryEntry" />. The price is <c>null</c> for giveaways.
/// </summary>
public sealed class TradeHistoryEntryConverter : JsonConverter<TradeHistoryEntry>
{
    private static bool IsPrice(JsonNode node)
        => node.GetValueKind() switch
        {
            JsonValueKind.Number => true,
            JsonValueKind.String => long.TryParse(node.GetValue<string>(), out _),
            _                    => false
        };

    public override TradeHistoryEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var array = JsonNode.Parse(ref reader)
                            ?.AsArray()
                    ?? throw new InvalidOperationException("Failed to deserialize trade-history entry.");

        return new TradeHistoryEntry
        {
            Event = array[0]!.GetValue<string>(),
            PartnerName = array[1]!.GetValue<string>(),
            Item = array[2]
                .Deserialize<TradeItem>(options)!,

            //a stringified price coerces through NumberHandling; any other shape, such as a swap's, reads as no price
            Price = (array.Count > 3) && array[3] is { } price && IsPrice(price) ? price.Deserialize<long?>(options) : null,
            Received = (array.Count > 4) && array[4] is JsonObject received ? received.Deserialize<TradeItem>(options) : null
        };
    }

    public override void Write(Utf8JsonWriter writer, TradeHistoryEntry value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}