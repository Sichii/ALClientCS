using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data.Geometry;
using AL.Data.Maps;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Extensions;
using Poly2Tri;
using Polygon = Poly2Tri.Polygon;

namespace AL.Pathfinding.Model
{
    internal class NavMeshBuilder
    {
        private readonly GGeometry Geometry;
        private readonly int Height;
        private readonly GMap Map;
        private readonly Dictionary<Point, GraphNode<Point>> NodeDic;
        private readonly PointType[,] PointMap;
        private readonly Dictionary<Point, DelaunayTriangle> Triangles;
        private readonly int Width;
        private readonly int XOffset;
        private readonly int YOffset;
        private int Index;
        private GraphNode<Point>? TownNode;

        internal NavMeshBuilder(GMap map, GGeometry geometry)
        {
            Map = map;
            Geometry = geometry;
            Width = Geometry.MaxX - Geometry.MinX;
            Height = Geometry.MaxY - Geometry.MinY;
            XOffset = Math.Abs(Geometry.MinX);
            YOffset = Math.Abs(Geometry.MinY);
            NodeDic = new Dictionary<Point, GraphNode<Point>>();
            PointMap = new PointType[Width + 1, Height + 1];
            Triangles = new Dictionary<Point, DelaunayTriangle>();
        }

        internal NavMesh Build()
        {
            FillWalls();
            var vertices = new HashSet<Point>();

            foreach (var spawn in Map.Spawns)
                vertices.UnionWith(FloodFindVertices(spawn));

            var polySet = TracePolygons(vertices);
            P2T.Triangulate(polySet);

            foreach (var triangle in polySet.Polygons.SelectMany(polygon => polygon.Triangles))
                Triangles.Add(TriangleNetExtensions.Centroid(triangle), triangle);

            foreach (var triangle in Triangles.Values)
            {
                var vertex1 = triangle.Points._0.ToPoint();
                var vertex2 = triangle.Points._1.ToPoint();
                var vertex3 = triangle.Points._2.ToPoint();

                if (!NodeDic.TryGetValue(vertex1, out var node1))
                {
                    node1 = new GraphNode<Point>(vertex1, Index++);
                    NodeDic[vertex1] = node1;
                }

                if (!NodeDic.TryGetValue(vertex2, out var node2))
                {
                    node2 = new GraphNode<Point>(vertex2, Index++);
                    NodeDic[vertex2] = node2;
                }

                if (!NodeDic.TryGetValue(vertex3, out var node3))
                {
                    node3 = new GraphNode<Point>(vertex3, Index++);
                    NodeDic[vertex3] = node3;
                }

                node1.Neighbors.Add(node2);
                node1.Neighbors.Add(node3);
                node2.Neighbors.Add(node1);
                node2.Neighbors.Add(node3);
                node3.Neighbors.Add(node1);
                node3.Neighbors.Add(node2);
            }

            TownNode = CreateTownNode();

            if (TownNode != null)
                NodeDic[TownNode.Edge] = TownNode;

            //ensure unique neighbors
            foreach (var node in NodeDic.Values)
                node.Neighbors = node.Neighbors.Distinct().ToList();

            var context = new NavMeshBuilderContext
            {
                Nodes = NodeDic.Values.ToList(),
                Triangles = Triangles,
                TownNode = TownNode,
                XOffset = XOffset,
                YOffset = YOffset,
                PointMap = PointMap
            };

            return new NavMesh(context);
        }

        #region Utility

        private GraphNode<Point>? CreateTownNode()
        {
            var spawn = Map.Spawns.Count > 0 ? Map.Spawns[0] : default;

            if ((spawn == null) || Map.Boundless)
                return null;

            var spawnPoint = new Point(spawn.X + XOffset, spawn.Y + YOffset);
            var node = new GraphNode<Point>(spawnPoint, Index++);
            var townTriangle = Triangles.OrderBy(kvp => kvp.Key.Distance(spawnPoint))
                .Select(kvp => kvp.Value)
                .FirstOrDefault(triangle => triangle.ContainsPoint(spawnPoint.X, spawnPoint.Y));

            if (townTriangle == null)
                return null;

            foreach (var vertex in townTriangle.Points)
            {
                var point = vertex.ToPoint();

                if (spawnPoint.Equals(point))
                    continue;

                node.Neighbors.Add(NodeDic[point]);
            }

            NodeDic.Add(node.Edge, node);
            return node;
        }

        #endregion

        #region Polygons

