#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     The combined multiplier each active encouragement bonus contributes, one per reward the bonuses touch.
/// </summary>
/// <remarks>
///     These multiply on top of the character's own <c>goldm</c> , <c>xpm</c> and <c>luckm</c> rather than folding into
///     them, so a reward estimate built on those three alone is short by whatever is here. 1 means no bonus.
/// </remarks>
public sealed record EncouragementTotals
{
    /// <summary>The multiplier applied to gold.</summary>
    [JsonPropertyName("gold")]
    public float Gold { get; init; } = 1f;

    /// <summary>
    ///     The multiplier applied to luck, and so to every drop roll.
    /// </summary>
    [JsonPropertyName("luck")]
    public float Luck { get; init; } = 1f;

    /// <summary>The multiplier applied to experience.</summary>
    [JsonPropertyName("xp")]
    public float Xp { get; init; } = 1f;
}