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
        //LEAVE NULLABLE - needed to chack if something is actually compoundable null vs empty
        public IReadOnlyDictionary<ALAttribute, float> CompoundModifiers { get; init; }

        [JsonProperty("damage")]
        public DamageType DamageType { get; init; }

        [JsonProperty("g")]
        public float GoldValue { get; init; }
        //LEAVE NULLABLE - needed to chack if something is actually compoundable/upgradeable null vs empty
        public IReadOnlyList<int> Grades { get; init; }
        public bool Ignore { get; init; }
        public string Name { get; init; }
        public string Projectile { get; init; }
        public ALAttribute ScrollStat { get; private set; }

        [JsonProperty("s"), JsonConverter(typeof(FalsyConverter<int>), 1)]
        public int StackSize { get; init; } = 1;

        public float Tier { get; init; }
        public ItemType Type { get; init; }

        [JsonProperty("upgrade")]
        //LEAVE NULLABLE - needed to chack if something is actually upgradeable null vs empty
        public IReadOnlyDictionary<ALAttribute, float> UpgradeModifiers { get; init; }

        [JsonProperty("wtype")]
        public WeaponType WeaponType { get; init; }

        [OnError]
        // ReSharper disable once UnusedParameter.Global
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