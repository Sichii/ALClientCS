using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

#nullable enable

namespace AL.Data
{
    /// <summary>
    ///     Represents a recipe that can be crafted or dismantled.
    /// </summary>
    public record Recipe
    {
        /// <summary>
        ///     The cost of the recipe in gold.
        /// </summary>
        public int Cost { get; init; }

        /// <summary>
        ///     The name, quantity, and level of the items associated with the recipe.
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(ArrayToTupleConverter<float, string, int>))]
        public IReadOnlyList<(float Quantity, string ItemName, int Level)> Items { get; init; } =
            new List<(float Quantity, string ItemName, int Level)>();

        /// <summary>
        ///     The tag of the NPC this recipe is related to.
        /// </summary>
        public Quest Quest { get; init; }
    }
}