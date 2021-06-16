using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Extensions;
using AL.Data.Maps;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;
using AL.Pathfinding.Objects;
using Common.Logging;

namespace AL.Pathfinding
{
    public class WorldMesh : GraphBase<GraphNode<Map>, Map>
    {
        private readonly Dictionary<Map, GraphNode<Map>> NodeLookup;
        protected sealed override ILog Logger { get; init; }

        public WorldMesh(Dictionary<Map, GraphNode<Map>> nodes)
            : base(nodes.Values.ToList(), (_, _) => 1,
                (node, _) => node.Edge.Irregular ? ConnectorType.Leave : ConnectorType.Transport)
        {
            NodeLookup = nodes;
            Logger = LogManager.GetLogger<WorldMesh>();
            Reset();
        }

        public IAsyncEnumerable<IConnector<Map>> FindRoute(Map start, IEnumerable<Map> ends)
        {
            if(!NodeLookup.TryGetValue(start, out var startNode))
                throw new InvalidOperationException($"Unable to locate a start node for the given map. ({start.Key})");

            var endMaps = ends.ToArray();
            var endNodes = NodeLookup.TryGetValues(endMaps).ToArray();

            if (endNodes.Length == 0)
                throw new InvalidOperationException(
                    $"Unable to locate any end nodes for the given maps. {string.Join(',', endMaps.Select(map => map.Key))}");

            return Navigate(startNode.Index, endNodes.Select(node => node.Index));
        }
    }
}