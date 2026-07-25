#region
using AL.Core.Helpers;
using AL.SocketClient.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     Phase 10 - inbound event coverage. A server frame is dispatched only if its wire name parses to an
///     <see cref="ALSocketMessageType" /> via <c>EnumHelper.TryParse</c> (OrdinalIgnoreCase over the member name
///     and its [EnumMember] value). This pins every tier-1 inbound member to the exact server wire name, so a
///     rename or a missing [EnumMember] fails the build's tests.
/// </summary>
[TestClass]
public class MessageTypeWireNameTests
{
    [DataTestMethod]
    [DataRow("cm", ALSocketMessageType.Cm)]
    [DataRow("magiport", ALSocketMessageType.Magiport)]
    [DataRow("request", ALSocketMessageType.Request)]
    [DataRow("track", ALSocketMessageType.Track)]
    [DataRow("tracker", ALSocketMessageType.Tracker)]
    [DataRow("lostandfound", ALSocketMessageType.LostAndFound)]
    [DataRow("kill_credit", ALSocketMessageType.KillCredit)]
    [DataRow("trade_history", ALSocketMessageType.TradeHistory)]
    [DataRow("game_event", ALSocketMessageType.GameEvent)]
    public void WireNameParsesToMessageType(string wireName, ALSocketMessageType expected)
    {
        var parsed = EnumHelper.TryParse(wireName, out ALSocketMessageType actual);

        Assert.IsTrue(parsed, $"'{wireName}' did not parse to an ALSocketMessageType");
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///     EnumHelper builds a case-insensitive value lookup that throws on a duplicate key; a successful parse
    ///     proves initialisation did not throw - i.e. no two members collide on name or [EnumMember] value.
    /// </summary>
    [TestMethod]
    public void MessageTypeLookupHasNoDuplicateKeys()
        => Assert.IsTrue(EnumHelper.TryParse("entities", out ALSocketMessageType _));
}
