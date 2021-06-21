using System;
using System.Collections.Generic;

namespace AL.Pathfinding.Interfaces
{
    public interface IGraphNode<T> : IEquatable<IGraphNode<T>>, IComparable<IGraphNode<T>>
    {
        bool Closed { get; set; }
        int? Parent { get; set; }
        T Edge { get; }
        int Index { get; }
        List<IGraphNode<T>> Neighbors { get; }
        float Priority { get; }
        void Reset();
    }
}