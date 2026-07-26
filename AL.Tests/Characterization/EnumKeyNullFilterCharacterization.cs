#region
using AL.Client.Extensions;
using AL.SocketClient.Model;
using FluentAssertions;
using Condition = AL.Core.Definitions.Condition;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins
///     <c>
///         TolerantEnumKeyDictionaryConverter
///     </c>
///     's
///     <c>
///         if (value != null)
///     </c>
///     filter as load-bearing: a null value under a known enum key is dropped so it never enters the non-nullable
///     <see cref="EntityBase.Conditions" /> dictionary, and an unrecognised key is skipped rather than merged onto the
///     enum's zero member. Neither is System.Text.Json's own default, which is why the whole-dictionary converter exists
///     and why it is applied by a member-level
///     <c>
///         [JsonConverter]
///     </c>
///     on <see cref="EntityBase.Conditions" /> rather than left to the built-in enum-key reader.
/// </summary>
public class EnumKeyNullFilterCharacterization
{
    [Test]
    public void T9_KnownConditionKeyWithNullValueIsFilteredOut()
    {
        //the server can stamp a bare "condition":null - the filter must keep it out of the non-nullable dictionary
        const string ENTITY = @"{ ""id"":""someMonster"", ""s"":{ ""burned"":null, ""poisoned"":{ ""ms"":100 } } }";

        var monster = TestJson.Socket<Monster>(ENTITY);

        monster.Should()
               .NotBeNull();

        monster.Conditions
               .Count
               .Should()
               .Be(1);

        monster.Conditions
               .ContainsKey(Condition.Burned)
               .Should()
               .BeFalse();

        monster.Conditions
               .ContainsKey(Condition.Poisoned)
               .Should()
               .BeTrue();
    }

    [Test]
    public void T9_UnknownConditionKeyIsSkippedNotMappedToZeroMember()
    {
        //Condition.None is the zero member. STJ's bare enum-key reader degrades an unknown key onto it and merges
        //last-write-wins; the whole-dictionary converter is the only reason the key is skipped instead.
        const string ENTITY = @"{ ""id"":""someMonster"", ""s"":{ ""__not_a_condition"":{ ""ms"":1 } } }";

        var monster = TestJson.Socket<Monster>(ENTITY);

        monster.Should()
               .NotBeNull();

        monster.Conditions
               .Count
               .Should()
               .Be(0);

        monster.Conditions
               .ContainsKey(Condition.None)
               .Should()
               .BeFalse();
    }

    [Test]
    public void T9_WillBurnToDeathReturnsFalseWhenBurnedValueWasNull()
    {
        //without the null filter a null Condition lands under Burned and WillBurnToDeath NREs on burning.DurationMs
        const string ENTITY = @"{ ""id"":""someMonster"", ""s"":{ ""burned"":null, ""poisoned"":{ ""ms"":100 } } }";

        var monster = TestJson.Socket<Monster>(ENTITY);

        monster.Should()
               .NotBeNull();

        monster.WillBurnToDeath()
               .Should()
               .BeFalse();
    }
}