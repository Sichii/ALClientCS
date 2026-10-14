#region
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Monsters;

/// <summary>
///     Represents one entry of a monster's <see cref="GMonster.Spawns" />: a monster it summons next to its target.
///     Positional on the wire, as <c>[30000, "bee"]</c> for a timed summon or <c>["hp:0.75", "jr", 5]</c> for one
///     triggered by health.
/// </summary>
[JsonConverter(typeof(GMonsterSpawnConverter))]
public sealed record GMonsterSpawn
{
    /// <summary>How many it summons each time the entry fires.</summary>
    public int Count { get; init; } = 1;

    /// <summary>
    ///     If populated, the summon fires once the summoner's health falls to this fraction of its maximum.
    /// </summary>
    public float? HpThreshold { get; init; }

    /// <summary>If populated, the milliseconds between summons.</summary>
    public float? Interval { get; init; }

    /// <summary>The monster summoned.</summary>
    public string Monster { get; init; } = null!;
}

/// <summary>
///     Reads the positional entry. Anything that is not an array of a trigger, a monster name and an optional count reads
///     as null.
/// </summary>
public sealed class GMonsterSpawnConverter : JsonConverter<GMonsterSpawn>
{
    /// <inheritdoc />
    public override GMonsterSpawn? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        const string HP_PREFIX = "hp:";

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

        if ((slots.Length < 2) || (slots[1].ValueKind != JsonValueKind.String))
            return null;

        var count = 1;

        if ((slots.Length > 2)
            && ((slots[2].ValueKind != JsonValueKind.Number)
                || !slots[2]
                    .TryGetInt32(out count)))
            return null;

        var spawn = new GMonsterSpawn
        {
            Monster = slots[1]
                          .GetString()
                      ?? string.Empty,
            Count = count
        };

        if (slots[0].ValueKind == JsonValueKind.Number)
            return spawn with
            {
                Interval = slots[0]
                    .GetSingle()
            };

        if ((slots[0].ValueKind == JsonValueKind.String)
            && slots[0]
                .GetString() is { } trigger
            && trigger.StartsWith(HP_PREFIX, StringComparison.Ordinal)
            && float.TryParse(
                trigger[HP_PREFIX.Length..],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var threshold))
            return spawn with
            {
                HpThreshold = threshold
            };

        return null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, GMonsterSpawn value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        if (value.HpThreshold is { } threshold)
            writer.WriteStringValue($"hp:{threshold.ToString(CultureInfo.InvariantCulture)}");
        else
            writer.WriteNumberValue(value.Interval ?? 0);

        writer.WriteStringValue(value.Monster);
        writer.WriteNumberValue(value.Count);
        writer.WriteEndArray();
    }
}