using System;
using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.Core.Geometry
{
    public record Rectangle : IRectangle
    {
        public float Bottom { get; }
        public float Height { get; }
        public float Left { get; }
        public string Map { get; }
        public float Right { get; }
        public float Top { get; }
        public IPoint[] Vertices { get; }
        public float Width { get; }
        public float X { get; }
        public float Y { get; }

        [JsonConstructor]
        public Rectangle(float x, float y, float width, float height, string map = null)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Map = map;

            Top = y - height / 2;
            Left = x - width / 2;
            Right = x + width / 2;
            Bottom = y + height / 2;

            Vertices = new IPoint[] { new Point(Top, Left), new Point(Top, Right), new Point(Bottom, Left), new Point(Bottom, Right) };
        }

        public Rectangle(float height, float width, ILocation center)
            : this(center.X, center.Y, width, height, center.Map) { }

        public Rectangle(IPoint pt1, IPoint pt2, string mapName = null)
            : this(Math.Abs(pt1.Y - pt2.Y), Math.Abs(pt1.X - pt2.X), new Location((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2, mapName)) { }
    }
}