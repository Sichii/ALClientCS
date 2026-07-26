#region
using AL.Core.Definitions;
#endregion

namespace AL.Core.Interfaces;

/// <summary>
///     <inheritdoc cref="IPoint" /> (with directional information)
/// </summary>
public interface IOriented : IPoint
{
    public Direction Direction { get; }
}