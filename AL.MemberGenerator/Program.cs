#region
using System.Text;
using AL.APIClient;
using AL.Data;
using AL.Data.Achievements;
using AL.Data.Classes;
using AL.Data.Conditions;
using AL.Data.Events;
using AL.Data.Geometry;
using AL.Data.Items;
using AL.Data.Maps;
using AL.Data.Monsters;
using AL.Data.NPCs;
using AL.Data.Projectiles;
using AL.Data.Sets;
using AL.Data.Skills;
using AL.Data.Titles;
using AL.MemberGenerator.Extensions;
using System.Text.Json.Nodes;
#endregion

namespace AL.MemberGenerator;

public class Program
{
    private const string FOLDER_NAME = "dataMembers";
    private const string GLOBAL_PREFIX = "public ";
    private const string GLOBAL_SUFFIX = " { get; init; } = null!;";

    private static readonly Dictionary<string, string> TypeStrings = new(StringComparer.OrdinalIgnoreCase)
    {
        {
            "achievements", nameof(GAchievement)
        },

        //animations
        {
            "classes", nameof(GClass)
        },
        {
            "conditions", nameof(GCondition)
        },

        //cosmetics
        {
            "craft", nameof(Recipe)
        },
        {
            "dimensions", $"{nameof(IReadOnlyList<float>)}<float>"
        },
        {
            "dismantle", nameof(Recipe)
        },

        //docs
        //drops
        //emotions
        {
            "events", nameof(GEvent)
        },

        //games
        {
            "geometry", nameof(GGeometry)
        },

        //images
        //imagesets
        //inflation
        {
            "items", nameof(GItem)
        },

        //levels
        {
            "maps", nameof(GMap)
        },
        {
            "monsters", nameof(GMonster)
        },
        {
            "npcs", nameof(GNPC)
        },

        //positions
        {
            "projectiles", nameof(GProjectile)
        },

        {
            "sets", nameof(GSet)
        },

        //shells_to_gold
        {
            "skills", nameof(GSkill)
        },

        //sprites
        //tilesets
        {
            "titles", nameof(GTitle)
        },
        {
            "tokens", $"{nameof(IReadOnlyDictionary<string, float>)}<string, float>"
        }
    };

    private static async Task Main()
    {
        Console.WriteLine("Generating data members");

        var gameData = await AlApiClient.GetGameDataAsync();
        var jObj = JsonNode.Parse(gameData)!.AsObject();
        var version = jObj["version"]!.GetValue<int>();

        if (!Directory.Exists(FOLDER_NAME))
            Directory.CreateDirectory(FOLDER_NAME);

        await Parallel.ForEachAsync(
            jObj,
            async (gDataProperty, _) =>
            {
                var builder = new StringBuilder();
                var fileName = $@"{FOLDER_NAME}\{gDataProperty.Key}.txt";

                //"version" is a scalar, not a member table: emit it as the KNOWN_VERSION stamp. Paste it over
                //GameData.KNOWN_VERSION when refreshing the datums so Populate can tell when live data outruns them
                if (gDataProperty.Key is "version")
                {
                    await File.WriteAllTextAsync(fileName, $"public const int KNOWN_VERSION = {gDataProperty.Value!.GetValue<int>()};");

                    return;
                }

                if (!TypeStrings.TryGetValue(gDataProperty.Key, out var typeString))
                    typeString = string.Empty;

                //a section that is not an object has no members to generate. Newtonsoft's Children<JProperty>()
                //filtered by type and silently yielded nothing for those; AsObject() would throw instead. The
                //empty file is still written, as it was before.
                foreach (var child in gDataProperty.Value as JsonObject ?? [])
                {
                    var jsonPropertyValue = child.Key;
                    var name = jsonPropertyValue.ToCodeFormat();

                    if (!name.Equals(jsonPropertyValue))
                        builder.AppendLine($"[JsonPropertyName(\"{jsonPropertyValue}\")]");

                    builder.Append(GLOBAL_PREFIX);
                    builder.Append(!string.IsNullOrEmpty(typeString) ? typeString : "object");
                    builder.Append(' ');
                    builder.Append(name);
                    builder.AppendLine(GLOBAL_SUFFIX);
                }

                await File.WriteAllTextAsync(
                    fileName,
                    builder.ToString()
                           .Trim());
            });

        //the value half of a refresh: the member files say which keys moved, and only the previous fetch can say which
        //values moved behind the keys that stayed. Kept beside the tool that fetched it, so the diff is the same
        //wherever the refresh is run from
        if (Snapshots.BaselineFor(version) is { } baseline)
        {
            var old = JsonNode.Parse(await File.ReadAllTextAsync(baseline))!.AsObject();

            Console.WriteLine(
                $"values: {old["version"]} ({Path.GetFileName(baseline)}, fetched {File.GetLastWriteTime(baseline):yyyy-MM-dd}) -> {version}");

            foreach (var line in Snapshots.Diff(old, jObj))
                Console.WriteLine(line);
        } else
            Console.WriteLine($"no earlier snapshot under {Snapshots.FOLDER_NAME}/; nothing to compare on a first run.");

        await Snapshots.FileAwayAsync(gameData, version);
        Console.WriteLine($"filed {Snapshots.FOLDER_NAME}/G-{version}.json; keeping the {Snapshots.KEEP} newest");
    }
}