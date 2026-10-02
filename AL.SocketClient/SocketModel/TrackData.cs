namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents one entry in a ranger <c>track</c> skill result, an array sorted ascending by <see cref="Dist" />
///     covering players within the skill's range.
/// </summary>
public sealed record TrackData
{
    /// <summary>Distance from the caster to the tracked player.</summary>
    public double Dist { get; init; }

    /// <summary>
    ///     Whether the tracked player is invisible.
    /// </summary>
    public bool Invis { get; init; }

    /// <summary>
    ///     The audio cue for the tracked player's class group: "wmp" (default), "rr" (rogue/ranger), "pm" (priest/mage).
    /// </summary>
    public string Sound { get; init; } = null!;
}