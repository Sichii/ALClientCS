#region
using System.Collections.Generic;
using AL.Client.Extensions;
using AL.Core.Definitions;
using AL.SocketClient.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Condition = AL.Core.Definitions.Condition;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins <c>TolerantEnumKeyDictionaryConverter</c>'s <c>if (value != null)</c> filter as load-bearing:
///     a null value under a known enum key is dropped so it never enters the non-nullable
///     <see cref="EntityBase.Conditions" /> dictionary, and an unrecognised key is skipped rather than merged onto
///     the enum's zero member. Neither is System.Text.Json's own default, which is why the whole-dictionary
///     converter exists and why every pin here is asserted against both engines rather than re-pointed to one.
/// </summary>
[TestClass]
public class EnumKeyNullFilterCharacterization
{
    /// <summary>
    ///     The same frame through the frozen Newtonsoft oracle and the System.Text.Json path production now runs.
    ///     Asserting each pin against both is the migration proof; asserting it against one is just a restatement.
    /// </summary>
    private static IEnumerable<(string Engine, Monster? Monster)> BothEngines(string json)
    {
        yield return ("Newtonsoft", JsonConvert.DeserializeObject<Monster>(json));

        yield return ("STJ", TestJson.Socket<Monster>(json));
    }

    [TestMethod]
    public void T9_KnownConditionKeyWithNullValueIsFilteredOut()
    {
        //the server can stamp a bare "condition":null - the filter must keep it out of the non-nullable dictionary
        const string ENTITY = @"{ ""id"":""someMonster"", ""s"":{ ""burned"":null, ""poisoned"":{ ""ms"":100 } } }";

        foreach ((var engine, var monster) in BothEngines(ENTITY))
        {
            Assert.IsNotNull(monster, engine);
            Assert.AreEqual(1, monster.Conditions.Count, engine);
            Assert.IsFalse(monster.Conditions.ContainsKey(Condition.Burned), engine);
            Assert.IsTrue(monster.Conditions.ContainsKey(Condition.Poisoned), engine);
        }
    }

    [TestMethod]
    public void T9_WillBurnToDeathReturnsFalseWhenBurnedValueWasNull()
    {
        //without the null filter a null Condition lands under Burned and WillBurnToDeath NREs on burning.DurationMs
        const string ENTITY = @"{ ""id"":""someMonster"", ""s"":{ ""burned"":null, ""poisoned"":{ ""ms"":100 } } }";

        foreach ((var engine, var monster) in BothEngines(ENTITY))
        {
            Assert.IsNotNull(monster, engine);
            Assert.IsFalse(monster.WillBurnToDeath(), engine);
        }
    }

    [TestMethod]
    public void T9_UnknownConditionKeyIsSkippedNotMappedToZeroMember()
    {
        //Condition.None is the zero member. STJ's bare enum-key reader degrades an unknown key onto it and merges
        //last-write-wins; the whole-dictionary converter is the only reason both engines skip the key instead.
        const string ENTITY = @"{ ""id"":""someMonster"", ""s"":{ ""__not_a_condition"":{ ""ms"":1 } } }";

        foreach ((var engine, var monster) in BothEngines(ENTITY))
        {
            Assert.IsNotNull(monster, engine);
            Assert.AreEqual(0, monster.Conditions.Count, engine);
            Assert.IsFalse(monster.Conditions.ContainsKey(Condition.None), engine);
        }
    }
}
