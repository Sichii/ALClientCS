using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.Data;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using Condition = AL.Core.Definitions.Condition;

namespace AL.Client.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for <see cref="EntityBase" />'s.
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        ///     Calculates the final damage value
        /// </summary>
        /// <param name="entity">The entity that is attacking.</param>
        /// <param name="target">The entity being attacked.</param>
        /// <param name="damageType">The type of damage being dealt.</param>
        /// <param name="ignoreTempDefBuffs">
        ///     Whether or not to ignore defenses gained from temp buffs like
        ///     <see cref="AL.Core.Definitions.Condition.HardShell" />, <see cref="AL.Core.Definitions.Condition.WarCry" />, and
        ///     <see cref="AL.Core.Definitions.Condition.Fingered" />.
        /// </param>
        /// <returns>
        ///     <see cref="float" /> <br />
        ///     The amount of damage dealt after factoring in penetration and defenses.
        /// </returns>
        public static async ValueTask<float> CalculateDamageAgainstAsync(
            this EntityBase entity,
            EntityBase target,
            DamageType damageType,
            bool ignoreTempDefBuffs = false)
        {
            var defense = 0f;

            switch (damageType)
            {
                case DamageType.None:
                    break;
                case DamageType.Heal:
                case DamageType.Magical:
                {
                    defense = target.Resistance;

                    if (ignoreTempDefBuffs && target is Player player)
                    {
                        if (await player.Conditions.ContainsKeyAsync(Condition.Fingered).ConfigureAwait(false))
                            defense -= GameData.Conditions[Condition.Fingered]!.Resistance;

                        if (await player.Conditions.ContainsKeyAsync(Condition.FullGuard).ConfigureAwait(false))
                            defense -= GameData.Conditions[Condition.FullGuard]!.Resistance;

                        if (await player.Conditions.ContainsKeyAsync(Condition.WarCry).ConfigureAwait(false))
                            defense -= GameData.Conditions[Condition.WarCry]!.Resistance;
                    }

                    defense -= entity.RPiercing;

                    if (damageType == DamageType.Heal)
                        defense /= 2;

                    break;
                }
                case DamageType.Physical:
                {
                    defense = target.Armor;

                    if (ignoreTempDefBuffs && target is Player player)
                    {
                        if (await player.Conditions.ContainsKeyAsync(Condition.HardShell).ConfigureAwait(false))
                            defense -= GameData.Conditions[Condition.HardShell]!.Armor;

                        if (await player.Conditions.ContainsKeyAsync(Condition.FullGuard).ConfigureAwait(false))
                            defense -= GameData.Conditions[Condition.FullGuard]!.Armor;

                        if (await player.Conditions.ContainsKeyAsync(Condition.WarCry).ConfigureAwait(false))
                            defense -= GameData.Conditions[Condition.WarCry]!.Armor;
                    }

                    defense -= entity.APiercing;

                    break;
                }
                default:
                    defense = 0;

                    break;
            }

            var damageMultiplier = Utilities.CalculateDamageMultiplier(defense);

            return entity.Attack * damageMultiplier;
        }

        /// <summary>
        ///     Calculates whether or not the entity will die to existing projectiles.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <param name="projectiles">The projectiles to use in the check.</param>
        /// <returns><see cref="bool"/> <br/>
        /// <c>true</c> if the entity will die to existing projectiles, otherwise <c>false</c>.</returns>
        public static bool WillDieToProjectiles(this EntityBase entity, IEnumerable<ActionData> projectiles)
        {
            if ((entity.Evasion > 0) || (entity.Reflection > 0) || (entity.Lifesteal > 0))
                return false;
            
            if (entity is Monster monster && monster.GetData()._1hp)
                return false;

            return projectiles.Where(proj => proj.Target == entity.Id).Select(proj => proj.Damage).Sum() * 0.95 > entity.HP;
        }

        /// <summary>
        ///     Calculates whether or not the entity will die to burning damage.
        /// </summary>
        /// <param name="entity">The eneity to check.</param>
        /// <returns><see cref="bool"/> <br/>
        /// <c>true</c> if the entity will die to burning damage, otherwise <c>false</c>.</returns>
        public static async ValueTask<bool> WillBurnToDeathAsync(this EntityBase entity)
        {
            if (!await entity.Conditions.TryGetValueAsync(Condition.Burned, out var burnedTask).ConfigureAwait(false))
                return false;
            
            if (entity.Lifesteal > 0)
                return false;

            if (entity is Monster monster)
            {
                var data = monster.GetData();
                if (data._1hp)
                    return false;

                if (data.Abilities.ContainsKey("self_healing"))
                    return false;
            }

            var burning = await burnedTask.ConfigureAwait(false);
            var interval = GameData.Conditions.Burned.IntervalMS;
            var remainingTicks = Math.Floor(burning.RemainingMS / interval);
            var damagePer = burning.Intensity / 5;

            return remainingTicks * damagePer > entity.HP;
        }
    }
}