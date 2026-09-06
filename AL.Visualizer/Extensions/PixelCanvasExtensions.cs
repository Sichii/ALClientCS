#region
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Pathfinding.Model;
using AL.Visualizer.Model;
using SkiaSharp;
#endregion

namespace AL.Visualizer.Extensions;

/// <summary>
///     Provides a set of extensions for <see cref="PixelCanvas" />.
/// </summary>
public static class PixelCanvasExtensions
{
    /// <summary>
    ///     The point in canvas pixels for a map point.
    /// </summary>
    public static IPoint ToCanvas(this NavMesh navMesh, IPoint point)
    {
        var geometry = GameData.Geometry[navMesh.Map]!;

        return new Point(point.X - geometry.MinX, point.Y - geometry.MinY);
    }

    /// <summary>
    ///     Draws every triangle edge of the mesh.
    /// </summary>
    public static PixelCanvas DrawEdges(this PixelCanvas canvas, NavMesh navMesh, SKColor color = default)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(navMesh);

        if (color == default)
            color = SKColors.Red;

        var mesh = navMesh.Mesh;

        for (var triangle = 0; triangle < mesh.TriangleCount; triangle++)
            for (var slot = 0; slot < 3; slot++)
            {
                var a = mesh.Vertices[mesh.Corners[triangle * 3 + slot]];
                var b = mesh.Vertices[mesh.Corners[triangle * 3 + (slot + 1) % 3]];

                canvas.DrawLine(
                    navMesh.ToCanvas(a),
                    navMesh.ToCanvas(b),
                    color,
                    color);
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
    ///     Draws the legs of a path that lie on the mesh's map.
    /// </summary>
    public static PixelCanvas DrawPath(
        this PixelCanvas canvas,
        NavMesh navMesh,
        IEnumerable<PathEdge> path,
        SKColor color = default)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(path);

        IEnumerable<IPoint> SelectPoints()
        {
            foreach (var edge in path)
            {
                yield return navMesh.ToCanvas(edge.Start);
                yield return navMesh.ToCanvas(edge.End);
            }
        }

        return canvas.DrawPath(SelectPoints(), color);
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
