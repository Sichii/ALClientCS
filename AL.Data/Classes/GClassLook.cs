#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Classes;

/// <summary>
///     Represents one of the ready-made looks the character creation screen offers for a class.
/// </summary>
/// <param name="Name">
///     The look's own sprite, which is what the character is dressed in.
/// </param>
/// <param name="Pieces">
///     What the look puts in each cosmetic slot, keyed by the slot's name.
/// </param>
/// <remarks>
///     Positional on the wire: <c>[name, { slot: piece }]</c>.
/// </remarks>
public sealed record GClassLook(
    [property: JsonArrayIndex(0)]
    string Name,
    [property: JsonArrayIndex(1)]
    IReadOnlyDictionary<string, string> Pieces);