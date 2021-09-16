using System;
using System.Collections.Generic;

namespace AL.Pathfinding.Interfaces
{
    public interface IGenericTriangle<TVertex> : IEnumerable<TVertex>, IEquatable<IGenericTriangle<TVertex>>
    {
        IReadOnlyList<TVertex> Vertices { get; }
    }
}