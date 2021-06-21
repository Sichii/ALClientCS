using System;
using AL.Pathfinding.Definitions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AL.Visualizer
{
    public static class Visualizer
    {
        public static Image<Rgba32> CreateGridImage(PointType[,] pointMap)
        {
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

                    throw new ArgumentOutOfRangeException($"Unknown point type {(int) type}");
            }
        }
    }
}