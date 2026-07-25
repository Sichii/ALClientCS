namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents a broadcast boss/holiday spawn event (node/server_functions.js:1698-1815). Distinct from the
///     periodic <c>server_info</c> snapshot (<see cref="EventAndBossData" />).
/// </summary>
public sealed record GameEventData
{
    /// <summary>
    ///     The event/boss type: pinkgoo, crabxx, snowman, dragold, franky, icegolem, mrpumpkin, grinch,
    ///     slenderman, tiger, wabbit, ...
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     The map the event spawned on.
    /// </summary>
    public string Map { get; init; } = null!;

    /// <summary>
    ///     The spawn x-coordinate, when the event carries one (absent for map-wide spawns like pinkgoo).
    /// </summary>
    public float? X { get; init; }

    /// <summary>
    ///     The spawn y-coordinate, when the event carries one.
    /// </summary>
    public float? Y { get; init; }
}
