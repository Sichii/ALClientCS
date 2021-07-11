using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Pathfinding.Interfaces;

namespace AL.Pathfinding.Extensions
{
    public static class ConnectorExtensions
    {
        public static ILine ToLine<T>(this IConnector<T> pointConnector) where T: IPoint =>
            new Line(pointConnector.Start, pointConnector.End);
    }
}