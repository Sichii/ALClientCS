using AL.Core.Definitions;

namespace AL.Core.Interfaces
{
    public interface IOriented : IPoint
    {
        public Direction Direction { get; }
    }
}