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
    ///     The gold charged to run this recipe, on top of the items it consumes.
    /// </summary>
    public long Cost { get; init; }

    /// <summary>
    ///     The items on the other side of the recipe: what crafting consumes, or what dismantling hands back. The level is an
    ///     exact requirement when crafting, and is zero unless the recipe names one.
    /// </summary>
    public IReadOnlyList<(float Quantity, string ItemName, int Level)> Items { get; init; }
        = new List<(float Quantity, string ItemName, int Level)>();

    /// <summary>
    ///     The NPC a craft recipe is run at - its quest NPC when it has a quest tag, the craftsman otherwise. Null on a
    ///     dismantle recipe, which the server still requires the craftsman for.
    /// </summary>
    /// <remarks>Enriched property</remarks>
    public GNPC NPC { get; internal set; } = null!;

    /// <summary>
    ///     If populated, the item this recipe really produces, which is not the one it is filed under.
    /// </summary>
    /// <remarks>
    ///     A recipe without this makes the item it is named for, which is nearly all of them. The server's craft reply names
    ///     whatever came out, so anything matching that reply against a recipe name has to come through here first.
    /// </remarks>
    public RecipeOutput? Output { get; init; }

    /// <summary>
    ///     If populated, the quest tag of the NPC this recipe must be run beside.
    ///     <br />
    ///     Otherwise this recipe is crafted at the craftsman. No dismantle recipe carries one.
    /// </summary>
    public Quest? Quest { get; init; }
}