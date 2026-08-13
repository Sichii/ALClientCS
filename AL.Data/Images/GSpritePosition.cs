#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Images;

/// <summary>
///     Where an item's skin sits: a cell in the named <see cref="GImageSet" />.
/// </summary>
/// <remarks>
///     Look one of these up by an item's <c>Skin</c> and nothing else. The table it comes from is a general index of
///     named art, and a dozen of its entries are texture rectangles or lists of other names - those parse to whatever
///     their first three elements happen to say, so a lookup by an arbitrary string can come back with a cell that
///     means nothing.
/// </remarks>
[JsonConverter(typeof(GSpritePositionConverter))]
public sealed record GSpritePosition
{
    /// <summary>Which column of the sheet, or -1 when this entry names no cell.</summary>
    public int Column { get; init; } = -1;

    /// <summary>The sheet's key, or empty for the default one. Empty is the common case.</summary>
    public string ImageSet { get; init; } = string.Empty;

    /// <summary>Which row of the sheet, or -1 when this entry names no cell.</summary>
    public int Row { get; init; } = -1;

    /// <summary>Whether this entry names a cell at all.</summary>
    [JsonIgnore]
    public bool IsCell => (Column >= 0) && (Row >= 0);
}

/// <summary>
///     Reads a <c>[sheet, column, row]</c> triple. Anything that is not that shape reads back as a position naming no
///     cell, rather than throwing and taking the whole payload down with it - the table holds entries of several
///     shapes and only this one is an icon.
/// </summary>
internal sealed class GSpritePositionConverter : JsonConverter<GSpritePosition>
{
    public override GSpritePosition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            //consume whatever it is so the reader stays aligned for the next entry
            reader.Skip();

            return new GSpritePosition();
        }

        var imageSet = string.Empty;
        var column = -1;
        var row = -1;
        var index = 0;

        while (reader.Read() && (reader.TokenType != JsonTokenType.EndArray))
        {
            switch (index)
            {
                case 0 when reader.TokenType == JsonTokenType.String:
                    imageSet = reader.GetString() ?? string.Empty;

                    break;
                case 1 when reader.TokenType == JsonTokenType.Number:
                    column = reader.GetInt32();

                    break;
                case 2 when reader.TokenType == JsonTokenType.Number:
                    row = reader.GetInt32();

                    break;
                default:
                    reader.Skip();

                    break;
            }

            index++;
        }

        return new GSpritePosition
        {
            ImageSet = imageSet,
            Column = column,
            Row = row
        };
    }

    public override void Write(Utf8JsonWriter writer, GSpritePosition value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.ImageSet);
        writer.WriteNumberValue(value.Column);
        writer.WriteNumberValue(value.Row);
        writer.WriteEndArray();
    }
}
