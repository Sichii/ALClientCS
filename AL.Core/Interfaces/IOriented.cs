using AL.Core.Definitions;

namespace AL.Core.Interfaces
{
    /// <summary>
    ///     <inheritdoc cref="IPoint" /> (with directional information)
    /// </summary>
    public interface IOriented : IPoint
    {
        public Direction Direction { get; }
    }
}