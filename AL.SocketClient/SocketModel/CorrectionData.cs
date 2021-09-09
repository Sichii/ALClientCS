using AL.Core.Interfaces;

namespace AL.SocketClient.SocketModel
{
    public record CorrectionData : IPoint
    {
        public float X { get; init; }
        public float Y { get; init; }
        public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);
    }
}