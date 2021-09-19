using System;
using System.Collections.Generic;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;
using AL.Visualizer.Extensions;
using Chaos.Core.Extensions;
using Priority_Queue;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AL.Visualizer
{
    /// <summary>
    ///     Provides some short-handed ways of visualizing a navmesh.
    /// </summary>
    public static class Visualizer
    {
        /// <summary>
        ///     Creates a basic image of the map for a <see cref="MeshBase{TNode,TEdge}" />.
        /// </summary>
        /// <param name="navMesh">The navmesh to create an image for.</param>
        /// <returns>
        ///     <see cref="Image{TPixel}" /> <br />
        ///     An image representing the map. It's not exact (it doesnt use tiles), but it gives you a useable 1 to 1
        ///     visualization. <br />
        ///     You can use <see cref="SixLabors.ImageSharp.ImageExtensions" /> to layer on more information about the
        ///     <see cref="MeshBase{TNode,TEdge}" />
        ///     .
        /// </returns>
        /// <exception cref="ArgumentNullException">navMesh</exception>
        public static Image<Rgba32> CreateGridImage<TNode, TEdge>(MeshBase<TNode, TEdge> navMesh)
            where TEdge: IGraphEdge<TNode>, new() where TNode: FastPriorityQueueNode, IGraphNode<TEdge>

        {
            if (navMesh == null)
                throw new ArgumentNullException(nameof(navMesh));

            var pointMap = navMesh.PointMap;
            var width = pointMap.GetLength(0);
            var height = pointMap.GetLength(1);
            var image = new Image<Rgba32>(width, height, Color.White);

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    image[x, y] = PointTypeToColor(pointMap[x, y]);

            return image;
        }
        /*
                 public static Image<Rgba32> DrawPath<TEdge, TNode>(
            this Image<Rgba32> image,
            NavMesh2 navMesh,
            IEnumerable<TEdge?> pathConnectors,
            Color color = default) where TEdge: IGraphEdge<TNode> where TNode: IGraphNode2<TEdge>
         */

        public static async IAsyncEnumerable<Image<Rgba32>> DrawPath<TGraph, TMesh, TNode, TEdge>(
            TGraph graph,
            IAsyncEnumerable<TEdge> path,
            Color color = default) where TGraph: GraphBase<TMesh, TNode, TEdge> where TEdge: IGraphEdge<TNode>, new()
                                   where TNode: FastPriorityQueueNode, IGraphNode<TEdge> where TMesh: MeshBase<TNode, TEdge>

        {
            TMesh? currentMesh = null;
            Image<Rgba32>? currentImage = null;
            var currentPath = new List<TEdge>();

            await foreach (var edge in path)
            {
                if ((currentMesh == null) || !currentMesh.Map.EqualsI(edge.Start.Vertex.Map))
                {
                    if ((currentMesh != null) && (currentImage != null))
                    {
                        currentImage.DrawPath(currentMesh, currentPath, color);
                        currentPath.Clear();

                        yield return currentImage;
                    }

                    currentMesh = graph.NavMeshes[edge.Start.Vertex.Map];
                    currentImage = CreateGridImage(currentMesh).DrawEdges(currentMesh);
                }

                if (!edge.End.Vertex.Map.EqualsI(currentMesh.Map))
                    continue;

                currentPath.Add(edge);
            }

            currentImage!.DrawPath(currentMesh!, currentPath, color);
        }

        private static Color PointTypeToColor(PointType type)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (type)
            {
                case PointType.None:
                    return Color.DarkBlue;
                case PointType.Wall:
                    return Color.Black;
                case PointType.Walkable:
                    return Color.Green;
                case PointType.Inline:
                    return Color.Yellow;
                case PointType.Vertex:
                    return Color.Red;
                default:
                    if (type.HasFlag(PointType.Vertex))
                        return Color.Red;

                    if (type.HasFlag(PointType.Inline))
                        return Color.Yellow;

                    throw new ArgumentOutOfRangeException($"Unknown point type {(int)type}");
            }
        }
    }
}