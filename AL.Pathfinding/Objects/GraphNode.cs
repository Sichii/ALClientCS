using System.Collections.Generic;
using AL.Pathfinding.Interfaces;
using Priority_Queue;

namespace AL.Pathfinding.Objects
{
    public class GraphNode<T> : FastPriorityQueueNode, IGraphNode<T>
    {
        public bool Closed { get; set; }
        public int Index { get; }
        public int? Parent { get; set; }
        public List<IGraphNode<T>> Neighbors { get; internal set; }
        public T Edge { get; }
        
        public GraphNode(T edge, int index)
        {
            Index = index;
            Edge = edge;
            Neighbors = new List<IGraphNode<T>>();
        }
        
        public void Reset()
        {
            Closed = false;
            Parent = null;
        }
        
        public bool Equals(IGraphNode<T> other) => other != null && Equals(Edge, other.Edge);
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return obj.GetType() == GetType() && Equals((IGraphNode<T>) obj);

        }

        public override int GetHashCode() => Edge.GetHashCode();

        public int CompareTo(IGraphNode<T> other)
        {
            if (ReferenceEquals(this, other))
                return 0;

            return ReferenceEquals(null, other) ? 1 : Priority.CompareTo(other.Priority);
        }
    }
}