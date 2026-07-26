#nullable disable

#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Games;

public sealed class GamesDatum
{
    [JsonPropertyName("dice")]
    public object Dice { get; init; } = null!;

    [JsonPropertyName("slots")]
    public object Slots { get; init; } = null!;

    [JsonPropertyName("tarot")]
    public object Tarot { get; init; } = null!;

    [JsonPropertyName("wheel")]
    public object Wheel { get; init; } = null!;
}