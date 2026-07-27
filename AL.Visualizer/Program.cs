#region
using System.Globalization;
using AL.APIClient;
using AL.Core.Geometry;
using AL.Data;
using AL.Pathfinding;
using AL.Pathfinding.Model;
using AL.Visualizer;
using AL.Visualizer.Extensions;
using Chaos.Extensions.Common;
using SixLabors.ImageSharp;
#endregion

const string USAGE = """
                     usage:
                       AL.Visualizer dump-maps [outputDir]
                       AL.Visualizer draw-path [startMap startX startY endMap endX endY] [outputDir]

                     outputDir defaults to "images" beside the executable.
                     draw-path defaults to main -1582 496 -> spookytown 0 0.
                     """;

if (args.Length == 0)
{
    Console.WriteLine(USAGE);

    return 1;
}

var dumpMaps = args[0]
    .EqualsI("dump-maps");

var drawPath = args[0]
    .EqualsI("draw-path");

//dump-maps takes an optional outputDir; draw-path takes an optional 6-part endpoint pair before it
var argCountValid = dumpMaps ? args.Length is 1 or 2 : args.Length is 1 or 2 or 7 or 8;

if ((!dumpMaps && !drawPath) || !argCountValid)
{
    Console.WriteLine(USAGE);

    return 1;
}

var hasEndpoints = drawPath && (args.Length >= 7);
var outputDirIndex = hasEndpoints ? 7 : 1;

var outputDir = args.Length > outputDirIndex ? args[outputDirIndex] : Path.Combine(AppContext.BaseDirectory, "images");

var start = new Location("main", -1582, 496);
var end = new Location("spookytown", 0, 0);

if (hasEndpoints)
{
    if (!float.TryParse(args[2], CultureInfo.InvariantCulture, out var startX)
        || !float.TryParse(args[3], CultureInfo.InvariantCulture, out var startY)
        || !float.TryParse(args[5], CultureInfo.InvariantCulture, out var endX)
        || !float.TryParse(args[6], CultureInfo.InvariantCulture, out var endY))
    {
        Console.WriteLine("Coordinates must be numbers.");

        return 1;
    }

    start = new Location(args[1], startX, startY);
    end = new Location(args[4], endX, endY);
}

Directory.CreateDirectory(outputDir);

Console.WriteLine("Loading game data...");
GameData.Populate(await AlApiClient.GetGameDataAsync());

Console.WriteLine("Building navmeshes...");
Pathfinder.Initialize();

if (dumpMaps)
    foreach (var map in GameData.Maps.Values.DistinctBy(map => map.Accessor))
    {
        var navMesh = Pathfinder.GetNavMesh(map.Accessor);

        if (navMesh == null)
            continue;

        using var image = Visualizer.CreateGridImage(navMesh)
                                    .DrawEdges(navMesh);

        await image.SaveAsync(Path.Combine(outputDir, $"{map.Accessor}.png"));
    }
else
{
    var path = Pathfinder.FindPathAsync(start, [new Destination(end, 0)]);

    var images = Visualizer.DrawPath<DirectedGraph, NavMesh, GraphNode, GraphEdge>(Pathfinder.DirectedGraph, path);

    var counter = 1;

    await foreach (var image in images)
        using (image)
            await image.SaveAsync(Path.Combine(outputDir, $"img{counter++}.png"));
}

Console.WriteLine($"Wrote images to {outputDir}");

return 0;