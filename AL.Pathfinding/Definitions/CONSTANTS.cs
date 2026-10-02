#region
using AL.Core.Geometry;
using Chaos.Extensions.Common;
#endregion

namespace AL.Pathfinding.Definitions;

public static class CONSTANTS
{
    /// <summary>
    ///     How long the town channel runs. The server caps it at 3000ms whatever it was asked for.
    /// </summary>
    public const float TOWN_CHANNEL_SECONDS = 3f;

    /// <summary>
    ///     The <c>penalty_cd</c> a door, transporter or <c>leave</c> adds on landing.
    /// </summary>
    public const float DOOR_PENALTY_MS = 3200f;

    /// <summary>
    ///     The <c>penalty_cd</c> a blink, magiport or recall adds on landing.
    /// </summary>
    public const float EFFECT_PENALTY_MS = 812f;

    /// <summary>
    ///     The most <c>penalty_cd</c> the server lets pile up.
    /// </summary>
    public const float PENALTY_CAP_MS = 120000f;

    /// <summary>
    ///     The most of the pending <c>penalty_cd</c> one cast adds to its skill's next ready time.
    /// </summary>
    public const float PENALTY_CHARGE_CAP_MS = 10000f;

    /// <summary>
    ///     How long a blink takes from the cast to the landing.
    /// </summary>
    public const float BLINK_LANDING_MS = 200f;

    /// <summary>
    ///     The multiplier the town channel is priced at over the walk it replaces.
    /// </summary>
    /// <remarks>
    ///     Covers what a walk does not cost: an interrupted channel leaves the character where it started, and landing adds
    ///     3200ms of penalty cooldown. Raise it to make the bot town less readily.
    /// </remarks>
    public const float TOWN_RISK_PREMIUM = 1.2f;

    /// <summary>
    ///     The walk speed the town channel is priced at when the character's own speed is unknown or zero.
    /// </summary>
    public const float NOMINAL_WALK_SPEED = 50f;

    /// <summary>
    ///     The <see cref="CalculateTownCost" /> at <see cref="NOMINAL_WALK_SPEED" />.
    /// </summary>
    public static readonly float NOMINAL_TOWN_COST = CalculateTownCost(NOMINAL_WALK_SPEED);

    /// <summary>
    ///     The heuristic value of a transport, door, or leave connection.
    /// </summary>
    public const float TRANSPORT_HEURISTIC = 50f;

    /// <summary>
    ///     What a door into the bank costs instead of <see cref="TRANSPORT_HEURISTIC" />.
    /// </summary>
    /// <remarks>
    ///     The door opens from up to 150 units along main's street, so a cheaper door makes a round trip through the bank
    ///     undercut walking past it.
    /// </remarks>
    public const float BANK_DOOR_COST = 200f;

    /// <summary>
    ///     How far a search looks for standable ground around a point the flood fill never reached. Clears the widest padded
    ///     wall band, with room for a corner where two bands stack.
    /// </summary>
    public const int MAX_UNSTICK_DISTANCE = 24;

    /// <summary>
    ///     How many of the nearest mesh vertices a point outside every triangle tries to reach before settling for the
    ///     nearest one.
    /// </summary>
    public const int NEAREST_VERTEX_CANDIDATES = 64;

    /// <summary>
    ///     The side of a cell in the uniform grid that finds which triangle a point is in.
    /// </summary>
    public const int MESH_GRID_CELL = 64;

    /// <summary>The default values of a player bounding base.</summary>
    public static readonly BoundingBase DEFAULT_BOUNDING_BASE = new(8, 7, 2);

    /// <summary>
    ///     Determines whether the server takes a <c>leave</c> command from a map.
    /// </summary>
    /// <param name="map">
    ///     The map's key.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if <c>leave</c> works on the map; otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    /// <remarks>
    ///     The server also allows solo instances, which only a gm can create.
    /// </remarks>
    public static bool CanLeave(string map) => map.EqualsI("jail") || map.EqualsI("cyberland");

    /// <summary>
    ///     Calculates the cost of a town teleport in walk distance: the ground the character would cover on foot while the
    ///     channel runs, times <see cref="TOWN_RISK_PREMIUM" />.
    /// </summary>
    /// <param name="walkSpeed">
    ///     The character's walk speed; zero or less falls back to <see cref="NOMINAL_WALK_SPEED" />.
    /// </param>
    /// <returns>
    ///     The cost of the town edge.
    /// </returns>
    public static float CalculateTownCost(float walkSpeed)
        => TOWN_CHANNEL_SECONDS * (walkSpeed > 0f ? walkSpeed : NOMINAL_WALK_SPEED) * TOWN_RISK_PREMIUM;
}