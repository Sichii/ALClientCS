using System;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Pathfinding.Interfaces;

namespace AL.Pathfinding.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for <see cref="IConnector{TEdge}" />s.
    /// </summary>
    public static class ConnectorExtensions
    {
        /// <summary>
        ///     Converts an implementation of <see cref="IConnector{TEdge}" /> to an <see cref="ILine" />.
        /// </summary>
        /// <param name="connector">A connector.</param>
        /// <typeparam name="T">The edge type of the connector. Must implement <see cref="IPoint" />.</typeparam>
        /// <returns>
        ///     <see cref="Line" />. <br />
        ///     An object representing a line drawn from point to point.
        /// </returns>
        /// <exception cref="ArgumentNullException">connector</exception>
        public static Line ToLine<T>(this IConnector<T> connector) where T: IPoint
        {
            if (connector == null)
                throw new ArgumentNullException(nameof(connector));

            return new Line(connector.Start, connector.End);
        }
    }
}