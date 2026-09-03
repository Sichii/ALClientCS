#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Classes;

/// <summary>
///     One of the ready-made looks the character creation screen offers for a class.
/// </summary>
/// <param name="Name">
///     The look's own sprite, which is what the character is dressed in.
/// </param>
/// <param name="Pieces">
///     What the look puts in each cosmetic slot, keyed by the slot's name.
/// </param>
/// <remarks>
///     Positional on the wire -
///     <c>
///         [name, { slot: piece }]
///     </c>
///     - so it is read by index rather than by key.
/// </remarks>
public sealed record GClassLook(
    [property: JsonArrayIndex(0)]
    string Name,
    [property: JsonArrayIndex(1)]
    IReadOnlyDictionary<string, string> Pieces);
