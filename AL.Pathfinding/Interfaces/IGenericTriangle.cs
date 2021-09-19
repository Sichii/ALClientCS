using System;
using System.Collections.Generic;

namespace AL.Pathfinding.Interfaces
{
    /// <summary>
    ///     Represents a triangle of some things you can make a triangle out of.
    /// </summary>
    /// <typeparam name="TVertex">Something you can make a triangle out of.</typeparam>
    public interface IGenericTriangle<TVertex> : IEnumerable<TVertex>, IEquatable<IGenericTriangle<TVertex>>
    {
        /// <summary>
        ///     The 3 vertices of this triangle.
        /// </summary>
        IReadOnlyList<TVertex> Vertices { get; }
    }
}