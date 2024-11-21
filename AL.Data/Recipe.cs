#region
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using AL.Data.NPCs;
using Newtonsoft.Json;
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
    [JsonProperty(ItemConverterType = typeof(ArrayToTupleConverter<int, string, int>))]
    public IReadOnlyList<(int Quantity, string ItemName, int Level)> Items { get; init; }
        = new List<(int Quantity, string ItemName, int Level)>();

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