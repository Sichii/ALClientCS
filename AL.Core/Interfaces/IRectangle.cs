using Newtonsoft.Json;

namespace AL.Core.Interfaces
{
    /// <summary>
    ///     Represents a rectangle.
    /// </summary>
    /// <seealso cref="IPolygon"/>
    /// <seealso cref="AL.Core.Interfaces.IPoint" />
    [JsonObject]
    public interface IRectangle : IPolygon, IPoint
    {
        public float Bottom { get; }
        public float Height { get; }
        public float Left { get; }
        public float Right { get; }
        public float Top { get; }
        public float Width { get; }
    }
}