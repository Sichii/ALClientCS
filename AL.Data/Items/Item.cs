using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.Core.Json.Converters;
using Chaos.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AL.Data.Items
{
    public record Item : AttributedRecordBase
    {
        [JsonProperty("compound")]
        public IReadOnlyDictionary<ALAttribute, float> CompoundModifiers { get; init; } =
            new Dictionary<ALAttribute, float>();

        [JsonProperty("damage")]
        public DamageType DamageType { get; init; }

        [JsonProperty("g")]
        public float GoldValue { get; init; }
        public int[] Grades { get; init; }
        public bool Ignore { get; init; }
        public string Name { get; init; }
        public string Projectile { get; init; }
        public ALAttribute ScrollStat { get; private set; }

        [JsonProperty("s"), JsonConverter(typeof(ObjOrFalseConverter<int>), 1)]
        public int StackSize { get; init; } = 1;

        public float Tier { get; init; }
        public ItemType Type { get; init; }

        [JsonProperty("upgrade")]
        public IReadOnlyDictionary<ALAttribute, float> UpgradeModifiers { get; init; } =
            new Dictionary<ALAttribute, float>();

        [JsonProperty("wtype")]
        public WeaponType WeaponType { get; init; }

        [OnError]
        public void OnError(StreamingContext context, ErrorContext errorContext)
        {
            if (errorContext.Member?.ToString().EqualsI("stat") == true)
            {
                var match = Regex.Match(errorContext.Error.Message, @"double\: (\w+)\.");

                if (match.Success && EnumHelper.TryParse(match.Groups[1].Value, out ALAttribute attr))
                {
                    ScrollStat = attr;
                    errorContext.Handled = true;
                }
            }
        }
    }
}