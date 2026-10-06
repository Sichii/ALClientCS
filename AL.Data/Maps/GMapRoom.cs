namespace AL.Data.Maps;

/// <summary>
///     Represents one room of a generated floor's layout, as its definition lists it.
/// </summary>
/// <remarks>
///     Every room of the layout is listed, whatever the run put in it; the run's own room ids (<c>1:3</c>) are picked from
///     this list by the server and are not on it.
/// </remarks>
public sealed record GMapRoom
{
    /// <summary>The room's left, top, right and bottom edges.</summary>
    public IReadOnlyList<float> Bounds { get; init; } = [];

    /// <summary>The room's index in the layout.</summary>
    public int Id { get; init; }

    /// <summary>
    ///     The layout's kind of room, such as <c>cavern</c>.
    /// </summary>
    public string? Kind { get; init; }

    /// <summary>The x coordinate of the room's centre.</summary>
    public float X { get; init; }

    /// <summary>The y coordinate of the room's centre.</summary>
    public float Y { get; init; }
}