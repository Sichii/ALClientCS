using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using AL.Data.Skills;
using AL.Data.Titles;
using AL.MemberGenerator.Extensions;
using Chaos.Core.Extensions;
using Newtonsoft.Json.Linq;

namespace AL.MemberGenerator
{
    internal class Program
    {
        private const string FOLDER_NAME = "dataMembers";
        private const string GLOBAL_PREFIX = "public ";
        private const string GLOBAL_SUFFIX = " { get; init; } = null!;";
        private static readonly Dictionary<string, string> Replacements = new(StringComparer.OrdinalIgnoreCase)
        {
            { "licenced", "licensed" },
            { "elixirfires", "elixirfireres" },
            { "elixirfzres", "elixirfreezeres" },
            { "bank_b", "BankBasement" },
            { "bank_u", "BankUnderground" }
        };

        private static readonly Dictionary<string, string> TypeStrings = new(StringComparer.OrdinalIgnoreCase)
        {
            { "achievements", nameof(GAchievement) },
            //animations
            { "classes", nameof(GClass) },
            { "conditions", nameof(GCondition) },
            //cosmetics
            { "craft", nameof(Recipe) },
            { "dimensions", $"{nameof(IReadOnlyList<float>)}<float>" },
            { "dismantle", nameof(Recipe) },
            //docs
            //drops
            //emotions
            { "events", nameof(GEvent) },
            //games
            { "geometry", nameof(GGeometry) },
            //images
            //imagesets
            //inflation
            { "items", nameof(GItem) },
            //levels
            { "maps", nameof(GMap) },
            { "monsters", nameof(GMonster) },
            { "npcs", nameof(GNPC) },
            //positions
            { "projectiles", nameof(GProjectile) },
            //sets
            //shells_to_gold
            { "skills", nameof(GSkill) },
            //sprites
            //tilesets
            { "titles", nameof(GTitle) },
            { "tokens", $"{nameof(IReadOnlyDictionary<string, float>)}<string, float>" }
        };

        private static async Task Main()
        {
            Console.WriteLine("Generating data members");

            var gameData = await ALAPIClient.GetGameDataAsync().ConfigureAwait(false);
            var jObj = JObject.Parse(gameData);

            if (!Directory.Exists(FOLDER_NAME))
                Directory.CreateDirectory(FOLDER_NAME);

            await jObj.Properties()
                .ParallelForEachAsync(async gDataProperty =>
                {
                    var builder = new StringBuilder();
                    var fileName = $@"{FOLDER_NAME}\{gDataProperty.Name}.txt";

                    if (!TypeStrings.TryGetValue(gDataProperty.Name, out var typeString))
                        typeString = string.Empty;

                    foreach (var child in gDataProperty.Value.OfType<JProperty>())
                    {
                        var name = child.Name;
                        var jsonPropertyValue = name;

                        name = Replacements.TryGetValue(name, out var replacement) ? replacement.ToUpperFirstLetter() : name.ToCodeFormat();

                        if (!name.Equals(jsonPropertyValue))
                            builder.AppendLine($"[JsonProperty(\"{jsonPropertyValue}\")]");

                        builder.Append(GLOBAL_PREFIX);
                        builder.Append(!string.IsNullOrEmpty(typeString) ? typeString : "object");
                        builder.Append(' ');
                        builder.Append(name);
                        builder.AppendLine(GLOBAL_SUFFIX);
                    }

                    await File.WriteAllTextAsync(fileName, builder.ToString().Trim()).ConfigureAwait(false);
                })
                .ConfigureAwait(false);
        }
    }
}