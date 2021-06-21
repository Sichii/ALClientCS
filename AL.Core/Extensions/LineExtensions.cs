using System;
using System.Collections.Generic;
using AL.Core.Geometry;
using AL.Core.Interfaces;

namespace AL.Core.Extensions
{
    public static class LineExtensions
    {
        /// <summary>
        ///     https://playtechs.blogspot.com/2007/03/raytracing-on-grid.html
        /// </summary>
        public static IEnumerable<IPoint> Points(this ILine line)
        {
            var x0 = line.Point1.X;
            var y0 = line.Point1.Y;
            var x1 = line.Point2.X;
            var y1 = line.Point2.Y;
            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var x = (int) Math.Floor(x0);
            var y = (int) Math.Floor(y0);
            var n = 1;
            int xOffset;
            int yOffset;
            float error;

            if (dx == 0)
            {
                xOffset = 0;
                error = float.PositiveInfinity;
            } else if (x1 > x0)
            {
                xOffset = 1;
                n += (int) Math.Floor(x1) - x;
                error = (float) (Math.Floor(x0) + 1 - x0) * dy;
            } else
            {
                xOffset = -1;
                n += x - (int) Math.Floor(x1);
                error = (float) (x0 - Math.Floor(x0)) * dy;
            }

            if (dy == 0)
            {
                yOffset = 0;
                error -= float.PositiveInfinity;
            } else if (y1 > y0)
            {
                yOffset = 1;
                n += (int) Math.Floor(y1) - y;
                error -= (float) (Math.Floor(y0) + 1 - y0) * dx;
            } else
            {
                yOffset = -1;
                n += y - (int) Math.Floor(y1);
                error -= (float) (y0 - Math.Floor(y0)) * dx;
            }

            for (; n > 0; --n)
            {
                yield return new Point(x, y);

                if (error > 0)
                {
                    y += yOffset;
                    error -= dx;
                } else
                {
                    x += xOffset;
                    error += dy;
                }
            }
        }
    }
}