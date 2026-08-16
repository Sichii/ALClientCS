#region
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Definitions;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     <inheritdoc cref="GraphBase{TMesh,TNode,TEdge}" />
/// </summary>
public sealed class DirectedGraph : GraphBase<NavMesh, GraphNode, GraphEdge>
{
    //the scan's resolution along a leg when hunting a tighter bend, and with it the shortest piece a leg can be
    //cut into - the first candidate sits one stride in, and a leg shorter than the stride yields no candidates at
    //all, so the stride doubles as the minimum leg guard and resolution stays consistent across the whole path
    private const float BEND_SCAN_STRIDE = 5f;

    //what a moved bend must actually save. below this the move constructs a node for ground a character covers in
    //a step, and accepting zero-saving moves would re-move the same bend every pass forever
    private const float MIN_BEND_SAVING = 1f;

    //moving one bend exposes slack at its neighbors, so the pass repeats - each accepted move shortens the path
    //by at least MIN_BEND_SAVING, so the first pass takes nearly everything and the cap only bounds the tail
    private const int MAX_TIGHTEN_PASSES = 3;

    public DirectedGraph(Dictionary<string, NavMesh> navMeshes)
        : base(navMeshes) { }

    private async IAsyncEnumerable<GraphEdge> EnhancePathAsync(IAsyncEnumerable<GraphEdge> edges)
    {
        var partitionedPath = new List<GraphEdge>();

        //for each edge in the path
        await foreach (var edge in edges)
            if (edge.Type != EdgeType.Walk) //flush the path (with smoothing) if we come across a non walk node
            {
                partitionedPath.Add(edge);

                foreach (var partitionedEdge in TightenBends(SmoothPath(partitionedPath)))
                    yield return partitionedEdge;

                partitionedPath.Clear();
            } else if (edge.End.Radius > 0) //find distance shortcuts up to this point if we come across a node with a radius
            {
                partitionedPath.Add(edge);

                //shortcuts before tightening, so the bends ahead of the cut tighten against the true final stop
                //rather than a door centre the cut then moves
                var smoothPath = FindDistanceShortcuts(SmoothPath(partitionedPath));

                foreach (var partitionedEdge in TightenBends(smoothPath))
                    yield return partitionedEdge;

                partitionedPath.Clear(); //discard everything up to this point
            } else
                partitionedPath.Add(edge);

        //yield anything left over (with smoothing)
        if (partitionedPath.Count > 0)
            foreach (var partitionedEdge in TightenBends(SmoothPath(partitionedPath)))
                yield return partitionedEdge;
    }

    private IEnumerable<GraphEdge> FindDistanceShortcuts(IEnumerable<GraphEdge> edges)
    {
        var edgesArr = edges.ToArray();
        var endEdge = edgesArr.Last();
        var end = endEdge.End;

        //the end will have a radius, create destinations from it. a door carries several circles rather than one,
        //because the server measures it as a box - so the walk can stop at whichever of them it reaches first, and
        //every one of them is a place the server accepts
        var destinations = end.ReachableFrom
                              ?.Select(circle => new Destination(new Location(end.Vertex.Map, circle), circle.Radius))
                              .ToArray()
                           ?? [new Destination(end.Vertex, end.Radius)];

        foreach (var edge in edgesArr)
        {
            var current = edge.Start.Vertex;

            //if the node isnt on the same map as the current node, there are no shortcuts to find
            if (!current.OnSameMapAs(end.Vertex))
            {
                yield return edge;

                continue;
            }

            //if any destination already contains this point, we can stop here
            var alreadyThere = false;

            foreach (var destination in destinations)
                if (destination.Contains(current))
                {
                    alreadyThere = true;

                    break;
                }

            if (alreadyThere)
                yield break;

            var navMesh = NavMeshes[current.Map];

            //the nearest point on any of the circles we can reach in a straight line
            Location? nearestStop = null;
            var nearestDistance = float.MaxValue;

            foreach (var destination in destinations)
            {
                var candidate = new Location(current.Map, destination.OffsetTowards(current, destination.Radius));
                var distance = current.Distance(candidate);

                if ((distance >= nearestDistance) || !navMesh.CanMove(current, candidate))
                    continue;

                nearestStop = candidate;
                nearestDistance = distance;
            }

            //if we can move to one, return a new connection and break the path
            if (nearestStop != null)
            {
                var endNode = navMesh.ConstructNode(nearestStop);

                yield return navMesh.ConstructEdge(edge.Start, endNode, EdgeType.Walk);

                yield break;
            }

            //if that didnt work, take the earliest point at which this edge crosses into any of them
            Point? entryPoint = null;
            var entryDistance = float.MaxValue;

            foreach (var destination in destinations)
            {
                var candidate = destination.CalculateIntersectionEntryPoint(edge);

                if (candidate == null)
                    continue;

                var distance = current.Distance(candidate);

                if (distance >= entryDistance)
                    continue;

                entryPoint = candidate;
                entryDistance = distance;
            }

            if (entryPoint != null)
            {
                var endLoc = new Location(navMesh.Map, entryPoint);
                var endNode = navMesh.ConstructNode(endLoc);

                yield return navMesh.ConstructEdge(edge.Start, endNode, EdgeType.Walk);

                yield break;
            }

            yield return edge;
        }
    }

