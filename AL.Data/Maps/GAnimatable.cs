#region
using System.Text.Json.Serialization;
using AL.Core.Geometry;
#endregion

namespace AL.Data.Maps;

/// <summary>
///     Represents a piece of composed scenery placed on a map, such as the dungeon gate on <c>main</c> .
/// </summary>
/// <remarks>
///     Scenery is drawn from the map's own art, so most of it is presentation this client has no use for. What matters is
///     <see cref="Collision" /> : the game folds those boxes into the map's wall lines while it processes the map, which
///     means a piece of scenery blocks movement without appearing anywhere in <c>G.geometry</c> .
/// </remarks>
public sealed record GAnimatable
{
    /// <summary>
    ///     If populated, the boxes this scenery blocks, each as <c>[x1, y1, x2, y2]</c> offsets from <see cref="X" /> and
    ///     <see cref="Y" /> .
    /// </summary>
    [JsonPropertyName("collision")]
    public IReadOnlyList<IReadOnlyList<int>>? Collision { get; init; }

    /// <summary>
    ///     The X coordinate the scenery is placed at, which its collision boxes are measured from.
    /// </summary>
    public int X { get; init; }

    /// <summary>
    ///     The Y coordinate the scenery is placed at, which its collision boxes are measured from.
    /// </summary>
    public int Y { get; init; }

    //unmapped: position (obj), role (str) - both presentation

    /// <summary>
    ///     The wall lines this scenery's collision boxes stand for, in map coordinates: each box's four sides.
    /// </summary>
    /// <remarks>
    ///     Duplicates and overlaps are left in. The caller merges them along with the map's own lines, which is where every
    ///     other source of walls is reconciled too.
    /// </remarks>
    /// <returns>
    ///     One line per box side, or nothing when the scenery blocks nothing. A box of fewer than four numbers is skipped
    ///     rather than guessed at - half a box is a wall in the wrong place, which is worse than no wall.
    /// </returns>
    public IEnumerable<StraightLine> CollisionLines()
    {
        if (Collision is not { Count: > 0 } boxes)
            yield break;

        foreach (var box in boxes)
        {
            if (box.Count < 4)
                continue;

            var left = X + box[0];
            var top = Y + box[1];
            var right = X + box[2];
            var bottom = Y + box[3];

            yield return new StraightLine(
                left,
                top,
                bottom,
                true);

            yield return new StraightLine(
                right,
                top,
                bottom,
                true);

            yield return new StraightLine(
                top,
                left,
                right,
                false);

            yield return new StraightLine(
                bottom,
                left,
                right,
                false);
        }
    }
}