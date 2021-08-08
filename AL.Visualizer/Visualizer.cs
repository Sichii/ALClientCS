using System;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
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
        ///     Creates a basic image of the map for a <see cref="NavMesh" />.
        /// </summary>
        /// <param name="navMesh">The navmesh to create an image for.</param>
        /// <returns>
        ///     <see cref="Image{TPixel}" /> <br />
        ///     An image representing the map. It's not exact (it doesnt use tiles), but it gives you a useable 1 to 1
        ///     visualization. <br />
        ///     You can use <see cref="Extensions.ImageExtensions" /> to layer on more information about the <see cref="NavMesh" />
        ///     .
        /// </returns>
        /// <exception cref="ArgumentNullException">navMesh</exception>
        public static Image<Rgba32> CreateGridImage(NavMesh navMesh)
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