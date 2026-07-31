#region
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Interfaces;
using AL.Visualizer.Model;
using Priority_Queue;
using SkiaSharp;
#endregion

namespace AL.Visualizer.Extensions;

/// <summary>
///     Provides a set of extensions for <see cref="PixelCanvas" />.
/// </summary>
public static class PixelCanvasExtensions
{
    /// <summary>
    ///     Draws all connections between all navMesh and their neighbors on a canvas.
    /// </summary>
    /// <param name="canvas">
    ///     The canvas to draw on.
    /// </param>
    /// <param name="navMesh">
    ///     The navMesh to draw connections for.
    /// </param>
    /// <param name="color">
    ///     The color to draw the connections as.
    /// </param>
    /// <returns>
    ///     <see cref="PixelCanvas" />
    ///     <br />
    ///     The canvas with the connections drawn on it.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     canvas
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     navMesh
    /// </exception>
    public static PixelCanvas DrawEdges<TNode, TEdge>(this PixelCanvas canvas, MeshBase<TNode, TEdge> navMesh, SKColor color = default)
        where TEdge: IGraphEdge<TNode>, new()
        where TNode: FastPriorityQueueNode, IGraphNode<TEdge>

    {
        ArgumentNullException.ThrowIfNull(canvas);

        ArgumentNullException.ThrowIfNull(navMesh);

        if (color == default)
            color = SKColors.Red;

        foreach (var edge in navMesh.TraverseEdges())
        {
            var start = navMesh.ApplyOffset(edge.Start.Vertex);
            var midEnd = navMesh.ApplyOffset(edge.End.Vertex.MidPoint(edge.Start.Vertex));

            canvas.DrawLine(start, midEnd, color);
        }

        return canvas;
    }

    /// <summary>
    ///     Draws a line on a canvas.
    /// </summary>
    /// <param name="canvas">
    ///     The canvas to draw on.
    /// </param>
    /// <param name="line">
    ///     The line to draw on the canvas.
    /// </param>
    /// <param name="color">
    ///     The color to draw the line.
    /// </param>
    /// <param name="ptColor">
    ///     The color to draw the pixel the start/end points.
    /// </param>
    /// <returns>
    ///     <see cref="PixelCanvas" />
    ///     <br />
    ///     The canvas with the line drawn on it.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     canvas
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     line
    /// </exception>
    public static PixelCanvas DrawLine<TLine>(
        this PixelCanvas canvas,
        TLine line,
        SKColor color = default,
        SKColor ptColor = default) where TLine: ILine
    {
        ArgumentNullException.ThrowIfNull(canvas);

        ArgumentNullException.ThrowIfNull(line);

        return canvas.DrawLine(
            line.Point1,
            line.Point2,
            color,
            ptColor);
    }

    /// <summary>
    ///     Draws a line on a canvas.
    /// </summary>
    /// <param name="canvas">
    ///     The canvas to draw on.
    /// </param>
    /// <param name="start">
    ///     The start of the line.
    /// </param>
    /// <param name="end">
    ///     The end of the line.
    /// </param>
    /// <param name="color">
    ///     The color to draw the line.
    /// </param>
    /// <param name="ptColor">
    ///     The color to draw the pixel the start/end points.
    /// </param>
    /// <returns>
    ///     <see cref="PixelCanvas" />
    ///     <br />
    ///     The canvas with the line drawn on it.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     canvas
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     start
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     end
    /// </exception>
    public static PixelCanvas DrawLine<TPoint>(
        this PixelCanvas canvas,
        TPoint start,
        TPoint end,
        SKColor color = default,
        SKColor ptColor = default) where TPoint: IPoint
    {
        ArgumentNullException.ThrowIfNull(canvas);

        ArgumentNullException.ThrowIfNull(start);

        ArgumentNullException.ThrowIfNull(end);

        if (color == default)
            color = SKColors.Gold;

        if (ptColor == default)
            ptColor = SKColors.Magenta;

        foreach ((var x, var y) in new Line(start, end).Points())
            canvas[Math.Clamp(Convert.ToInt32(x), 0, canvas.Width - 1), Math.Clamp(Convert.ToInt32(y), 0, canvas.Height - 1)] = color;

        canvas[Math.Clamp(Convert.ToInt32(start.X), 0, canvas.Width - 1), Math.Clamp(Convert.ToInt32(start.Y), 0, canvas.Height - 1)]
            = ptColor;

        canvas[Math.Clamp(Convert.ToInt32(end.X), 0, canvas.Width - 1), Math.Clamp(Convert.ToInt32(end.Y), 0, canvas.Height - 1)] = ptColor;

        return canvas;
    }

    /// <summary>
    ///     Draws a path along a number of path connectors on a canvas.
    /// </summary>
    /// <param name="canvas">
    ///     The canvas to draw on.
    /// </param>
    /// <param name="navMesh">
    ///     The navmesh this path is for.
    /// </param>
    /// <param name="pathConnectors">
    ///     The connectors to draw.
    /// </param>
    /// <param name="color">
    ///     The color to draw the path.
    /// </param>
    /// <returns>
    ///     <see cref="PixelCanvas" />
    ///     <br />
    ///     The canvas with the path drawn on it.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     canvas
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     pathConnectors
    /// </exception>
    public static PixelCanvas DrawPath<TNode, TEdge>(
        this PixelCanvas canvas,
        MeshBase<TNode, TEdge> navMesh,
        IEnumerable<TEdge?> pathConnectors,
        SKColor color = default) where TEdge: IGraphEdge<TNode>, new()
                                 where TNode: FastPriorityQueueNode, IGraphNode<TEdge>

    {
        ArgumentNullException.ThrowIfNull(canvas);

        ArgumentNullException.ThrowIfNull(pathConnectors);

        IEnumerable<IPoint> SelectPoints(IEnumerable<TEdge?> connectors)
        {
            foreach (var edge in connectors)
                if (edge is not null)
                {
                    yield return navMesh.ApplyOffset(edge.Start.Vertex);
                    yield return navMesh.ApplyOffset(edge.End.Vertex);
                }
        }

        return canvas.DrawPath(SelectPoints(pathConnectors), color);
    }

    /// <summary>
    ///     Draws a path along a number of points on a canvas.
    /// </summary>
    /// <param name="canvas">
    ///     The canvas to draw on.
    /// </param>
    /// <param name="points">
    ///     The points to draw the path along.
    /// </param>
    /// <param name="color">
    ///     The color to draw the path.
    /// </param>
    /// <returns>
    ///     <see cref="PixelCanvas" />
    ///     <br />
    ///     The canvas with the path drawn on it.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     canvas
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     points
    /// </exception>
    public static PixelCanvas DrawPath<TPoint>(this PixelCanvas canvas, IEnumerable<TPoint> points, SKColor color = default)
        where TPoint: IPoint
    {
        ArgumentNullException.ThrowIfNull(canvas);

        ArgumentNullException.ThrowIfNull(points);

        if (color == default)
            color = SKColors.Gold;

        _ = points.Aggregate((prev, cur) =>
        {
            canvas.DrawLine(prev, cur, color);

            return cur;
        });

        return canvas;
    }
}
