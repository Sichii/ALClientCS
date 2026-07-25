namespace AL.SocketClient.SocketModel;

/// <summary>
///     One entry in a ranger 'track' skill result (node/server.js:9499). The result is an array of these,
///     sorted ascending by <see cref="Dist" />, covering players within the skill's range.
/// </summary>
public sealed record TrackData
{
    /// <summary>
    ///     The audio cue for the tracked player's class group: "wmp" (default), "rr" (rogue/ranger),
    ///     "pm" (priest/mage).
    /// </summary>
    public string Sound { get; init; } = null!;

    /// <summary>
    ///     Distance from the caster to the tracked player.
    /// </summary>
    public double Dist { get; init; }

    /// <summary>
    ///     True when the tracked player is invisible. Absent (false) otherwise.
    /// </summary>
    public bool Invis { get; init; }
}
