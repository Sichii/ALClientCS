using System;
using System.Collections.Generic;
using AL.Core.Interfaces;

namespace AL.Pathfinding.Interfaces
{
    public interface IGraphNode2<TEdge> : IEquatable<IGraphNode2<TEdge>>
    {
        bool Closed { get; set; }

        ICollection<TEdge> Edges { get; init; }

        TEdge? Parent { get; set; }

        ILocation Vertex { get; init; }

        float Priority { get; }

        void Reset();
    }
}