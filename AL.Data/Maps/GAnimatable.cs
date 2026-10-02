#region
using System.Text.Json.Serialization;
using AL.Core.Geometry;
#endregion

namespace AL.Data.Maps;

/// <summary>
///     Represents a piece of composed scenery placed on a map, such as the dungeon gate on <c>main</c>.
/// </summary>
/// <remarks>
///     The game folds the <see cref="Collision" /> boxes into the map's wall lines, so scenery blocks movement without
///     appearing in <c>G.geometry</c>.
/// </remarks>
public sealed record GAnimatable
{
    /// <summary>
    ///     If populated, the boxes this scenery blocks, each as <c>[x1, y1, x2, y2]</c> offsets from <see cref="X" /> and
    ///     <see cref="Y" />.
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
    ///     Builds the wall lines this scenery's collision boxes stand for, in map coordinates: each box's four sides.
    /// </summary>
    /// <returns>
    ///     One line per box side, or nothing when the scenery blocks nothing. A box of fewer than four numbers is skipped,
    ///     and duplicates are left for the caller to merge.
    /// </returns>
    public IEnumerable<StraightLine> BuildCollisionLines()
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