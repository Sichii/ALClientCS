#region
using AL.Core.Helpers;
using AL.SocketClient.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     Phase 9 - emit surface coverage. <see cref="ALSocketClient" /> sends an emit as
///     <c>EnumHelper.ToString(emitType).ToLowerInvariant()</c>, so a member only reaches the right handler if
///     that lowercased name equals the server's exact <c>socket.on</c> name. This pins every tier-1 member (and
///     the previously-unreachable members that gained a method this phase) to its handler in node/server.js.
/// </summary>
[TestClass]
public class EmitTypeWireNameTests
{
    [DataTestMethod]
    // tier 1 (new members)
    [DataRow(ALSocketEmitType.Say, "say")]
    [DataRow(ALSocketEmitType.SetHome, "set_home")]
    [DataRow(ALSocketEmitType.Mail, "mail")]
    [DataRow(ALSocketEmitType.Enter, "enter")]
    [DataRow(ALSocketEmitType.ExchangeBuy, "exchange_buy")]
    [DataRow(ALSocketEmitType.EquipBatch, "equip_batch")]
    [DataRow(ALSocketEmitType.LostAndFound, "lostandfound")]
    [DataRow(ALSocketEmitType.Split, "split")]
    [DataRow(ALSocketEmitType.Donate, "donate")]
    [DataRow(ALSocketEmitType.Destroy, "destroy")]
    [DataRow(ALSocketEmitType.JoinGiveaway, "join_giveaway")]
    [DataRow(ALSocketEmitType.TradeSell, "trade_sell")]
    [DataRow(ALSocketEmitType.TradeHistory, "trade_history")]
    [DataRow(ALSocketEmitType.Interaction, "interaction")]
    [DataRow(ALSocketEmitType.Friend, "friend")]
    [DataRow(ALSocketEmitType.Join, "join")]
    [DataRow(ALSocketEmitType.Bet, "bet")]
    [DataRow(ALSocketEmitType.Pet, "pet")]
    [DataRow(ALSocketEmitType.Whistle, "whistle")]
    [DataRow(ALSocketEmitType.Pets, "pets")]
    // existing members that gained a method this phase
    [DataRow(ALSocketEmitType.Command, "cm")]
    [DataRow(ALSocketEmitType.Emotion, "emotion")]
    [DataRow(ALSocketEmitType.Tracker, "tracker")]
    [DataRow(ALSocketEmitType.TakeMailItem, "mail_take_item")]
    [DataRow(ALSocketEmitType.Booster, "booster")]
    public void EmitTypeSerializesToServerHandlerName(ALSocketEmitType emitType, string wireName)
        => Assert.AreEqual(
            wireName,
            EnumHelper.ToString(emitType)
                      .ToLowerInvariant());
}
