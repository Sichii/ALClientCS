#region
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.Data;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using Chaos.Extensions.Common;
using Condition = AL.Core.Definitions.Condition;
#endregion

namespace AL.Client.Extensions;

/// <summary>
///     Provides a set of extensions for <see cref="EntityBase" />'s.
/// </summary>
public static class EntityExtensions
{
    /// <summary>Calculates the final damage value</summary>
    /// <param name="entity">The entity that is attacking.</param>
    /// <param name="target">The entity being attacked.</param>
    /// <param name="damageType">The type of damage being dealt.</param>
    /// <param name="ignoreTempDefBuffs">
    ///     Whether or not to ignore defenses gained from temp buffs like
    ///     <see cref="AL.Core.Definitions.Condition.HardShell" />, <see cref="AL.Core.Definitions.Condition.WarCry" />, and
    ///     <see cref="AL.Core.Definitions.Condition.Fingered" />.
    /// </param>
    /// <returns>
    ///     <see cref="float" />
    ///     <br />
    ///     The amount of damage dealt after factoring in penetration and defenses.
    /// </returns>
    public static float CalculateDamageAgainst(
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
                    if (player.Conditions.ContainsKey(Condition.Fingered))
                        defense -= GameData.Conditions[Condition.Fingered]!.Resistance;

                    if (player.Conditions.ContainsKey(Condition.FullGuard))
                        defense -= GameData.Conditions[Condition.FullGuard]!.Resistance;

                    if (player.Conditions.ContainsKey(Condition.WarCry))
                        defense -= GameData.Conditions[Condition.WarCry]!.Resistance;
                }

                //the server subtracts pierce twice for damage: once from the attacker's live stat and once from
                //the copy stamped onto the projectile at creation
                defense -= entity.RPiercing;

                //heals subtract pierce once and halve the whole term instead
                if (damageType == DamageType.Heal)
                    defense /= 2;
                else
                    defense -= entity.RPiercing;

