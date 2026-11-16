#region
using AL.Data;
using AL.Pathfinding;
using AL.Pathfinding.Model;
using AL.Visualizer.Extensions;
using SkiaSharp;
#endregion

namespace AL.Tests.Pathfinding.Tests;

public class ScratchBscorpTests : PathfindingTestBed
{
    private static int CalculateMinSpeed(Hop hop)
    {
        for (var speed = 1; speed <= HopBuilder.MAX_SPEED; speed++)
            if (HopRisk.IsSafe(hop.Penalties, hop.Length, speed))
                return speed;

        return int.MaxValue;
    }

    private static SKColor GetColor(int speed)
        => speed switch
        {
            <= 40  => SKColors.Lime,
            <= 60  => SKColors.Cyan,
            <= 90  => SKColors.Yellow,
            <= 200 => SKColors.Orange,
            _      => SKColors.Red
        };

    private static string FormatSpeed(int speed) => speed == int.MaxValue ? "x" : speed.ToString();

    private static async Task DrawMapAsync(NavMesh mesh, string outputDir)
    {
        var hops = HopBuilder.FindHops(mesh);
        var basePath = Path.Combine(Path.GetTempPath(), $"{mesh.Map}-base.png");

        await Visualizer.Visualizer
                        .CreateGridImage(mesh)
                        .DrawEdges(mesh, SKColors.DimGray)
                        .SaveAsPngAsync(basePath);

        using var bitmap = SKBitmap.Decode(basePath);
        using var canvas = new SKCanvas(bitmap);
        using var font = new SKFont(SKTypeface.Default, 26);
        using var fill = new SKPaint();
        using var outline = new SKPaint();

        fill.IsAntialias = true;
        outline.IsAntialias = true;
        outline.Style = SKPaintStyle.Stroke;
        outline.StrokeWidth = 5;
        outline.Color = SKColors.Black;

        //each hop is listed both ways; draw the pair once, labelled there/back
        foreach (var hop in hops.Where(hop => (hop.Takeoff.X, hop.Takeoff.Y).CompareTo((hop.Landing.X, hop.Landing.Y)) < 0))
        {
            var back = hops.First(other => (other.Takeoff == hop.Landing) && (other.Landing == hop.Takeoff));
            var forwardSpeed = CalculateMinSpeed(hop);
            var backSpeed = CalculateMinSpeed(back);
            var a = mesh.ToCanvas(hop.Takeoff);
            var b = mesh.ToCanvas(hop.Landing);

            fill.Style = SKPaintStyle.Stroke;
            fill.StrokeWidth = 4;
            fill.Color = GetColor(Math.Min(forwardSpeed, backSpeed));

            canvas.DrawLine(
                a.X,
                a.Y,
                b.X,
                b.Y,
                fill);

            var label = forwardSpeed == backSpeed ? FormatSpeed(forwardSpeed) : $"{FormatSpeed(forwardSpeed)}/{FormatSpeed(backSpeed)}";
            var x = (a.X + b.X) / 2f + 6f;
            var y = (a.Y + b.Y) / 2f - 6f;

            fill.Style = SKPaintStyle.Fill;
            fill.Color = SKColors.White;

            canvas.DrawText(
                label,
                x,
                y,
                font,
                outline);

            canvas.DrawText(
                label,
                x,
                y,
                font,
                fill);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        await File.WriteAllBytesAsync(Path.Combine(outputDir, $"{mesh.Map}.png"), data.ToArray());
        File.Delete(basePath);
    }

    [Test]
    public async Task DrawEveryMapsHops()
    {
        var outputDir = Path.Combine(Path.GetTempPath(), "hop-maps");
        Directory.CreateDirectory(outputDir);

        foreach (var map in GameData.Maps.Values.DistinctBy(map => map.Accessor))
            if (HopBuilder.AllowsHops(map) && Pathfinder.GetNavMesh(map.Accessor) is { } mesh)
                await DrawMapAsync(mesh, outputDir);

        Console.WriteLine($"images: {outputDir}");
    }
}