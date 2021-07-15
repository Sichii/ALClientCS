using AL.Core.Interfaces;

namespace AL.Core.Extensions
{
    /// <summary>
    /// Provides a set of extensions for <see cref="IPolygon"/>s.
    /// </summary>
    public static class PolygonExtensions
    {
        /// <summary>
        /// Determins if a given <see cref="IPoint"/> is inside of a <see cref="IPolygon"/>. <br/>
        /// https://wrf.ecse.rpi.edu//Research/Short_Notes/pnpoly.html
        /// </summary>
        /// <param name="polygon">A polygon.</param>
        /// <param name="point">A point.</param>
        /// <returns><see cref="bool"/> <br/>
        /// <c>true</c> if the point lies inside(or on the endge) the polygon, otherwise <c>false</c>.</returns>
        public static bool Contains(this IPolygon polygon, IPoint point)
        {
            var inside = false;
            var count = polygon.Vertices.Count;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                var iVertex = polygon.Vertices[i];
                var jVertex = polygon.Vertices[j];

                //long form version of pnpoly, allowing for fast fails
                if ((((iVertex.Y < point.Y) && (jVertex.Y >= point.Y))
                     || ((jVertex.Y < point.Y) && (iVertex.Y >= point.Y)))
                    && ((iVertex.X <= point.X) || (jVertex.X <= point.X)))
                    inside ^= iVertex.X + (point.Y - iVertex.Y) / (jVertex.Y - iVertex.Y) * (jVertex.X - iVertex.X)
                              < point.X;
            }

            return inside;
        }
    }
}