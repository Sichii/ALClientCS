using AL.Core.Definitions;
using AL.Core.Interfaces;

namespace AL.Core.Objects
{
    public record Exit : IPoint
    {
        public float X { get; }
        public float Y { get; }
        public string DestinationMap { get; }
        public int DestinationSpawnId { get; }
        public ExitType Type { get; }

        public Exit(IPoint point, string destinationMap, int destinationSpawnId, ExitType type)
        {
            X = point.X;
            Y = point.Y;
            DestinationMap = destinationMap;
            DestinationSpawnId = destinationSpawnId;
            Type = type;
        }
    }
}