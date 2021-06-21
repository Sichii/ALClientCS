using AL.Core.Interfaces;

namespace AL.Core.Geometry
{
    public record BoundingRectangle : BoundingBase, IRectangle
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

        public BoundingRectangle(
            float x,
            float y,
            int halfWidth,
            int verticalNorth,
            int verticalNotNorth,
            string map = null)
            : base(halfWidth, verticalNorth, verticalNotNorth)
        {
            X = x;
            Y = y;
            Width = halfWidth * 2;
            Height = verticalNorth + verticalNotNorth;
            Map = map;

            Top = y - verticalNorth;
            Left = x - halfWidth;
            Right = x + halfWidth;
            Bottom = y + verticalNotNorth;

            Vertices = new IPoint[]
                { new Point(Top, Left), new Point(Top, Right), new Point(Bottom, Left), new Point(Bottom, Right) };
        }
    }
}