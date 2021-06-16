using System;
using System.Collections.Generic;

namespace AL.Pathfinding.Interfaces
{
    public interface IGraphNode<T> : IEquatable<IGraphNode<T>>, IComparable<IGraphNode<T>>
    {
        int Index { get; }
        int? Parent { get; set; }
        float Priority { get; }
        bool Closed { get; set; }
        List<IGraphNode<T>> Neighbors { get; }
        T Edge { get; }
        void Reset();
    }
}