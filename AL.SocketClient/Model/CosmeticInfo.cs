#region
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents the appearance data for a <see cref="Player" />.
/// </summary>
public sealed class CosmeticInfo
{
    [JsonProperty]
    public string Chin { get; init; } = null!;

    [JsonProperty]
    public string Face { get; init; } = null!;

    [JsonProperty]
    public string Hair { get; init; } = null!;

    [JsonProperty]
    public string Hat { get; init; } = null!;

    [JsonProperty]
    public string Head { get; init; } = null!;

    [JsonProperty]
    public string Makeup { get; init; } = null!;

    [JsonProperty]
    public string Upper { get; init; } = null!;
}