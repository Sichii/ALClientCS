#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Titles;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class TitlesDatum : DatumBase<GTitle>
{
    [JsonPropertyName("abtesting")]
    public GTitle Abtesting { get; init; } = null!;

    [JsonPropertyName("critmonger")]
    public GTitle Critmonger { get; init; } = null!;

    [JsonPropertyName("fast")]
    public GTitle Fast { get; init; } = null!;

    [JsonPropertyName("festive")]
    public GTitle Festive { get; init; } = null!;

    [JsonPropertyName("firehazard")]
    public GTitle Firehazard { get; init; } = null!;

    [JsonPropertyName("glitched")]
    public GTitle Glitched { get; init; } = null!;

    [JsonPropertyName("gooped")]
    public GTitle Gooped { get; init; } = null!;

    [JsonPropertyName("legacy")]
    public GTitle Legacy { get; init; } = null!;

    [JsonPropertyName("lucky")]
    public GTitle Lucky { get; init; } = null!;

    [JsonPropertyName("shiny")]
    public GTitle Shiny { get; init; } = null!;

    [JsonPropertyName("sniper")]
    public GTitle Sniper { get; init; } = null!;

    [JsonPropertyName("stomped")]
    public GTitle Stomped { get; init; } = null!;

    [JsonPropertyName("superfast")]
    public GTitle Superfast { get; init; } = null!;
}