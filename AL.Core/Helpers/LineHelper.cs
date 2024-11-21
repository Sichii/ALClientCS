#region
using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Geometry;
using Common.Logging;
#endregion

namespace AL.Core.Helpers;

/// <summary>
///     Provides a set of helper methods for interacting with <see cref="Interfaces.ILine" />.
/// </summary>
public static class LineHelper
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(LineHelper).FullName);

    /// <summary>
    ///     A helper method for fixing overlapping lines.
    /// </summary>
    /// <param name="lines">
    ///     The lines to fix.
    /// </param>
    /// <param name="isVertical">
    ///     Whether or not the lines are X lines or not. (X lines are vertical lines)
    /// </param>
    /// <returns>
    ///     <see cref="Array" /> of <see cref="StraightLine" />
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     lines
    /// </exception>
    public static StraightLine[] FixLines(IReadOnlyList<StraightLine> lines, bool isVertical)
    {
        ArgumentNullException.ThrowIfNull(lines);

        if (lines.Count == 0)
            return [];

        var newLines = new HashSet<StraightLine>();

        for (var i = 0; i < lines.Count; i++)
        {
            var line1 = lines[i] with
            {
                IsVertical = isVertical
            };

            for (var i2 = 0; i2 < lines.Count; i2++)
            {
                var line2 = lines[i2] with
                {
                    IsVertical = isVertical
                };

                if ((i != i2) && line1.Overlaps(line2))
                {
                    Logger.Trace(
                        $@"Merging lines
{line1}
{lines[i2]}
");

                    line1 = line1.Merge(line2);

                    break;
                }
            }

            newLines.Add(line1);
        }

        return newLines.ToArray();
    }
}