        private void FillWalls()
        {
            //for each point in each wall rect, set that point to a wall in the grid
            foreach (var rect in PadWalls(CONSTANTS.DEFAULT_BOUNDING_BASE))
                foreach ((var x, var y) in rect.Points())
                    PointMap[Convert.ToInt32(x), Convert.ToInt32(y)] = PointType.Wall;

            //for any walls that are almost touching
            //fill in the space between with wall points
            //this is to avoid problems where 2 different polygons' vertices/inline points are touching
            for (var x = 0; x <= Width; x++)
                for (var y = 0; y <= Height; y++)
                    if ((y <= Height - 4)
                        && (PointMap[x, y] == PointType.Wall)
                        && (PointMap[x, y + 1] == PointType.None)
                        && (PointMap[x, y + 2] == PointType.None)
                        && (PointMap[x, y + 3] == PointType.Wall))
                    {
                        PointMap[x, y + 1] = PointType.Wall;
                        PointMap[x, y + 2] = PointType.Wall;
                    }

            for (var y = 0; y <= Height; y++)
                for (var x = 0; x <= Width; x++)
                    if ((x <= Width - 4)
                        && (PointMap[x, y] == PointType.Wall)
                        && (PointMap[x + 1, y] == PointType.None)
                        && (PointMap[x + 2, y] == PointType.None)
                        && (PointMap[x + 3, y] == PointType.Wall))
                    {
                        PointMap[x + 1, y] = PointType.Wall;
                        PointMap[x + 2, y] = PointType.Wall;
                    }
        }

        private IEnumerable<IRectangle> PadWalls(BoundingBase boundingBase)
        {
            (var halfWidth, var verticalNorth, var verticalNotNorth) = boundingBase;

            //for each line, pad based on half of the character bounding base
            //yield a rectangle after padding each line
            foreach (var line in Geometry.VerticalLines)
            {
                var x = line.Point1.X + XOffset;
                var startY = line.Point1.Y + YOffset;
                var endY = line.Point2.Y + YOffset;

                if ((0 > x) || (x > Width) || (0 > startY) || (startY > Height) || (0 > endY) || (endY > Height))
                    continue;

                //pad sides with half of the width of the bounding base
                //pad the top with the bottom part of the bounding base (characters will walk DOWN into the top of the wall)
                //pad the bottom with the top part of the bounding base (characters will walk UP into the bottom of the wall)
                var tl = new Point(Math.Max(x - halfWidth, 0), Math.Max(startY - verticalNotNorth, 0));
                var br = new Point(Math.Min(x + halfWidth, Width), Math.Min(endY + verticalNorth, Height));
                yield return new Rectangle(tl, br);
            }

            foreach (var line in Geometry.HorizontalLines)
            {
                var y = line.Point1.Y + YOffset;
                var startX = line.Point1.X + XOffset;
                var endX = line.Point2.X + XOffset;

                if ((0 > y) || (y > Height) || (0 > startX) || (startX > Width) || (0 > endX) || (endX > Width))
                    continue;

                var tl = new Point(Math.Max(startX - halfWidth, 0), Math.Max(y - verticalNotNorth, 0));
                var br = new Point(Math.Min(endX + halfWidth, Width), Math.Min(y + verticalNorth, Height));
                yield return new Rectangle(tl, br);
            }
        }

        private PolygonSet TracePolygons(HashSet<Point> vertices)
        {
            var polygons = new List<Polygon>();

            while (vertices.Count > 0)
            {
                //get a polygon via floodfill
                var polyLine = FloodPolyLine(vertices).Select((point, i) => point.ToPolyPoint(i)).ToArray();
                var polygon = new Polygon(polyLine);
                var isHole = false;

                //for each existing polygon
                foreach (var existing in polygons.ToArray())
                    //check if the new polygon is inside of the existing polygon
                    if (existing.ContainsPoint((float)polyLine[0].X, (float)polyLine[0].Y))
                    {
                        existing.AddHole(polygon);
                        isHole = true;
                        break;
                        //check if any existing polygons are inside of the new polygon    
                    } else if (polygon.ContainsPoint((float)existing.Points[0].X, (float)existing.Points[0].Y))
                    {
                        polygons.Remove(existing);
                        polygon.AddHole(existing);
                        break;
                    }

                //if this polygon wasnt inside of another, add it
                if (!isHole)
                    polygons.Add(polygon);
            }

            var polySet = new PolygonSet();

            foreach (var polygon in polygons)
                polySet.Add(polygon);

            return polySet;
        }

        #endregion

        #region Flood