                break;
            }
            case DamageType.Physical:
            {
                defense = target.Armor;

                if (ignoreTempDefBuffs && target is Player player)
                {
                    if (player.Conditions.ContainsKey(Condition.HardShell))
                        defense -= GameData.Conditions[Condition.HardShell]!.Armor;

                    if (player.Conditions.ContainsKey(Condition.FullGuard))
                        defense -= GameData.Conditions[Condition.FullGuard]!.Armor;

                    if (player.Conditions.ContainsKey(Condition.WarCry))
                        defense -= GameData.Conditions[Condition.WarCry]!.Armor;
                }

                //pierce counts twice, same as the magical branch
                defense -= entity.APiercing * 2;

                break;
            }
            default:
                defense = 0;

                break;
        }

        var damageMultiplier = Utilities.CalculateDamageMultiplier(defense);

        return entity.Attack * damageMultiplier;
    }

    private static float CalculateMitigation(EntityBase target, ActionData projectile, Func<string, EntityBase?>? findAttackerFunc)
    {
        var attacker = string.IsNullOrEmpty(projectile.AttackerId) ? null : findAttackerFunc?.Invoke(projectile.AttackerId);

        if (attacker is null)
            return 1f;

        //a skill can stamp extra pierce onto its projectile - piercingshot's 500 - which the server adds to the
        //creation-time copy on top of the doubled stat
        var skill = string.IsNullOrEmpty(projectile.Source) ? null : GameData.Skills[projectile.Source];

        return ResolveDamageType(attacker, projectile.Source) switch
        {
            DamageType.Physical => Utilities.CalculateDamageMultiplier(target.Armor - attacker.APiercing * 2 - (skill?.APiercing ?? 0f)),
            DamageType.Magical =>
                Utilities.CalculateDamageMultiplier(target.Resistance - attacker.RPiercing * 2 - (skill?.RPiercing ?? 0f)),

            //pure ignores defenses entirely, and heals never carry damage
            _ => 1f
        };
    }

    /// <summary>
    ///     Determines the damage type the server selects: monster data or class, then mainhand weapon, then skill for anything
    ///     but a basic attack. Unset falls back to physical.
    /// </summary>
    /// <param name="attacker">The entity dealing the damage.</param>
    /// <param name="source">The skill that dealt it, if any.</param>
    /// <returns>The damage type the server applies.</returns>
    private static DamageType ResolveDamageType(EntityBase attacker, string? source)
    {
        var damageType = attacker switch
        {
            Monster monster => monster.GetData()
                                      .DamageType,
            Player player => ResolvePlayerDamageType(player),
            _             => DamageType.Physical
        };

        if (!string.IsNullOrEmpty(source) && !source.EqualsI("attack"))
        {
            var skillDamageType = GameData.Skills[source]?.DamageType ?? DamageType.None;

            if (skillDamageType != DamageType.None)
                damageType = skillDamageType;
        }

        return damageType == DamageType.None ? DamageType.Physical : damageType;
    }

    private static DamageType ResolvePlayerDamageType(Player player)
    {
        var damageType = player.GetData()
                               ?.DamageType
                         ?? DamageType.Physical;

        if (player.Slots.TryGetValue(Slot.MainHand, out var mainHand) && mainHand is not null)
        {
            var weaponDamageType = GameData.Items[mainHand.Name]?.DamageType ?? DamageType.None;

            if (weaponDamageType != DamageType.None)
                damageType = weaponDamageType;
        }

        return damageType;
    }

    /// <summary>
    ///     Calculates whether or not the entity will die to burning damage.
    /// </summary>
    /// <param name="entity">The eneity to check.</param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>true</c> if the entity will die to burning damage, otherwise <c>false</c> .
    /// </returns>
    public static bool WillBurnToDeath(this EntityBase entity)
    {
        if (!entity.Conditions.TryGetValue(Condition.Burned, out var burning))
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

        const double INSTANCE_UPDATE_MS = 75;

        //the server polls conditions once per instance update, at least 75ms apart, so a burn ticks on the first
        //update at or after its interval
        var interval = GameData.Conditions.Burned.IntervalMS;
        var tickMs = Math.Ceiling(interval / INSTANCE_UPDATE_MS) * INSTANCE_UPDATE_MS;

        //RemainingMs, not DurationMs - the latter is the wire value from whenever the frame arrived
        var remainingTicks = Math.Floor(burning.RemainingMs / tickMs);

        //a fifth of the intensity per tick, rounded up
        var damagePerTick = Math.Ceiling(burning.Intensity / 5f);

        return (remainingTicks * damagePerTick) > entity.HP;
    }

    /// <summary>
    ///     Calculates whether or not the entity will die to existing projectiles.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <param name="projectiles">The projectiles to use in the check.</param>
    /// <param name="findAttackerFunc">
    ///     Resolves a projectile's attacker id to a live entity. When provided, each projectile's damage is reduced by the
    ///     target's armor or resistance less twice the attacker's pierce. A projectile whose attacker cannot be resolved
    ///     counts at full damage.
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>true</c> if the entity will die to existing projectiles, otherwise <c>false</c> .
    /// </returns>
    public static bool WillDieToProjectiles(
        this EntityBase entity,
        IEnumerable<ActionData> projectiles,
        Func<string, EntityBase?>? findAttackerFunc = null)
    {
        if ((entity.Evasion > 0) || (entity.Reflection > 0) || (entity.Lifesteal > 0))
            return false;

        if (entity is Monster monster
            && monster.GetData()
                      ._1hp)
            return false;

        return (projectiles.Where(proj => proj.Target == entity.Id)
                           .Select(proj => proj.Damage * CalculateMitigation(entity, proj, findAttackerFunc))
                           .Sum()
                * 0.95)
               > entity.HP;
    }
}