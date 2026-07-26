#region
using AL.Core.Definitions;
using AL.Data;
using AL.Data.Skills;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     How the server signals that it accepted a skill.
/// </summary>
public enum SkillCompletionKind
{
    /// <summary>
    ///     Completes on the <c>skill_timeout</c> frame, which <c>consume_skill</c> sends for any skill that resolves to a
    ///     non-zero cooldown. This is the contract most skills use.
    /// </summary>
    Timeout,

    /// <summary>
    ///     Completes when a condition goes from absent to present on the character. The fallback for skills whose cooldown is
    ///     zero, since those get no <c>skill_timeout</c> frame at all.
    /// </summary>
    Condition,

    /// <summary>
    ///     Completes on a non-failed <c>game_response</c> of type <c>data</c> naming the skill in <c>place</c>. Used by the
    ///     skills whose whole volley collapses into a single frame.
    /// </summary>
    ResponseData,

    /// <summary>
    ///     Completes on the projectile itself, which is how <c>commence_attack</c> skills answer. Only valid for a skill
    ///     that produces exactly one projectile, since it settles on the first that arrives.
    /// </summary>
    Action,

    /// <summary>
    ///     Completes as soon as the emit is written, for the skills the server acknowledges with nothing at all.
    /// </summary>
    Immediate
}

/// <summary>
///     The signal a skill is awaited on. Orthogonal to what the call returns — collecting projectiles is a separate
///     concern from knowing the cast landed.
/// </summary>
public readonly struct SkillCompletion
{
    /// <summary>
    ///     The kind of signal awaited.
    /// </summary>
    public SkillCompletionKind Kind { get; }

    /// <summary>
    ///     The condition awaited when <see cref="Kind" /> is <see cref="SkillCompletionKind.Condition" />.
    /// </summary>
    public Condition RequiredCondition { get; }

    private SkillCompletion(SkillCompletionKind kind, Condition requiredCondition)
    {
        Kind = kind;
        RequiredCondition = requiredCondition;
    }

    /// <summary>
    ///     Awaits the <c>skill_timeout</c> frame.
    /// </summary>
    public static SkillCompletion Timeout { get; } = new(SkillCompletionKind.Timeout, Condition.None);

    /// <summary>
    ///     Awaits a non-failed <c>game_response</c> of type <c>data</c> naming the skill.
    /// </summary>
    public static SkillCompletion ResponseData { get; } = new(SkillCompletionKind.ResponseData, Condition.None);

    /// <summary>
    ///     Awaits the single projectile the skill produces.
    /// </summary>
    public static SkillCompletion Action { get; } = new(SkillCompletionKind.Action, Condition.None);

    /// <summary>
    ///     Returns as soon as the emit is written.
    /// </summary>
    public static SkillCompletion Immediate { get; } = new(SkillCompletionKind.Immediate, Condition.None);

    /// <summary>
    ///     Awaits <paramref name="condition" /> appearing on the character.
    /// </summary>
    /// <param name="condition">
    ///     The condition the skill applies to the caster.
    /// </param>
    public static SkillCompletion OnCondition(Condition condition) => new(SkillCompletionKind.Condition, condition);

    /// <summary>
    ///     Picks the completion a skill uses from G data, mirroring the server's <c>consume_skill</c>: resolve
    ///     <c>share</c> first, then send no frame at all if the resolved cooldown is zero.
    /// </summary>
    /// <param name="skillName">
    ///     The name of the skill as the server knows it.
    /// </param>
    /// <remarks>
    ///     A cooldown outranks a condition because the frame acknowledges <i>this</i> cast, whereas a condition can be
    ///     applied by someone else's skill. The skills that share <c>attack</c> resolve to no cooldown and so land on
    ///     <see cref="Immediate" /> here — every one of them overrides this explicitly, because they answer with
    ///     projectiles instead.
    ///     <br />
    ///     Two cases this gets wrong, all four of which have a dedicated method that overrides it: a skill carrying only
    ///     <c>reuse_cooldown</c> (<c>invis</c>, <c>pickpocket</c>, <c>fishing</c>, <c>mining</c>) reads as
    ///     <see cref="Timeout" /> here, but the server takes that cooldown when the skill finishes rather than when it is
    ///     cast, so no frame arrives in time.
    /// </remarks>
    public static SkillCompletion ForSkill(string skillName)
    {
        var skill = GameData.Skills[skillName];

        //an unknown name is assumed to behave like the majority, and the caller finds out via the network timeout
        if (skill == null)
            return Timeout;

        if (ResolveCooldownMS(skill) > 0)
            return Timeout;

        if (skill.Condition != Condition.None)
            return OnCondition(skill.Condition);

        return Immediate;
    }

    /// <summary>
    ///     The name the <c>skill_timeout</c> frame carries, which is the shared skill's name when there is one.
    ///     <c>quickstab</c> is acknowledged as <c>quickpunch</c>, for instance.
    /// </summary>
    /// <param name="skillName">
    ///     The name of the skill as the server knows it.
    /// </param>
    public static string ResolveTimeoutName(string skillName) => GameData.Skills[skillName]?.SharedCooldown ?? skillName;

    /// <summary>
    ///     The cooldown the server finds for a skill once <c>share</c> is resolved.
    /// </summary>
    /// <param name="skill">
    ///     The G entry for the skill.
    /// </param>
    private static int ResolveCooldownMS(GSkill skill)
    {
        if (skill.SharedCooldown == null)
            return skill.CooldownMS;

        return GameData.Skills[skill.SharedCooldown]
                       ?.CooldownMS
               ?? 0;
    }
}
