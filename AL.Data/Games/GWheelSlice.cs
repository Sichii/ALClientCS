#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Games;

/// <summary>
///     Represents one wedge of the tavern's wheel, positional in <c>games.wheel.slices</c>.
/// </summary>
/// <param name="Name">
///     The wedge's name, a colour such as <c>indigo</c>, distinct for every wedge.
/// </param>
/// <param name="Side">
///     Which of <see cref="GWheel.Sides" /> this wedge pays.
/// </param>
/// <param name="Colour">The wedge's fill, as a CSS hex colour.</param>
public sealed record GWheelSlice(
    [property: JsonArrayIndex(0)]
    string Name,
    [property: JsonArrayIndex(1)]
    string Side,
    [property: JsonArrayIndex(2)]
    string Colour);