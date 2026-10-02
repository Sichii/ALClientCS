#region
using AL.Core.Interfaces;
using AL.Pathfinding.Definitions;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents one leg of a path, its <see cref="Cost" /> in walk distance. On a <see cref="EdgeType.Door" /> or
///     <see cref="EdgeType.Transport" /> leg <see cref="Start" /> is the <see cref="AL.Data.Exit" /> being used.
/// </summary>
public readonly record struct PathEdge(
    EdgeType Type,
    ILocation Start,
    ILocation End,
    float Cost)
{
    /// <summary>
    ///     Formats the leg as its type, both ends and cost, for logs.
    /// </summary>
    /// <returns>The leg as one line of text.</returns>
    public override string ToString() => $"{Type} {ILocation.ToString(Start)} -> {ILocation.ToString(End)} ({Cost:F1})";
}