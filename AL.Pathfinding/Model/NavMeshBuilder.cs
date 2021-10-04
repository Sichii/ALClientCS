using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data.Geometry;
using AL.Data.Maps;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Extensions;
using Chaos.Core.Extensions;
using Poly2Tri;
using Polygon = Poly2Tri.Polygon;

namespace AL.Pathfinding.Model
{
    /// <summary>
    ///     Provides the ability to build a mesh from a map's data.
    /// </summary>
    public class NavMeshBuilder
    {
        private readonly GGeometry Geometry;
        private readonly int Height;
        private readonly GMap Map;
        private readonly PointType[,] PointMap;
        private readonly int Width;
        private readonly int XOffset;
        private readonly int YOffset;

        /// <summary>
        ///     Initializes a new instances of the <see cref="NavMeshBuilder" /> class.
        /// </summary>
        /// <param name="map">A map's gamedata.</param>
        /// <param name="geometry">A maps gemometry data.</param>
        public NavMeshBuilder(GMap map, GGeometry geometry)
        {
            Map = map;
            Geometry = geometry;
            Width = Geometry.MaxX - Geometry.MinX;
            Height = Geometry.MaxY - Geometry.MinY;
            XOffset = Math.Abs(Geometry.MinX);
            YOffset = Math.Abs(Geometry.MinY);
            PointMap = new PointType[Width + 1, Height + 1];
        }

        /// <summary>
        ///     Builds a triangulated navmesh and pointmap from a map's data.
        /// </summary>
        /// <returns>
        ///     <see cref="NavMesh" /> <br />
        ///     A navmesh containing triangle data
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public NavMesh Build()
        {
            FillWalls();
            var polyVertices = new HashSet<Point>();

            foreach (var spawn in Map.Spawns)
                polyVertices.UnionWith(FloodFindVertices(spawn));

            var polySet = TracePolygons(polyVertices);
            P2T.Triangulate(polySet);

            var polyTriangles = polySet.Polygons.SelectMany(polygon => polygon.Triangles);

            var meshTriangles = polyTriangles.Select(triangle => triangle.ToGenericTriangle(Map.Accessor));

            return new NavMesh(Map.Accessor, meshTriangles, PointMap, XOffset, YOffset);
        }

        #region Polygons
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private PolygonSet TracePolygons(ICollection<Point> vertices)
        {
            var polygons = new List<Polygon>();

            while (vertices.Count > 0)
            {
                //get a polygon via floodfill
                var polyLine = FloodPolyLine(vertices).Select((point, i) => new PolygonPoint(point.X, point.Y, i)).ToArray();
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
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private int OctagonalFill(int ix, int iy, Stack<Point> stack)
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private IEnumerable<Point> FloodPolyLine(ICollection<Point> vertices)
        {
            //new points will always be on the top, allowing us to directly trace the inner poly line clockwise
            var stack = new Stack<Point>();

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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