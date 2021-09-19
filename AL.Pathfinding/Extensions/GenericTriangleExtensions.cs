using AL.Core.Interfaces;
using AL.Pathfinding.Interfaces;

namespace AL.Pathfinding.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for <see cref="IGenericTriangle{TVertex}" />s.
    /// </summary>
    public static class GenericTriangleExtensions
    {
        /// <summary>
        ///     Determines if a triangle of nodes contains a location in it's area.
        /// </summary>
        /// <param name="genericTriangle">A triangle of graph nodes.</param>
        /// <param name="point"></param>
        /// <typeparam name="TNode">An implementation of <see cref="IGraphNode{TEdge}" />.</typeparam>
        /// <typeparam name="TEdge">The edge type of the node.</typeparam>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if the point is contained within the triangle, otherwise <c>false</c>.
        /// </returns>
        public static bool Contains<TNode, TEdge>(this IGenericTriangle<TNode> genericTriangle, IPoint point) where TNode: IGraphNode<TEdge>
        {
            var p1 = genericTriangle.Vertices[0].Vertex;
            var p2 = genericTriangle.Vertices[1].Vertex;
            var p3 = genericTriangle.Vertices[2].Vertex;

            var alpha = ((p2.Y - p3.Y) * (point.X - p3.X) + (p3.X - p2.X) * (point.Y - p3.Y))
                        / ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));

            var beta = ((p3.Y - p1.Y) * (point.X - p3.X) + (p1.X - p3.X) * (point.Y - p3.Y))
                       / ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));

            var gamma = 1.0f - alpha - beta;

            return ((alpha > 0) && (beta > 0) && (gamma > 0)) || ((alpha < 0) && (beta < 0) && (gamma < 0));
        }
    }
}