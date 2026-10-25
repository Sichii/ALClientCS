#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Events;

/// <summary>
///     Represents a Witch's Cipher ingredient and the monsters that drop it while the round's gathering lasts. Positional
///     in <see cref="GEvent.Ingredients" />.
/// </summary>
/// <param name="Name">The ingredient's item name.</param>
/// <param name="Monsters">The monsters that drop it.</param>
public sealed record GWitchIngredient(
    [property: JsonArrayIndex(0)]
    string Name,
    [property: JsonArrayIndex(1)]
    IReadOnlyList<string> Monsters);