        private IEnumerable<Point> FloodFindVertices(IPoint start)
        {
            var x = Convert.ToInt32(start.X + XOffset);
            var y = Convert.ToInt32(start.Y + YOffset);

            if ((x < 0) || (x > Width))
                yield break;

            if ((y < 0) || (y > Height))
                yield break;

            if (PointMap[x, y] != PointType.None)
                yield break;

            var stack = new Stack<Point>(Width * Height / 5);

            PointMap[x, y] = PointType.Walkable;
            stack.Push(new Point(x, y));

            while (stack.Count > 0)
            {
                var curr = stack.Pop();
                (var fx, var fy) = curr;

                var ix = Convert.ToInt32(fx);
                var iy = Convert.ToInt32(fy);
                var signature = OctagonalFill(ix, iy, stack);

                if (MinesweeperLogic(ix, iy, signature))
                    yield return curr;
            }
        }

        private int OctagonalFill(int ix, int iy, Stack<Point> stack)
        {
            void FillIndex(int x, int y, ref int signature)
            {
                var val = PointMap[x, y];

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (val == PointType.Wall)
                    signature <<= 1;
                else if (val == PointType.None)
                {
                    PointMap[x, y] = PointType.Walkable;
                    stack.Push(new Point(x, y));
                }
            }

            var signature = 1;
            // Left
            ix--;
            if (ix >= 0)
                FillIndex(ix, iy, ref signature);

            // Top Left
            iy -= 1;
            if ((ix >= 0) && (iy >= 0))
                FillIndex(ix, iy, ref signature);

            // Top
            ix += 1;
            if (iy >= 0)
                FillIndex(ix, iy, ref signature);

            // Top Right
            ix += 1;
            if ((ix <= Width) && (iy >= 0))
                FillIndex(ix, iy, ref signature);

            // Right
            iy += 1;
            if (ix <= Width)
                FillIndex(ix, iy, ref signature);

            // Bottom Right
            iy += 1;
            if ((iy <= Height) && (ix <= Width))
                FillIndex(ix, iy, ref signature);

            // Bottom
            ix -= 1;
            if (iy <= Height)
                FillIndex(ix, iy, ref signature);

            // Bottom Left
            ix -= 1;
            if ((ix >= 0) && (iy <= Height))
                FillIndex(ix, iy, ref signature);

            return signature;
        }

        private bool MinesweeperLogic(int cx, int cy, int signature)
        {
            switch (signature)
            {
                case 1:
                    return false;
                case 1 << 1: //1 wall next to it
                    PointMap[cx, cy] = PointType.Vertex;
                    return true;
                case 1 << 2: //2 walls next to it
                    PointMap[cx, cy] = PointType.Inline;
                    return false;
                case 1 << 3: //3 walls next to it
                    PointMap[cx, cy] = PointType.Inline;
                    return false;
                case 1 << 4: //4 walls next to it
                    PointMap[cx, cy] = PointType.Vertex;
                    return true;
                case 1 << 5: //5 walls next to it
                    PointMap[cx, cy] = PointType.Vertex;
                    return true;
                default:
                    throw new Exception("Found a point with an unexpected number of walls around it.");
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private IEnumerable<Point> FloodPolyLine(HashSet<Point> vertices)
        {
            //new points will always be on the top, allowing us to directly trace the inner poly line
            var stack = new Stack<Point>();

            bool TryDiscoverVertex(int x, int y, out Point point)
            {
                point = Point.None;
                var type = PointMap[x, y];

                //if the point is inline and undiscovered
                if (type.HasFlag(PointType.Inline) && !type.HasFlag(PointType.Discovered))
                {
                    point = new Point(x, y);
                    stack.Push(point);
                    //set discovered flag on the point
                    PointMap[x, y] |= PointType.Discovered;

                    //if this point is a vertex, remove it from undiscovered vertices
                    if (type.HasFlag(PointType.Vertex))
                    {
                        vertices.Remove(point);
                        return true;
                    }
                }

                return false;
            }

            (var sx, var sy) = vertices.First();
            if (TryDiscoverVertex(Convert.ToInt32(sx), Convert.ToInt32(sy), out var vertex))
                yield return vertex;

            while (stack.Count > 0)
            {
                (var fx, var fy) = stack.Pop();
                var ix = Convert.ToInt32(fx);
                var iy = Convert.ToInt32(fy);

                // Left
                var cx = ix - 1;
                var cy = iy;
                if ((cx >= 0) && TryDiscoverVertex(cx, cy, out vertex))
                    yield return vertex;

                // Top
                cx = ix;
                cy = iy - 1;
                if ((cy >= 0) && TryDiscoverVertex(cx, cy, out vertex))
                    yield return vertex;

                // Right
                cx = ix + 1;
                cy = iy;
                if ((cx <= Width) && TryDiscoverVertex(cx, cy, out vertex))
                    yield return vertex;

                // Bot
                cx = ix;
                cy = iy + 1;
                if ((cy <= Height) && TryDiscoverVertex(cx, cy, out vertex))
                    yield return vertex;
            }
        }

        #endregion
    }
}