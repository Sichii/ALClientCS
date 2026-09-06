#region
using AL.Core.Interfaces;
using AL.Pathfinding.Definitions;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     One leg of a path. On a <see cref="EdgeType.Door" /> or <see cref="EdgeType.Transport" /> leg <see cref="Start" />
///     is the <see cref="AL.Data.Exit" /> being used, so a caller can read the spawn index it lands on.
///     <see cref="Cost" /> is in walk-distance units for every type.
/// </summary>
public readonly record struct PathEdge(
    EdgeType Type,
    ILocation Start,
    ILocation End,
    float Cost)
{
    /// <summary>
    ///     The leg as its type, both ends and cost, for logs.
    /// </summary>
    public override string ToString() => $"{Type} {ILocation.ToString(Start)} -> {ILocation.ToString(End)} ({Cost:F1})";
}