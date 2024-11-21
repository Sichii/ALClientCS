#nullable disable

#region
using Newtonsoft.Json;
#endregion

namespace AL.Data.Games;

public sealed class GamesDatum
{
    [JsonProperty("dice")]
    public object Dice { get; init; } = null!;

    [JsonProperty("slots")]
    public object Slots { get; init; } = null!;

    [JsonProperty("tarot")]
    public object Tarot { get; init; } = null!;

    [JsonProperty("wheel")]
    public object Wheel { get; init; } = null!;
}