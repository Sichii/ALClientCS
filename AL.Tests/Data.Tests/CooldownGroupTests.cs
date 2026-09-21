#region
using AL.Data.Skills;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     Which name a skill's cooldown is tracked under. The four potion and regeneration abilities pair up two at a time
///     under <c>share</c> but all four lock together under one group, so reading the pairing alone leaves half the set
///     looking ready when the server would refuse it.
/// </summary>
public class CooldownGroupTests
{
    /// <summary>
    ///     A group wins over a shared cooldown, which is the whole reason it exists: <c>regen_hp</c> carries both, and
    ///     answering <c>use_hp</c> would leave the MP half of the set free.
    /// </summary>
    [Test]
    public void AGroupBeatsAShare()
    {
        var regenHp = new GSkill
        {
            CooldownGroup = "potion",
            SharedCooldown = "use_hp",
            CooldownMultiplier = 2
        };

        regenHp.CooldownKey("regen_hp")
               .Should()
               .Be("potion");
    }

    /// <summary>
    ///     Every skill in a group answers the same key, which is what makes one of them going down take the rest with it.
    /// </summary>
    [Test]
    public void EveryPotionSkillAnswersTheSameKey()
    {
        var group = new[]
        {
            (Name: "use_hp", Skill: new GSkill
            {
                CooldownGroup = "potion"
            }),
            (Name: "use_mp", Skill: new GSkill
            {
                CooldownGroup = "potion"
            }),
            (Name: "regen_hp", Skill: new GSkill
            {
                CooldownGroup = "potion",
                SharedCooldown = "use_hp"
            }),
            (Name: "regen_mp", Skill: new GSkill
            {
                CooldownGroup = "potion",
                SharedCooldown = "use_mp"
            })
        };

        group.Select(entry => entry.Skill.CooldownKey(entry.Name))
             .Should()
             .AllBe("potion");
    }

    /// <summary>
    ///     Nearly every skill is in no group, so the two older answers have to survive unchanged.
    /// </summary>
    [Test]
    public void WithoutAGroupTheOlderAnswersHold()
    {
        new GSkill
            {
                SharedCooldown = "attack"
            }.CooldownKey("heal")
             .Should()
             .Be("attack");

        new GSkill().CooldownKey("blink")
                    .Should()
                    .Be("blink");
    }
}