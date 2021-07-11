namespace AL.Core.Interfaces
{
    /// <summary>
    ///     Represents a rectangle.
    /// </summary>
    /// <seealso cref="AL.Core.Interfaces.IPoint" />
    public interface IRectangle : IPoint
    {
        public float Bottom { get; }
        public float Height { get; }
        public float Left { get; }
        public float Right { get; }
        public float Top { get; }
        public IPoint[] Vertices { get; }
        public float Width { get; }
    }
}