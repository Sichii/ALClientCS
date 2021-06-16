using System.Text.Json.Serialization;
using AL.Core.Extensions;
using AL.Core.Interfaces;

namespace AL.Core.Geometry
{
    public record Line : ILine
    {
        public IPoint Point1 { get; init; }
        public IPoint Point2 { get; init; }
        
        [JsonIgnore]
        public float Length => Point1.Distance(Point2);

        public Line() { }

        public Line(IPoint point1, IPoint point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public void Deconstruct(out IPoint point1, out IPoint point2)
        {
            point1 = Point1;
            point2 = Point2;
        }
    }
}