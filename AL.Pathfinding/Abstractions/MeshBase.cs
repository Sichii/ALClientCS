using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Extensions;
using AL.Pathfinding.Interfaces;
using Priority_Queue;

namespace AL.Pathfinding.Abstractions
{
    public abstract class MeshBase<TNode, TEdge, TVertex, TTriangle> : IEnumerable<TNode>
        where TNode: FastPriorityQueueNode, IGraphNode2<TEdge> where TTriangle: IGenericTriangle<TNode>
        where TVertex: ILocation, IEquatable<TVertex> where TEdge: IGraphEdge<TNode>, new()
    {
        protected internal readonly TNode? TownNode;
        protected internal string Map { get; set; }
        protected internal PointType[,] PointMap { get; set; }
        protected internal ICollection<TTriangle> Triangles { get; set; }
        protected internal int XOffset { get; }
        protected internal int YOffset { get; }

        protected MeshBase(
            string map,
            IEnumerable<IGenericTriangle<ILocation>> triangles,
            PointType[,] pointMap,
            int xOffset,
            int yOffset)
        {
            Map = map;
            PointMap = pointMap;
            XOffset = xOffset;
            YOffset = yOffset;
            Triangles = BuildConnections(triangles);
            TownNode = CreateTownNode();
        }

        public virtual IPoint ApplyOffset(IPoint point) => new Point(point.X + XOffset, point.Y + YOffset);

        private ICollection<TTriangle> BuildConnections(IEnumerable<IGenericTriangle<ILocation>> triangles)
        {
            var nodeDic = new Dictionary<TVertex, TNode>();
            var nodeTriangles = new HashSet<TTriangle>();

            foreach (var triangle in triangles)
            {
                var loc1 = triangle.Vertices[0];
                var loc2 = triangle.Vertices[1];
                var loc3 = triangle.Vertices[2];
                var offsetPoint1 = RemoveOffset(loc1);
                var offsetPoint2 = RemoveOffset(loc2);
                var offsetPoint3 = RemoveOffset(loc3);
                var vertex1 = ConstructVertex(new Location(loc1.Map, offsetPoint1));
                var vertex2 = ConstructVertex(new Location(loc2.Map, offsetPoint2));
                var vertex3 = ConstructVertex(new Location(loc3.Map, offsetPoint3));

                if (!nodeDic.TryGetValue(vertex1, out var node1))
                {
                    node1 = ConstructNode(vertex1);
                    nodeDic[vertex1] = node1;
                }

                if (!nodeDic.TryGetValue(vertex2, out var node2))
                {
                    node2 = ConstructNode(vertex2);
                    nodeDic[vertex2] = node2;
                }

                if (!nodeDic.TryGetValue(vertex3, out var node3))
                {
                    node3 = ConstructNode(vertex3);
                    nodeDic[vertex3] = node3;
                }

                node1.Edges.Add(ConstructEdge(node1, node2));
                node1.Edges.Add(ConstructEdge(node1, node3));
                node2.Edges.Add(ConstructEdge(node2, node1));
                node2.Edges.Add(ConstructEdge(node2, node3));
                node3.Edges.Add(ConstructEdge(node3, node1));
                node3.Edges.Add(ConstructEdge(node3, node2));

                nodeTriangles.Add(ConstructTriangle(node1, node2, node3));
            }

            return nodeTriangles;
        }

        protected internal virtual float CalculateHeuristic(TVertex start, TVertex end) => start.FastDistance(end);

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public virtual bool CanMove(IPoint start, IPoint end)
        {
            var startOffset = ApplyOffset(start);
            var endOffset = ApplyOffset(end);

            return !startOffset.RayTraceTo(endOffset)
                .Any(p => PointMap[Convert.ToInt32(p.X), Convert.ToInt32(p.Y)].HasFlag(PointType.Wall));
        }

        protected internal virtual ConnectorType ConnectorTypeSelector(TVertex start, TVertex end)
        {
            var gMap1 = GameData.Maps[start.Map]!;

            if (!start.OnSameMapAs(end))
            {
                if (gMap1 is { Irregular: true })
                    return ConnectorType.Leave;

                if (gMap1.Doors.Any(door => door.Equals(start)))
                    return ConnectorType.Door;
            }

            var transport =
                gMap1.NPCs.FirstOrDefault(npc => npc.Locations.Contains((ILocation)start) && (npc.Data!.Role == NPCRole.Transport));

            if (transport != null)
                return ConnectorType.Transport;

            if ((TownNode != null) && TownNode.Vertex.Equals(end))
                return ConnectorType.Town;

            return ConnectorType.Walk;
        }

        protected internal abstract TEdge ConstructEdge(TNode start, TNode end, ConnectorType? typeOverride = null);

        protected internal abstract TNode ConstructNode(TVertex vertex);

        protected internal abstract TTriangle ConstructTriangle(TNode node1, TNode node2, TNode node3);

        protected internal abstract TVertex ConstructVertex(ILocation location);

        private TNode? CreateTownNode()
        {
            var gMap = GameData.Maps[Map];

            if (gMap == null)
                throw new InvalidOperationException($"Missing map metadata for {Map}");

            if (gMap.Boundless)
                return null;

            var spawn = gMap.Spawns.Count > 0 ? gMap.Spawns[0] : default;

            if (spawn == null)
                return null;

            var spawnLocation = ConstructVertex(new Location(Map, spawn.X, spawn.Y));
            var node = ConstructNode(spawnLocation);

            var containingTriangle = GetContainingTriangle(spawnLocation);

            if (containingTriangle == null)
                throw new InvalidOperationException("Spawn located out of bounds");

            foreach (var nodeVertex in containingTriangle)
                node.Edges.Add(ConstructEdge(node, nodeVertex));

            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        protected internal virtual TNode FindBestNode(TVertex vertex)
        {
            IEnumerable<TNode> possibleNodes = GetContainingTriangle(vertex) ?? this.AsEnumerable();

            return possibleNodes.Where(n => n.Edges.Count >= 2).MinBy(n => n.Vertex.FastDistance(vertex))!;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        protected internal virtual TTriangle? GetContainingTriangle(TVertex vertex) => Triangles.FirstOrDefault(t =>
            t.Contains<TNode, TEdge, TVertex>(vertex));

        public IEnumerator<TNode> GetEnumerator() => Triangles.SelectMany(t => t).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual bool IsWall(IPoint point)
        {
            var offsetLoc = ApplyOffset(point);

            return PointMap[Convert.ToInt32(offsetLoc.X), Convert.ToInt32(offsetLoc.Y)].HasFlag(PointType.Wall);
        }

        public virtual IPoint RemoveOffset(IPoint point) => new Point(point.X - XOffset, point.Y - YOffset);
    }
}