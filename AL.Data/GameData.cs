using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AL.Core.Helpers;
using AL.Data.Achievements;
using AL.Data.Classes;
using AL.Data.Conditions;
using AL.Data.Craft;
using AL.Data.Dismantle;
using AL.Data.Events;
using AL.Data.Games;
using AL.Data.Geometry;
using AL.Data.Items;
using AL.Data.Maps;
using AL.Data.Monsters;
using AL.Data.NPCs;
using AL.Data.Projectiles;
using AL.Data.Skills;
using AL.Data.Titles;
using AL.Data.Tokens;
using Newtonsoft.Json;

namespace AL.Data
{
    [JsonObject]
    public record GameData
    {
        [JsonProperty]
        public static AchievementsDatum Achievements { get; private set; }

        [JsonProperty]
        public static ClassesDatum Classes { get; private set; }

        [JsonProperty]
        public static ConditionsDatum Conditions { get; private set; }

        [JsonProperty]
        public static CraftDatum Craft { get; private set; }

        [JsonProperty]
        public static DismantleDatum Dismantle { get; private set; }

        [JsonProperty]
        public static ALEventsDatum Events { get; private set; }

        [JsonProperty]
        public static GamesDatum Games { get; private set; }

        [JsonProperty]
        public static GeometryDatum Geometry { get; private set; }

        [JsonProperty]
        public static int Inflation { get; private set; }

        [JsonProperty]
        public static ItemsDatum Items { get; private set; }

        [JsonProperty]
        public static IReadOnlyDictionary<int, float> Levels { get; private set; }

        [JsonProperty]
        public static MapsDatum Maps { get; private set; }

        [JsonProperty]
        public static MonstersDatum Monsters { get; private set; }

        [JsonProperty]
        public static NPCsDatum NPCs { get; private set; }

        [JsonProperty]
        public static ProjectilesDatum Projectiles { get; private set; }

        [JsonProperty("shells_to_gold")]
        public static int ShellsToGold { get; private set; }

        [JsonProperty]
        public static SkillsDatum Skills { get; private set; }

        [JsonProperty]
        public static TitlesDatum Titles { get; private set; }

        [JsonProperty]
        public static TokensDatum Tokens { get; private set; }

        //public Dimensions Dimensions { get; private set; }

        //public Sprites Sprites { get; private set; }
        //public Tilesets Tilesets { get; private set; }
        //public Sets Sets { get; private set; }
        //public Positions Positions { get; private set; }
        //public Images1 Images { get; private set; }
        //public Imagesets Imagesets { get; private set; }
        //public Docs Docs { get; private set; }
        //public Drops Drops { get; private set; }
        //public Emotions Emotions { get; private set; }
        //public Cosmetics Cosmetics { get; private set; }

        [JsonProperty]
        public static int Version { get; private set; }

        private static void FixLines()
        {
            foreach (var mapGeometry in Geometry.Values)
            {
                mapGeometry.XLines = LineHelper.FixLines(mapGeometry.XLines, true);
                mapGeometry.YLines = LineHelper.FixLines(mapGeometry.YLines, false);
            }
        }

        public static async Task PopulateAsync()
        {
            using var webClient = new WebClient();
            var json = await webClient.DownloadStringTaskAsync("http://adventure.land/data.js");
            json = json.Substring(6, json.Length - 8);
            JsonConvert.DeserializeObject<GameData>(json);

            Achievements.ConstructCache();
            Classes.ConstructCache();
            Conditions.ConstructCache();
            Craft.ConstructCache();
            Dismantle.ConstructCache();
            Events.ConstructCache();
            Geometry.ConstructCache();
            Items.ConstructCache();
            Maps.ConstructCache();
            Monsters.ConstructCache();
            NPCs.ConstructCache();
            Projectiles.ConstructCache();
            Skills.ConstructCache();
            Titles.ConstructCache();
            Tokens.ConstructCache();

            FixLines();
        }
    }
}