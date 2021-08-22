using System;
using System.Collections.Generic;
using AL.Pathfinding.Interfaces;

namespace AL.Pathfinding.Comparers
{
    public sealed class GraphNodeComparer<T> : IEqualityComparer<IGraphNode<T>> where T: IEquatable<T>
    {
        public bool Equals(IGraphNode<T>? x, IGraphNode<T>? y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null))
                return false;

            return !ReferenceEquals(y, null) && x.Edge.Equals(y.Edge);
        }

        public int GetHashCode(IGraphNode<T> obj) => obj.Edge.GetHashCode();
    }
}