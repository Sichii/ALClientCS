#nullable disable

#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Games;

public sealed class GamesDatum
{
    [JsonPropertyName("dice")]
    public object Dice { get; init; } = null!;

    [JsonPropertyName("poker")]
    public GPoker Poker { get; init; } = null!;

    [JsonPropertyName("slots")]
    public GSlots Slots { get; init; } = null!;

    [JsonPropertyName("tarot")]
    public object Tarot { get; init; } = null!;

    [JsonPropertyName("wheel")]
    public GWheel Wheel { get; init; } = null!;
}