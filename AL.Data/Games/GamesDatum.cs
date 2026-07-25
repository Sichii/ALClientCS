#nullable disable

#region
using System.Text.Json.Serialization;
using Newtonsoft.Json;
#endregion

namespace AL.Data.Games;

public sealed class GamesDatum
{
    [JsonProperty("dice")]
    [JsonPropertyName("dice")]
    public object Dice { get; init; } = null!;

    [JsonProperty("slots")]
    [JsonPropertyName("slots")]
    public object Slots { get; init; } = null!;

    [JsonProperty("tarot")]
    [JsonPropertyName("tarot")]
    public object Tarot { get; init; } = null!;

    [JsonProperty("wheel")]
    [JsonPropertyName("wheel")]
    public object Wheel { get; init; } = null!;
}