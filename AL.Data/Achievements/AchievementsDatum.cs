#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Achievements;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class AchievementsDatum : DatumBase<GAchievement>
{
    [JsonPropertyName("1000boss")]
    public GAchievement _1000Boss { get; init; } = null!;

    [JsonPropertyName("100boss")]
    public GAchievement _100Boss { get; init; } = null!;

    [JsonPropertyName("abtesting")]
    public GAchievement Abtesting { get; init; } = null!;

    [JsonPropertyName("discoverlair")]
    public GAchievement Discoverlair { get; init; } = null!;

    [JsonPropertyName("festive")]
    public GAchievement Festive { get; init; } = null!;

    [JsonPropertyName("firehazard")]
    public GAchievement Firehazard { get; init; } = null!;

    [JsonPropertyName("gooped")]
    public GAchievement Gooped { get; init; } = null!;

    [JsonPropertyName("lucky")]
    public GAchievement Lucky { get; init; } = null!;

    [JsonPropertyName("monsterhunter")]
    public GAchievement Monsterhunter { get; init; } = null!;

    /// <summary>
    ///     Defeat 1,000 Bosses.
    /// </summary>
    [JsonPropertyName("reach40")]
    public GAchievement Reach40 { get; init; } = null!;

    [JsonPropertyName("reach50")]
    public GAchievement Reach50 { get; init; } = null!;

    [JsonPropertyName("reach60")]
    public GAchievement Reach60 { get; init; } = null!;

    [JsonPropertyName("reach70")]
    public GAchievement Reach70 { get; init; } = null!;

    [JsonPropertyName("reach80")]
    public GAchievement Reach80 { get; init; } = null!;

    [JsonPropertyName("reach90")]
    public GAchievement Reach90 { get; init; } = null!;

    [JsonPropertyName("stomped")]
    public GAchievement Stomped { get; init; } = null!;

    [JsonPropertyName("upgrade10")]
    public GAchievement Upgrade10 { get; init; } = null!;
}