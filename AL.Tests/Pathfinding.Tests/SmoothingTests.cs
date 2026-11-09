#region
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

public class SmoothingTests : PathfindingTestBed
{
    private static float CalculateLength(IEnumerable<GraphEdge> legs) => legs.Sum(edge => edge.Start.Location.Distance(edge.End.Location));

    /// <summary>
    ///     Routes a seeded set, half on one map and half across maps, with blink and with hops: every walk leg is clear by the
    ///     server's rule, goes somewhere and stops on walkable ground, and smoothing the route again neither lengthens it nor
    ///     moves a door, transport or recall.
    /// </summary>
    [Test]
    public void SmoothingNeverLengthensOrCrossesAWall()
    {
        const int ROUTE_COUNT = 500;

        var graph = Pathfinder.Graph!;
        var rng = new Random(5);

        var pairs = SamplePairs(rng, ROUTE_COUNT * 3 / 2)
            .ToList();

        PathOptions[] options =
        [
            PathOptions.Default,
            new()
            {
                BlinkCost = 400f
            },
            new()
            {
                UseHops = true,
                WalkSpeed = 200f
            }
        ];

        var failures = new List<string>();
        var routes = 0;
        var fallbacksBefore = DirectedGraph.FunnelFallbacks;

        for (var i = 0; i < ROUTE_COUNT; i++)
        {
            //half the routes stay on one map, half cross maps through doors, transports and recalls
            var half = ROUTE_COUNT / 2;
            var start = i < half ? pairs[i].Start : pairs[half + (i - half) * 2].Start;
            var endPoint = i < half ? pairs[i].End : pairs[half + (i - half) * 2 + 1].End;
            var end = new Destination(new Location(endPoint.Map, endPoint.X, endPoint.Y), i % 2 * 30f);
            List<GraphEdge> route;

            try
            {
                route = graph.FindPath(start, [end], options[i % options.Length]);
            } catch (InvalidOperationException)
            {
                continue;
            }

            routes++;

            var again = graph.EnhancePath(route);

            var fixedLegs = route.Where(edge => edge.Type != EdgeType.Walk)
                                 .ToList();

            var fixedAgain = again.Where(edge => edge.Type != EdgeType.Walk)
                                  .ToList();

            if (CalculateLength(again) > (CalculateLength(route) + 0.01f))
                failures.Add($"#{i}: smoothing again grew the route from {CalculateLength(route):F1} to {CalculateLength(again):F1}");

            if (fixedLegs.Count != fixedAgain.Count)
                failures.Add($"#{i}: {fixedLegs.Count} fixed legs became {fixedAgain.Count}");
            else

                //a recall leaves from wherever the walks before it started, so only its start may move
                for (var f = 0; f < fixedLegs.Count; f++)
                    if ((fixedLegs[f].Type != fixedAgain[f].Type)
                        || ((fixedLegs[f].Type != EdgeType.Town)
                            && !ILocation.Comparer.Equals(fixedLegs[f].Start.Location, fixedAgain[f].Start.Location))
                        || !ILocation.Comparer.Equals(fixedLegs[f].End.Location, fixedAgain[f].End.Location))
                        failures.Add($"#{i}: {fixedLegs[f].Type} leg {f} moved to {fixedAgain[f]}");

            for (var l = 0; l < route.Count; l++)
            {
                var leg = route[l];

                if (leg.Type != EdgeType.Walk)
                    continue;

                if ((route.Count > 1) && (leg.Start.Location.Distance(leg.End.Location) < 0.001f))
                    failures.Add($"#{i}: walk {leg} has no length");

                if (!Pathfinder.CanMove(leg.Start.Location, leg.End.Location))
                    failures.Add($"#{i}: walk {leg} crosses a wall");

                //a walk may stop on the destination itself, or on the exit a door leg leaves from
                var standsOnANode = (l == (route.Count - 1)) || ILocation.Comparer.Equals(leg.End.Location, route[l + 1].Start.Location);

                if (!standsOnANode && !Pathfinder.IsWalkable(leg.End.Location))
                    failures.Add($"#{i}: walk {leg} stops off walkable ground");
            }
        }

        Console.WriteLine($"SMOOTHING routes={routes} funnelFallbacks={DirectedGraph.FunnelFallbacks - fallbacksBefore}");

        failures.Should()
                .BeEmpty("{0}", string.Join("; ", failures.Take(10)));
    }
}