    /// <inheritdoc cref="GraphBase{TMesh,TNode,TEdge}.FindPathAsync" />
    public async IAsyncEnumerable<GraphEdge> FindPathAsync<T>(
        ILocation start,
        IEnumerable<T> ends,
        bool useTownIfOptimal = true,
        float? walkSpeed = null)
        where T: ILocation, ICircle
    {
        ArgumentNullException.ThrowIfNull(start);

        ArgumentNullException.ThrowIfNull(ends);

        var endsArr = ends.ToArray();

        if (endsArr.Length == 0)
            yield break;

        //a short hop with a clear line needs no search. The dijkstra below is serialized process-wide, so every
        //character waits on every other one's path, and short hops are most of what gets asked for.
        //
        //Bounded by the town cost because towning is the only same-map alternative a clear straight line can lose to,
        //and any path using it costs at least that. The bound is the same speed-derived figure the search itself
        //prices the channel at, so the two cannot disagree - a faster character both reaches for the channel less
        //readily and takes this shortcut over a longer walk, which is the same statement twice. Doors and transports
        //are cheaper by heuristic - 50 against a few hundred - but a cross-map exit is filtered out by the same-map
        //test below, and a same-map one lands back at its own door, so neither competes with a walk that is already
        //the shortest one there is.
        //
        //One destination only, which is what keeps that argument sound. Against a set, the nearest same-map
        //candidate is not the cheapest answer - a destination one door away costs the transport heuristic of 50 and
        //beats a clear 350-unit walk - so a shortcut that committed to the near one would quietly return a worse
        //path than the search. It would also widen a smaller hole: skipping the search skips the throw for a
        //destination whose map has no mesh
        var townCost = walkSpeed is { } speed ? CONSTANTS.TownCost(speed) : CONSTANTS.NOMINAL_TOWN_COST;

        if ((endsArr.Length == 1) && NavMeshes.TryGetValue(start.Map, out var navMesh))
            foreach (var end in endsArr.Where(e => e.OnSameMapAs(start) && (start.Distance(e) < townCost)))
            {
                //stop at the near edge of the destination rather than its centre, the same shortcut
                //FindDistanceShortcuts applies to a searched path
                var target = end.OffsetTowards(start, end.Radius);

                //the offset clamps to the start when already inside the radius, so there is nothing to walk
                if (IPoint.Comparer.Equals(target, start))
                    yield break;

                if (!navMesh.CanMove(start, target))
                    continue;

                var startNode = navMesh.ConstructNode(start);
                var endNode = navMesh.ConstructNode(new Location(start.Map, target));

                yield return navMesh.ConstructEdge(startNode, endNode, EdgeType.Walk);

                yield break;
            }

        var path = base.FindPathAsync(start, endsArr.Cast<ILocation>(), useTownIfOptimal, walkSpeed);

        await foreach (var edge in EnhancePathAsync(path))
            yield return edge;
    }

    private IEnumerable<GraphEdge> SmoothPath(IReadOnlyList<GraphEdge> connectors)
    {
        if (connectors.Count == 0)
            yield break;

        var last = connectors[^1];

        //EnhancePathAsync flushes on the first non-walk edge, so only the last element can be one.
        //a town edge there outranks every walk before it - you can town from anywhere - so the whole
        //partition collapses into it without testing a single line of sight.
        //The replacement carries the nominal cost rather than this character's, and nothing reads it: the search that
        //chose this edge is over, and every consumer of a returned town edge reads its Type alone
        if (last.Type == EdgeType.Town)
        {
            var first = connectors[0];

            yield return first == last
                ? last
                : NavMeshes[first.Start.Vertex.Map]
                    .ConstructEdge(first.Start, last.End, EdgeType.Town);

            yield break;
        }

        for (var i = 0; i < connectors.Count; i++)
        {
            var current = connectors[i];

            //scan back from the end and take the first edge in a straight line. that lands on the same
            //furthest-reachable edge the forward scan found, but stops there - the forward scan kept the
            //last success without ever breaking, so it paid for every candidate it went on to discard
            for (var e = connectors.Count - 1; e > i; e--)
            {
                //only a walk may be collapsed. a door or transport carries its spawn index on Start.Vertex,
                //which a collapse replaces, and HandlePathConnectorAsync casts that vertex to an Exit
                if (connectors[e].Type != EdgeType.Walk)
                    continue;

                if (!CanMove(current.Start.Vertex, connectors[e].End.Vertex))
                    continue;

                i = e;

                break;
            }

            var bestNext = connectors[i];

            if (current == bestNext)
                yield return current;
            else
            {
                var navMesh = NavMeshes[current.Start.Vertex.Map];

                //keep the collapsed edge's own type - re-deriving it relabels a walk that lands on the
                //map's spawn as a town teleport
                yield return navMesh.ConstructEdge(current.Start, bestNext.End, bestNext.Type);
            }
        }
    }

