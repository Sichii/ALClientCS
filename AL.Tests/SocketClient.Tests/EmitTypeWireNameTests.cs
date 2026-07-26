#region
using AL.Core.Helpers;
using AL.SocketClient.Definitions;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     Phase 9 - emit surface coverage. <see cref="AL.SocketClient.ALSocketClient" /> sends an emit as
///     <c>
///         EnumHelper.ToString(emitType).ToLowerInvariant()
///     </c>
///     , so a member only reaches the right handler if that lowercased name equals the server's exact
///     <c>
///         socket.on
///     </c>
///     name. This pins every tier-1 member (and the previously-unreachable members that gained a method this phase) to its
///     handler in node/server.js.
/// </summary>
public class EmitTypeWireNameTests
{
    [Test]

    // tier 1 (new members)
    [Arguments(ALSocketEmitType.Say, "say")]
    [Arguments(ALSocketEmitType.SetHome, "set_home")]
    [Arguments(ALSocketEmitType.Mail, "mail")]
    [Arguments(ALSocketEmitType.Enter, "enter")]
    [Arguments(ALSocketEmitType.ExchangeBuy, "exchange_buy")]
    [Arguments(ALSocketEmitType.EquipBatch, "equip_batch")]
    [Arguments(ALSocketEmitType.LostAndFound, "lostandfound")]
    [Arguments(ALSocketEmitType.Split, "split")]
    [Arguments(ALSocketEmitType.Donate, "donate")]
    [Arguments(ALSocketEmitType.Destroy, "destroy")]
    [Arguments(ALSocketEmitType.JoinGiveaway, "join_giveaway")]
    [Arguments(ALSocketEmitType.TradeSell, "trade_sell")]
    [Arguments(ALSocketEmitType.TradeHistory, "trade_history")]
    [Arguments(ALSocketEmitType.Interaction, "interaction")]
    [Arguments(ALSocketEmitType.Friend, "friend")]
    [Arguments(ALSocketEmitType.Join, "join")]
    [Arguments(ALSocketEmitType.Bet, "bet")]
    [Arguments(ALSocketEmitType.Pet, "pet")]
    [Arguments(ALSocketEmitType.Whistle, "whistle")]
    [Arguments(ALSocketEmitType.Pets, "pets")]

    // existing members that gained a method this phase
    [Arguments(ALSocketEmitType.Command, "cm")]
    [Arguments(ALSocketEmitType.Emotion, "emotion")]
    [Arguments(ALSocketEmitType.Tracker, "tracker")]
    [Arguments(ALSocketEmitType.TakeMailItem, "mail_take_item")]
    [Arguments(ALSocketEmitType.Booster, "booster")]
    public void EmitTypeSerializesToServerHandlerName(ALSocketEmitType emitType, string wireName)
        => EnumHelper.ToString(emitType)
                     .ToLowerInvariant()
                     .Should()
                     .Be(wireName);
}