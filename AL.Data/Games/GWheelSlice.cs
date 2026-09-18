#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Games;

/// <summary>
///     One wedge of the tavern's wheel. Rides <c>games.wheel.slices</c>, whose entries are positional arrays.
/// </summary>
/// <param name="Name">
///     The wedge's own name, which is a colour rather than a prize - <c>indigo</c>, <c>pink</c>, <c>teal</c>. Every one of
///     the fourteen is distinct, so it identifies the wedge the wheel stopped on.
/// </param>
/// <param name="Side">
///     Which of <see cref="GWheel.Sides" /> this wedge pays. The fourteen wedges split evenly, seven to each side.
/// </param>
/// <param name="Colour">
///     What the client fills the wedge with, as a CSS hex colour. Display only.
/// </param>
public sealed record GWheelSlice(
    [property: JsonArrayIndex(0)]
    string Name,
    [property: JsonArrayIndex(1)]
    string Side,
    [property: JsonArrayIndex(2)]
    string Colour);
