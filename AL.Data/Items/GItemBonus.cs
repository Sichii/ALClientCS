#region
using System.Text.Json.Serialization;
using AL.Core.Abstractions;
using AL.Core.Definitions;
#endregion

namespace AL.Data.Items;

/// <summary>
///     Represents the stats an item adds on top of its own for one class, or on one map - a rogue wearing a tiger helmet,
///     a cove mantle in the cave.
///     <br />
///     <inheritdoc cref="AttributedRecordBase" />
/// </summary>
/// <seealso cref="GItem.Bonuses" />
public sealed record GItemBonus : AttributedRecordBase
{
    /// <summary>
    ///     If populated, what each level of the item's compound track gains in this situation, over and above the item's own.
    /// </summary>
    [JsonPropertyName("compound")]
    public IReadOnlyDictionary<ALAttribute, float>? CompoundModifiers { get; init; }

    /// <summary>
    ///     If populated, what each level of the item's upgrade track gains in this situation, over and above the item's own.
    /// </summary>
    [JsonPropertyName("upgrade")]
    public IReadOnlyDictionary<ALAttribute, float>? UpgradeModifiers { get; init; }
}