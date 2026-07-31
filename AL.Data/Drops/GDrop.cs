#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Drops;

/// <summary>
///     One roll on a drop table.
/// </summary>
[JsonConverter(typeof(GDropConverter))]
public sealed record GDrop
{
    /// <summary>
    ///     Whether <see cref="Name" /> names a further drop table to roll rather than an item to hand over.
    /// </summary>
    public bool IsChest { get; init; }

    /// <summary>
    ///     The item dropped, or - when <see cref="IsChest" /> - the table opened in its place.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     How many of <see cref="Name" /> a successful roll hands over.
    /// </summary>
    public int Quantity { get; init; } = 1;

    /// <summary>
    ///     The per-kill chance, for a solo kill of a level-1 monster with no luck bonus. The server rolls
    ///     <c>
    ///         random() / (share * luckm * level * mult) &lt; rate
    ///     </c>
    ///     (node/server.js:2189), so a rate of 1 or more is a guaranteed drop rather than a probability - several
    ///     tables express that as 100 or 10000.
    /// </summary>
    public float Rate { get; init; }
}

/// <summary>
///     Reads the positional wire form of a <see cref="GDrop" />, whose third slot means two different things:
///     <c>
///         [rate, item]
///     </c>
///     ,
///     <c>
///         [rate, item, quantity]
///     </c>
///     ,
///     <c>
///         [rate, "open", tableName]
///     </c>
///     . <see cref="AL.Core.Json.SystemTextJson.ArrayToObjectConverter{T}" /> cannot express that - it binds one
///     declared type per index.
/// </summary>
public sealed class GDropConverter : JsonConverter<GDrop>
{
    /// <inheritdoc />
    public override GDrop? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            //consume whatever this is so the reader stays aligned for the next entry
            reader.Skip();

            return null;
        }

        using var document = JsonDocument.ParseValue(ref reader);

        var slots = document.RootElement.EnumerateArray()
                            .ToArray();

        //a rate that is not a number, or a second slot that is not a name, is not an entry this can mean anything
        //by - and GetSingle/GetString throw rather than degrade on the wrong token
        if ((slots.Length < 2) || (slots[0].ValueKind != JsonValueKind.Number) || (slots[1].ValueKind != JsonValueKind.String))
            return null;

        var rate = slots[0]
            .GetSingle();

        var second = slots[1]
                     .GetString()
                     ?? string.Empty;

        //"open" is the marker, and the table name follows it in the slot a quantity would otherwise occupy
        if (second.Equals("open", StringComparison.OrdinalIgnoreCase))
            return new GDrop
            {
                Rate = rate,
                Name = (slots.Length > 2) && (slots[2].ValueKind == JsonValueKind.String)
                    ? slots[2]
                        .GetString()
                      ?? string.Empty
                    : string.Empty,
                Quantity = 1,
                IsChest = true
            };

        //a fourth slot is a cosmetic skin for the item in the third, and is not modelled
        return new GDrop
        {
            Rate = rate,
            Name = second,
            Quantity = (slots.Length > 2) && (slots[2].ValueKind == JsonValueKind.Number)
                ? slots[2]
                    .GetInt32()
                : 1,
            IsChest = false
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, GDrop value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Rate);

        if (value.IsChest)
        {
            writer.WriteStringValue("open");
            writer.WriteStringValue(value.Name);
        } else
        {
            writer.WriteStringValue(value.Name);

            if (value.Quantity != 1)
                writer.WriteNumberValue(value.Quantity);
        }

        writer.WriteEndArray();
    }
}
