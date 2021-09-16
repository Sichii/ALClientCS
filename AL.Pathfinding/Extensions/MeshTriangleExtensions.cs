using AL.Core.Interfaces;
using AL.Pathfinding.Interfaces;

namespace AL.Pathfinding.Extensions
{
    public static class MeshTriangleExtensions
    {
        public static bool Contains<TNode, TEdge, TVertex>(this IGenericTriangle<TNode> genericTriangle, TVertex vertex)
            where TNode: IGraphNode2<TEdge> where TVertex: ILocation
        {
            var p1 = genericTriangle.Vertices[0].Vertex;
            var p2 = genericTriangle.Vertices[1].Vertex;
            var p3 = genericTriangle.Vertices[2].Vertex;

            var alpha = ((p2.Y - p3.Y) * (vertex.X - p3.X) + (p3.X - p2.X) * (vertex.Y - p3.Y))
                        / ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));

            var beta = ((p3.Y - p1.Y) * (vertex.X - p3.X) + (p1.X - p3.X) * (vertex.Y - p3.Y))
                       / ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));

            var gamma = 1.0f - alpha - beta;

            return ((alpha > 0) && (beta > 0) && (gamma > 0)) || ((alpha < 0) && (beta < 0) && (gamma < 0));
        }
    }
}