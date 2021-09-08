using System.Threading.Tasks;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.Data;
using AL.SocketClient.Model;
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
    }
}