#region
using System;
using System.Collections.Generic;
using AL.Core.Geometry;
using AL.Core.Interfaces;
#endregion

namespace AL.Core.Extensions;

/// <summary>
///     Provides a set of extensions for <see cref="ILine" />s.
/// </summary>
public static class LineExtensions
{
    /// <summary>
    ///     <inheritdoc cref="PointExtensions.RayTraceTo" />
    /// </summary>
    /// <param name="line">
    ///     The line to generate points for.
    /// </param>
    /// <returns>
    ///     <inheritdoc cref="PointExtensions.RayTraceTo" />
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     line
    /// </exception>
    public static IEnumerable<Point> Points(this ILine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return line.Point1.RayTraceTo(line.Point2);
    }
}