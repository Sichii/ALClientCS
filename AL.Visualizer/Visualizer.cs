#region
using AL.Data;
using AL.Pathfinding;
using AL.Pathfinding.Model;
using AL.Visualizer.Extensions;
using AL.Visualizer.Model;
using Chaos.Extensions.Common;
using SkiaSharp;
#endregion

namespace AL.Visualizer;

/// <summary>
///     Provides some short-handed ways of visualizing a navmesh.
/// </summary>
public static class Visualizer
{
    /// <summary>
    ///     Creates a white canvas the size of the map with its wall lines drawn in black. Layer more on with
    ///     <see cref="PixelCanvasExtensions" />.
    /// </summary>
    /// <param name="navMesh">
    ///     The navmesh whose map to draw.
    /// </param>
    /// <returns>
    ///     The canvas with the map's walls drawn on it.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     navMesh
    /// </exception>
    public static PixelCanvas CreateGridImage(NavMesh navMesh)
    {
        ArgumentNullException.ThrowIfNull(navMesh);

        var geometry = GameData.Geometry[navMesh.Map]!;
        var canvas = new PixelCanvas(geometry.MaxX - geometry.MinX + 1, geometry.MaxY - geometry.MinY + 1, SKColors.White);

        foreach (var line in geometry.VerticalLines.Concat(geometry.HorizontalLines))
            canvas.DrawLine(
                navMesh.ToCanvas(line.Point1),
                navMesh.ToCanvas(line.Point2),
                SKColors.Black,
                SKColors.Black);

        return canvas;
    }

    /// <summary>
    ///     Draws one image per map the path crosses, each with the map's triangles and the legs walked on it.
    /// </summary>
    /// <param name="path">
    ///     The path to draw.
    /// </param>
    /// <param name="color">
    ///     The color to draw the legs in.
    /// </param>
    /// <returns>
    ///     One canvas per map, in the order the path crosses them.
    /// </returns>
    public static IEnumerable<PixelCanvas> DrawPath(IReadOnlyList<PathEdge> path, SKColor color = default)
    {
        NavMesh? currentMesh = null;
        PixelCanvas? currentCanvas = null;
        var currentPath = new List<PathEdge>();

        foreach (var edge in path)
        {
            if ((currentMesh == null) || !currentMesh.Map.EqualsI(edge.Start.Map))
            {
                if ((currentMesh != null) && (currentCanvas != null))
                {
                    //a map crossed without a walk, landed on and left by its spawn, has no legs to draw
                    if (currentPath.Count > 0)
                        currentCanvas.DrawPath(currentMesh, currentPath, color);

                    currentPath.Clear();

                    yield return currentCanvas;
                }

                currentMesh = Pathfinder.GetNavMesh(edge.Start.Map);

                if (currentMesh is null)
                    yield break;

                currentCanvas = CreateGridImage(currentMesh)
                    .DrawEdges(currentMesh);
            }

            if (!edge.End.Map.EqualsI(currentMesh.Map))
                continue;

            currentPath.Add(edge);
        }

        if ((currentCanvas == null) || (currentMesh == null))
            yield break;

        if (currentPath.Count > 0)
            currentCanvas.DrawPath(currentMesh, currentPath, color);

        yield return currentCanvas;
    }
}