#region
using AL.Core.Definitions;
using AL.Data.NPCs;
#endregion

namespace AL.Data;

/// <summary>
///     Represents a recipe that can be crafted or dismantled.
/// </summary>
public sealed record Recipe
{
    /// <summary>
    ///     The cost of the recipe in gold.
    /// </summary>
    public long Cost { get; init; }

    /// <summary>
    ///     The name, quantity, and level of the items associated with the recipe.
    /// </summary>
    public IReadOnlyList<(float Quantity, string ItemName, int Level)> Items { get; init; }
        = new List<(float Quantity, string ItemName, int Level)>();

    /// <summary>
    ///     The NPC this item is crafted or dismantled at.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public GNPC NPC { get; internal set; } = null!;

    /// <summary>
    ///     If populated, this is the tag of the NPC this recipe is related to.
    ///     <br />
    ///     Otherwise this recipe is crafted at the craftsman.
    /// </summary>
    public Quest? Quest { get; init; }
}