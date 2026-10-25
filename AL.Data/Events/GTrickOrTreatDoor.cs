#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Events;

/// <summary>
///     Represents a Trick or Treat door. Positional in <see cref="GEvent.Doors" />.
/// </summary>
/// <param name="Map">The map the door is on.</param>
/// <param name="X">The door's x.</param>
/// <param name="Y">The door's y.</param>
public sealed record GTrickOrTreatDoor(
    [property: JsonArrayIndex(0)]
    string Map,
    [property: JsonArrayIndex(1)]
    float X,
    [property: JsonArrayIndex(2)]
    float Y);