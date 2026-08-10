#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Monsters;

/// <summary>
///     Represents a condition that a monster has when it spawns.
///     <br />
///     No information aside from duration is generally given.
/// </summary>
public record GInitialCondition
{
    /// <summary>
    ///     How long the monster spawns holding this condition, in milliseconds.
    /// </summary>
    [JsonPropertyName("ms")]
    public float DurationMS { get; init; }
}