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
    /// <summary>
    ///     <inheritdoc cref="AttributedRecordBase" /> <br />
    ///     Represents the static data for an item.
    /// </summary>
    /// <seealso cref="AttributedRecordBase" />
    public record Item : AttributedRecordBase
    {
        /// <summary>
        ///     <b>NULLABLE</b>. If null, this item is not compoundable.
        ///     If NOT null, this dictionary contains the <see cref="ALAttribute" /> modifications per compound level.
        /// </summary>
        [JsonProperty("compound")]
        public IReadOnlyDictionary<ALAttribute, float>? CompoundModifiers { get; init; }

        /// <summary>
        ///     If this item is a weapon, this is the damage type of the weapon.
        /// </summary>
        [JsonProperty("damage")]
        public DamageType DamageType { get; init; }

        /// <summary>
        ///     The default gold value of this item if selling to an NPC merchant.
        /// </summary>
        [JsonProperty("g")]
        public float GoldValue { get; init; }

        /// <summary>
        ///     <b>NULLABLE</b>. If null, this item is not compoundable or upgradeable.
        ///     If NOT null, this list contains the levels at which the grade increases.
        /// </summary>
        public IReadOnlyList<int>? Grades { get; init; }

        /// <summary>
        ///     If this is true, this is bad/old data that should be ignored.
        /// </summary>
        public bool Ignore { get; init; }

        /// <summary>
        ///     The name of the item.
        /// </summary>
        public string Name { get; init; } = null!;

        /// <summary>
        ///     If this item is a weapon, this is the name of the projectile it emits when attacking.
        /// </summary>
        public string? Projectile { get; init; }

        /// <summary>
        ///     If this is an equipment item, this is the number of stats this item will give at level 0.
        /// </summary>
        [JsonIgnore]
        public ALAttribute ScrollStat { get; private set; }

        /// <summary>
        ///     If this is an equipment item, the set this item belongs to.
        /// </summary>
        public ArmorSet Set { get; init; }

        /// <summary>
        ///     The number of this item that can be placed in a stack.
        /// </summary>
        [JsonProperty("s"), JsonConverter(typeof(FalsyConverter<int>), 1)]
        public int StackSize { get; init; } = 1;

        /// <summary>
        ///     If this is an equipment item, this is the tier of the item. (higher is better)
        /// </summary>
        public float Tier { get; init; }

        /// <summary>
        ///     The type of item.
        /// </summary>
        public ItemType Type { get; init; }

        /// <summary>
        ///     <b>NULLABLE</b>. If null, this item is not upgradeable.
        ///     If NOT null, this dictionary contains the <see cref="ALAttribute" /> modifications per upgrade level.
        /// </summary>
        [JsonProperty("upgrade")]
        public IReadOnlyDictionary<ALAttribute, float>? UpgradeModifiers { get; init; }

        /// <summary>
        ///     If this item is a weapon, this is the type of weapon.
        /// </summary>
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