    //SmoothPath only ever bends a path at graph vertices, and a mesh vertex rarely sits on the true corner - so a
    //smoothed path still overshoots each turn: partway along a leg, a straight line to a later corner is already
    //clear, but the walk goes the rest of the way to the bend first. the earliest point on the leg with such a
    //line is the optimal departure - by the triangle inequality every later point saves less - and from that
    //point the furthest visible corner is the optimal arrival, for the same reason SmoothPath takes the furthest
    //node it can see. both are knowable here at plan time, which is what makes mid-walk re-aiming unnecessary
    private List<GraphEdge> TightenBends(IEnumerable<GraphEdge> edges)
    {
        var path = edges.ToList();

        for (var pass = 0; pass < MAX_TIGHTEN_PASSES; pass++)
        {
            var changed = false;

            for (var i = 0; (i + 1) < path.Count; i++)
            {
                var incoming = path[i];

                //only a walk's endpoints may move or be skipped. every other type's endpoints are fixed points
                //in the world, and a door or transport carries its spawn index on Start.Vertex
                if (incoming.Type != EdgeType.Walk)
                    continue;

                //the run of consecutive walks this leg belongs to; its ends are the corners a cut may target
                var runEnd = i;

                while (((runEnd + 1) < path.Count) && (path[runEnd + 1].Type == EdgeType.Walk))
                    runEnd++;

                if (runEnd == i)
                    continue;

                var legStart = incoming.Start.Vertex;
                var bend = incoming.End.Vertex;
                var navMesh = NavMeshes[legStart.Map];

                //a previous move can open a straight line from this node across one or more corners; scan the
                //furthest corner first, the same direction SmoothPath scans, and collapse everything it clears
                var collapsed = false;

                for (var j = runEnd; j > i; j--)
                {
                    if (!navMesh.CanMove(legStart, path[j].End.Vertex))
                        continue;

                    path[i] = navMesh.ConstructEdge(incoming.Start, path[j].End, EdgeType.Walk);
                    path.RemoveRange(i + 1, j - i);
                    changed = true;
                    collapsed = true;

                    break;
                }

                if (collapsed)
                {
                    i--;

                    continue;
                }

                var legLength = legStart.Distance(bend);
                var moved = false;

                for (var offset = BEND_SCAN_STRIDE; !moved && (offset < legLength); offset += BEND_SCAN_STRIDE)
                {
                    var candidate = legStart.OffsetTowards(bend, offset);

                    //the point becomes the end of two emits, so it has to be ground the server lets a move stop
                    //on - water is not a wall - and both new legs have to rasterize clear, since a subsegment's
                    //raytrace can clip a cell the full leg's line missed
                    if (!navMesh.IsWalkable(candidate) || !navMesh.CanMove(legStart, candidate))
                        continue;

                    //furthest corner first again: from a fixed point the furthest visible corner saves the most,
                    //because the polyline it skips can only be longer than the straight line replacing it
                    for (var j = runEnd; j > i; j--)
                    {
                        var target = path[j].End.Vertex;

                        if (!navMesh.CanMove(candidate, target))
                            continue;

                        //what the cut replaces: the rest of this leg plus every leg through the skipped corners
                        var currentCost = legLength - offset;

                        for (var k = i + 1; k <= j; k++)
                            currentCost += path[k].Start.Vertex.Distance(path[k].End.Vertex);

                        var saving = currentCost - candidate.Distance(target);

                        if (saving > MIN_BEND_SAVING)
                        {
                            var bendNode = navMesh.ConstructNode(new Location(legStart.Map, candidate));

                            path[i] = navMesh.ConstructEdge(incoming.Start, bendNode, EdgeType.Walk);
                            path[j] = navMesh.ConstructEdge(bendNode, path[j].End, EdgeType.Walk);

                            if (j > (i + 1))
                                path.RemoveRange(i + 1, j - i - 1);

                            changed = true;
                            moved = true;
                        }

                        //deeper corners from this same point save less, but a later point may still see a corner
                        //this one cannot - so an unprofitable sighting ends the corner scan, not the leg scan
                        break;
                    }
                }
            }

            if (!changed)
                break;
        }

        return path;
    }
}