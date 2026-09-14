#region
using System.Text.Json.Nodes;
#endregion

namespace AL.MemberGenerator;

/// <summary>
///     The previous fetches of G, kept under <c>snapshots/</c> in the working directory so a fresh payload can be diffed
///     against the one before it. The member files say which keys moved; only the earlier payload can say which values
///     moved behind the keys that stayed.
/// </summary>
/// <remarks>
///     The baseline is the newest snapshot whose version is below the fresh payload's, so running twice on one payload
///     prints the same diff. Only the two newest are kept. Presentation sections get a count and no lines: nobody plays
///     against a sprite offset.
/// </remarks>
public static class Snapshots
{
    public const string FOLDER_NAME = "snapshots";
    public const int KEEP = 2;

    //detail lines printed per section before the rest is folded into a count
    private const int DETAIL_CAP = 60;
    private const int VALUE_WIDTH = 72;

    //presentation data: a count per section is all the report wants
    private static readonly HashSet<string> Quiet = new(StringComparer.OrdinalIgnoreCase)
    {
        "sprites",
        "imagesets",
        "images",
        "tilesets",
        "animations",
        "docs",
        "positions",
        "geometry",
        "cosmetics",
        "emotions"
    };

    /// <summary>
    ///     The newest snapshot below <paramref name="version" />, or null on a first run.
    /// </summary>
    public static string? BaselineFor(int version)
        => Kept()
           .Where(snapshot => snapshot.Version < version)
           .OrderByDescending(snapshot => snapshot.Version)
           .Select(snapshot => snapshot.Path)
           .FirstOrDefault();

    /// <summary>
    ///     What a refresh reports about the values: a header per section that moved with its counts, the moved entries
    ///     beneath it as <c>entry.path: old -> new</c>, and a closing summary line.
    /// </summary>
    public static IReadOnlyList<string> Diff(JsonObject old, JsonObject fresh)
    {
        var lines = new List<string>();
        var summary = new List<string>();

        var sections = old.Select(pair => pair.Key)
                          .Union(fresh.Select(pair => pair.Key))
                          .Where(section => section != "version")
                          .Order(StringComparer.Ordinal);

        foreach (var section in sections)
        {
            //a section that is a list or a scalar (levels, inflation) is one entry named after itself
            var bothObjects = old[section] is JsonObject && fresh[section] is JsonObject;
            var oldEntries = bothObjects ? (JsonObject)old[section]! : Single(section, old[section]);
            var newEntries = bothObjects ? (JsonObject)fresh[section]! : Single(section, fresh[section]);

            var added = newEntries.Keys
                                  .Except(oldEntries.Keys)
                                  .Order(StringComparer.Ordinal)
                                  .ToList();

            var removed = oldEntries.Keys
                                    .Except(newEntries.Keys)
                                    .Order(StringComparer.Ordinal)
                                    .ToList();

            var changed = new List<(string Name, List<(string Path, JsonNode? Before, JsonNode? After)> Leaves)>();

            foreach (var name in oldEntries.Keys
                                           .Intersect(newEntries.Keys)
                                           .Order(StringComparer.Ordinal))
            {
                var leaves = new List<(string Path, JsonNode? Before, JsonNode? After)>();
                Walk(oldEntries[name], newEntries[name], string.Empty, leaves);

                if (leaves.Count > 0)
                    changed.Add((name, leaves));
            }

            if ((added.Count == 0) && (removed.Count == 0) && (changed.Count == 0))
                continue;

            (int Count, string Word)[] parts = [(changed.Count, "changed"), (added.Count, "added"), (removed.Count, "removed")];

            var counts = string.Join(
                ", ",
                parts.Where(part => part.Count > 0)
                     .Select(part => $"{part.Count} {part.Word}"));

            summary.Add($"{section} {counts}");
            lines.Add($"== {section}: {counts}");

            if (Quiet.Contains(section))
                continue;

            var detail = added.Select(name => $"  + {name}")
                              .Concat(removed.Select(name => $"  - {name}"))
                              .ToList();

            foreach ((var name, var leaves) in changed)
                foreach ((var path, var before, var after) in leaves)
                {
                    var where = (path.Length == 0) || (path[0] == '[') ? $"{name}{path}" : $"{name}.{path}";

                    detail.Add(
                        before is null ? $"  {where}: (new) {Scalar(after)}"
                        : after is null ? $"  {where}: {Scalar(before)} (gone)"
                                          : $"  {where}: {Scalar(before)} -> {Scalar(after)}");
                }

            lines.AddRange(detail.Take(DETAIL_CAP));

            if (detail.Count > DETAIL_CAP)
                lines.Add($"  ... {detail.Count - DETAIL_CAP} more");
        }

        lines.Add(string.Empty);
        lines.Add("summary: " + (summary.Count > 0 ? string.Join("; ", summary) : "no value changes"));

        return lines;
    }

    /// <summary>
    ///     Writes the payload as <c>G-{version}.json</c> and drops every snapshot but the newest <see cref="KEEP" />.
    /// </summary>
    public static async Task FileAwayAsync(string payload, int version)
    {
        Directory.CreateDirectory(FOLDER_NAME);
        await File.WriteAllTextAsync(Path.Combine(FOLDER_NAME, $"G-{version}.json"), payload);

        foreach (var stale in Kept()
                              .OrderByDescending(snapshot => snapshot.Version)
                              .Skip(KEEP))
            File.Delete(stale.Path);
    }

    private static IEnumerable<(int Version, string Path)> Kept()
    {
        if (!Directory.Exists(FOLDER_NAME))
            yield break;

        foreach (var path in Directory.EnumerateFiles(FOLDER_NAME, "G-*.json"))
            if (int.TryParse(
                    Path.GetFileNameWithoutExtension(path)
                        .AsSpan(2),
                    out var version))
                yield return (version, path);
    }

    private static string Scalar(JsonNode? node)
    {
        var text = node?.ToJsonString() ?? "null";

        return text.Length <= VALUE_WIDTH ? text : text[..(VALUE_WIDTH - 3)] + "...";
    }

    private static IDictionary<string, JsonNode?> Single(string name, JsonNode? node)
        => new Dictionary<string, JsonNode?>
        {
            [name] = node
        };

    /// <summary>
    ///     Appends (path, old, new) for every leaf that differs. Arrays of unequal length count as one leaf.
    /// </summary>
    private static void Walk(
        JsonNode? old,
        JsonNode? fresh,
        string path,
        List<(string Path, JsonNode? Before, JsonNode? After)> leaves)
    {
        switch (old, fresh)
        {
            case (JsonObject oldObject, JsonObject newObject):
                foreach (var key in oldObject.Select(pair => pair.Key)
                                             .Union(newObject.Select(pair => pair.Key))
                                             .Order(StringComparer.Ordinal))
                {
                    var sub = path.Length == 0 ? key : $"{path}.{key}";

                    if (!newObject.ContainsKey(key))
                        leaves.Add((sub, oldObject[key], null));
                    else if (!oldObject.ContainsKey(key))
                        leaves.Add((sub, null, newObject[key]));
                    else
                        Walk(oldObject[key], newObject[key], sub, leaves);
                }

                return;
            case (JsonArray oldArray, JsonArray newArray):
                if (oldArray.Count != newArray.Count)
                {
                    leaves.Add((path, old, fresh));

                    return;
                }

                for (var index = 0; index < oldArray.Count; index++)
                    Walk(oldArray[index], newArray[index], $"{path}[{index}]", leaves);

                return;
            default:
                if (!JsonNode.DeepEquals(old, fresh))
                    leaves.Add((path, old, fresh));

                return;
        }
    }
}
