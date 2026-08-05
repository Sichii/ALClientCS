#region
using AL.Core.Helpers;
using AL.SocketClient.Definitions;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     Phase 10 - inbound event coverage. A server frame is dispatched only if its wire name parses to an
///     <see cref="ALSocketMessageType" /> via
///     <c>
///         EnumHelper.TryParse
///     </c>
///     (OrdinalIgnoreCase over the member name and its [EnumMember] value). This pins every tier-1 inbound member to the
///     exact server wire name, so a rename or a missing [EnumMember] fails the build's tests.
/// </summary>
public class MessageTypeWireNameTests
{
    /// <summary>
    ///     EnumHelper builds a case-insensitive value lookup that throws on a duplicate key; a successful parse proves
    ///     initialisation did not throw - i.e. no two members collide on name or [EnumMember] value.
    /// </summary>
    [Test]
    public void MessageTypeLookupHasNoDuplicateKeys()
        => EnumHelper.TryParse("entities", out ALSocketMessageType _)
                     .Should()
                     .BeTrue();

    [Test]
    [Arguments("cm", ALSocketMessageType.Cm)]
    [Arguments("magiport", ALSocketMessageType.Magiport)]
    [Arguments("request", ALSocketMessageType.Request)]
    [Arguments("track", ALSocketMessageType.Track)]
    [Arguments("tracker", ALSocketMessageType.Tracker)]
    [Arguments("lostandfound", ALSocketMessageType.LostAndFound)]
    [Arguments("kill_credit", ALSocketMessageType.KillCredit)]
    [Arguments("trade_history", ALSocketMessageType.TradeHistory)]
    [Arguments("game_event", ALSocketMessageType.GameEvent)]
    [Arguments("chat_log", ALSocketMessageType.ChatLog)]
    [Arguments("pm", ALSocketMessageType.Pm)]
    public void WireNameParsesToMessageType(string wireName, ALSocketMessageType expected)
    {
        var parsed = EnumHelper.TryParse(wireName, out ALSocketMessageType actual);

        parsed.Should()
              .BeTrue($"'{wireName}' did not parse to an ALSocketMessageType");

        actual.Should()
              .Be(expected);
    }
}