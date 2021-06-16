namespace AL.Core.Interfaces
{
    public interface IRectangle : IPoint
    {
        public float Bottom { get; }
        public float Height { get; }
        public float Left { get; }
        public string Map { get; }
        public float Right { get; }
        public float Top { get; }
        public IPoint[] Vertices { get; }
        public float Width { get; }
    }
}