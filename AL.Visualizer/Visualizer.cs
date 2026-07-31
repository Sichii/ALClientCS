#region
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;
using AL.Visualizer.Extensions;
using AL.Visualizer.Model;
using Chaos.Extensions.Common;
using Priority_Queue;
using SkiaSharp;
#endregion

namespace AL.Visualizer;

/// <summary>
///     Provides some short-handed ways of visualizing a navmesh.
/// </summary>
public static class Visualizer
{
    /// <summary>
    ///     Creates a basic image of the map for a <see cref="MeshBase{TNode,TEdge}" />.
    /// </summary>
    /// <param name="navMesh">
    ///     The navmesh to create an image for.
    /// </param>
    /// <returns>
    ///     <see cref="PixelCanvas" />
    ///     <br />
    ///     An image representing the map. It's not exact (it doesnt use tiles), but it gives you a useable 1 to 1
    ///     visualization.
    ///     <br />
    ///     You can use <see cref="PixelCanvasExtensions" /> to layer on more information about the
    ///     <see cref="MeshBase{TNode,TEdge}" /> .
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     navMesh
    /// </exception>
    public static PixelCanvas CreateGridImage<TNode, TEdge>(MeshBase<TNode, TEdge> navMesh) where TEdge: IGraphEdge<TNode>, new()
        where TNode: FastPriorityQueueNode, IGraphNode<TEdge>

    {
        ArgumentNullException.ThrowIfNull(navMesh);

        var pointMap = navMesh.PointMap;
        var width = pointMap.GetLength(0);
        var height = pointMap.GetLength(1);
        var canvas = new PixelCanvas(width, height, SKColors.White);

        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                canvas[x, y] = PointTypeToColor(pointMap[x, y]);

        return canvas;
    }

    public static async IAsyncEnumerable<PixelCanvas> DrawPath<TGraph, TMesh, TNode, TEdge>(
        TGraph graph,
        IAsyncEnumerable<TEdge> path,
        SKColor color = default) where TGraph: GraphBase<TMesh, TNode, TEdge>
                                 where TEdge: IGraphEdge<TNode>, new()
                                 where TNode: FastPriorityQueueNode, IGraphNode<TEdge>
                                 where TMesh: MeshBase<TNode, TEdge>

    {
        TMesh? currentMesh = null;
        PixelCanvas? currentCanvas = null;
        var currentPath = new List<TEdge>();

        await foreach (var edge in path)
        {
            if ((currentMesh == null) || !currentMesh.Map.EqualsI(edge.Start.Vertex.Map))
            {
                if ((currentMesh != null) && (currentCanvas != null))
                {
                    currentCanvas.DrawPath(currentMesh, currentPath, color);
                    currentPath.Clear();

                    yield return currentCanvas;
                }

                currentMesh = graph.NavMeshes[edge.Start.Vertex.Map];

                currentCanvas = CreateGridImage(currentMesh)
                    .DrawEdges(currentMesh);
            }

            if (!edge.End.Vertex.Map.EqualsI(currentMesh.Map))
                continue;

            currentPath.Add(edge);
        }

        if (currentCanvas == null)
            yield break;

        currentCanvas.DrawPath(currentMesh!, currentPath, color);

        yield return currentCanvas;
    }

    private static SKColor PointTypeToColor(PointType type)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (type)
        {
            case PointType.None:
                return SKColors.DarkBlue;
            case PointType.Wall:
                return SKColors.Black;
            case PointType.Walkable:
                return SKColors.Green;
            case PointType.Inline:
                return SKColors.Yellow;
            case PointType.Vertex:
                return SKColors.Red;
            default:
                if (type.HasFlag(PointType.Vertex))
                    return SKColors.Red;

                if (type.HasFlag(PointType.Inline))
                    return SKColors.Yellow;

                throw new ArgumentOutOfRangeException($"Unknown point type {(int)type}");
        }
    }
}
