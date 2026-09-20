#region
using System.Text.Json;
using AL.Core.Json;
using AL.Data.Geometry;
#endregion

namespace AL.Data.Maps;

/// <summary>
///     The document a run of map_chunk frames joins into: the floors of one dungeon run, each an ordinary map record plus
///     its geometry, and a manifest of the floors the same run has not delivered yet. A floor's key is spelled from the
///     run id and its floor number, and the game's client refuses a floor whose key says otherwise; so does
///     <see cref="Parse" />.
/// </summary>
public sealed record GeneratedMapBundle
{
    /// <summary>The most floors one run carries.</summary>
    public const int MAX_FLOORS = 8;

    /// <summary>
    ///     The floors this bundle delivers, each with its geometry.
    /// </summary>
    public IReadOnlyList<GeneratedFloor> Floors { get; init; } = [];

    /// <summary>
    ///     Floors of the same run this bundle does not carry the geometry for: their map records only, so a door on a
    ///     delivered floor can name where it leads before that floor arrives.
    /// </summary>
    public IReadOnlyList<GeneratedFloor> Manifest { get; init; } = [];

    /// <summary>The run every floor here belongs to.</summary>
    public string Run { get; init; } = null!;

    private static void Check(string run, GeneratedFloor floor)
    {
        var generated = floor.Definition?.Generated;

        if (generated is null || (generated.Run != run) || (floor.Key != FloorKey(run, generated.Floor)))
            throw new InvalidOperationException($"Generated floor {floor.Key} does not belong to run {run}.");
    }

    /// <summary>The map key a generated floor is filed under.</summary>
    public static string FloorKey(string run, int floor) => $"zone_{run}_{floor}";

    /// <summary>
    ///     Deserializes the joined chunk text and checks every floor against the run.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     The text is not a bundle, carries no floors or too many, or names a floor under a key that does not spell its own
    ///     run and floor number.
    /// </exception>
    public static GeneratedMapBundle Parse(string json)
    {
        var bundle = JsonSerializer.Deserialize<GeneratedMapBundle>(json, ALJson.Options)
                     ?? throw new InvalidOperationException("Generated map bundle is not a JSON object.");

        if (string.IsNullOrEmpty(bundle.Run) || bundle.Floors.Count is < 1 or > MAX_FLOORS)
            throw new InvalidOperationException($"Generated map bundle for run {bundle.Run} carries {bundle.Floors.Count} floors.");

        foreach (var floor in bundle.Floors)
        {
            Check(bundle.Run, floor);

            if (floor.Geometry is null)
                throw new InvalidOperationException($"Generated floor {floor.Key} carries no geometry.");
        }

        foreach (var entry in bundle.Manifest)
            Check(bundle.Run, entry);

        return bundle;
    }
}

/// <summary>
///     One floor of a <see cref="GeneratedMapBundle" />.
/// </summary>
public sealed record GeneratedFloor
{
    /// <summary>The floor's map record.</summary>
    public GMap Definition { get; init; } = null!;

    /// <summary>
    ///     The floor's wall lines and bounds. Null on a manifest entry.
    /// </summary>
    public GGeometry? Geometry { get; init; }

    /// <summary>The map key the floor is filed under.</summary>
    public string Key { get; init; } = null!;
}