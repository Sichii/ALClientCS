#region
using AL.Core.Definitions;
using AL.Core.Geometry;
#endregion

namespace AL.Data.Maps;

/// <summary>
///     Represents the static data for a trap on a map.
/// </summary>
public sealed record GTrap
{
    /// <summary>
    ///     <b>
    ///         NULLABLE
    ///     </b>
    ///     . The area a debuff trap covers. Standing anywhere inside it re-applies slowness every instance tick
    ///     (node/server.js:13632).
    ///     <br />
    /// </summary>
    public Polygon? Polygon { get; init; }

    /// <summary>
    ///     Where a spikes trap sits. Standing on it costs 50 hp per instance tick (node/server.js:13623).
    /// </summary>
    public Point Position { get; init; }

    /// <summary>
    ///     The type of trap. Spikes use <see cref="Position" />, debuff traps use <see cref="Polygon" />.
    /// </summary>
    public TrapType Type { get; init; }
}