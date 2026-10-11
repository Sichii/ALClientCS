#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Events;

/// <summary>
///     Represents one item a cave merchant can offer and its price in the party's cave gold. Positional on the wire, as
///     <c>["cave_loaded_die", 10000]</c>.
/// </summary>
[JsonConverter(typeof(GStockItemConverter))]
public sealed record GStockItem
{
    public string Item { get; init; } = null!;

    public long Price { get; init; }
}

/// <summary>
///     Reads the positional pair. Anything that is not an array of a string then a whole number reads as null.
/// </summary>
public sealed class GStockItemConverter : JsonConverter<GStockItem>
{
    /// <inheritdoc />
    public override GStockItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            //consume whatever this is so the reader stays aligned for the next entry
            reader.Skip();

            return null;
        }

        using var document = JsonDocument.ParseValue(ref reader);

        var slots = document.RootElement
                            .EnumerateArray()
                            .ToArray();

        if ((slots.Length < 2)
            || (slots[0].ValueKind != JsonValueKind.String)
            || (slots[1].ValueKind != JsonValueKind.Number)
            || !slots[1]
                .TryGetInt64(out var price))
            return null;

        return new GStockItem
        {
            Item = slots[0]
                       .GetString()
                   ?? string.Empty,
            Price = price
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, GStockItem value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.Item);
        writer.WriteNumberValue(value.Price);
        writer.WriteEndArray();
    }
}