#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Events;

/// <summary>
///     Represents a monster and how many of it: one entry of a camp's pack, or the fight an encounter reply starts. Positional
///     on the wire, as <c>["cave_rat", 6]</c>.
/// </summary>
[JsonConverter(typeof(GMonsterCountConverter))]
public sealed record GMonsterCount
{
    public int Count { get; init; } = 1;

    public string Monster { get; init; } = null!;
}

/// <summary>
///     Reads the positional pair. Anything that is not an array of a string then a whole number reads as null.
/// </summary>
public sealed class GMonsterCountConverter : JsonConverter<GMonsterCount>
{
    /// <inheritdoc />
    public override GMonsterCount? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                .TryGetInt32(out var count))
            return null;

        return new GMonsterCount
        {
            Monster = slots[0]
                          .GetString()
                      ?? string.Empty,
            Count = count
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, GMonsterCount value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.Monster);
        writer.WriteNumberValue(value.Count);
        writer.WriteEndArray();
    }
}