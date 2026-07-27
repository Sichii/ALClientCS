#region
using AL.Core.Comparers;
#endregion

namespace AL.Core.Interfaces;

/// <summary>
///     Represents a location on a map.
/// </summary>
public interface ILocation : IPoint, IEquatable<ILocation>
{
    static new IEqualityComparer<ILocation> Comparer { get; } = new LocationEqualityComparer();

    /// <summary>
    ///     The map this object is located in.
    /// </summary>
    string Map { get; }

    static string ToString(ILocation location) => $"{location.Map}:{IPoint.ToString(location)}";
}