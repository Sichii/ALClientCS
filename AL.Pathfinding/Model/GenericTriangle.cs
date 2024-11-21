#region
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Interfaces;
using AL.Pathfinding.Interfaces;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     A triangle whose vertices are locations.
/// </summary>
public sealed class GenericTriangle : IGenericTriangle<ILocation>
{
    public IReadOnlyList<ILocation> Vertices { get; init; } = new List<ILocation>();

    public bool Equals(IGenericTriangle<ILocation>? other) => other is not null && Vertices.SequenceEqual(other.Vertices);
    public IEnumerator<ILocation> GetEnumerator() => Vertices.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}