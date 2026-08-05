#region
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     The two chat payloads. Both bind by exact member name case-insensitively, so the only field that could
///     silently miss is the one whose wire name is not its member name - and that is <c>p</c>, the flag separating
///     player chat from the pvp arena, the Grinch and cyberland's mainframe. A false reading there is invisible:
///     a consumer keeping only player chat would show nothing at all and look exactly like a quiet server.
/// </summary>
public class ChatDataTests
{
    [Test]
    public void PlayerChatCarriesTheServersOwnPublicFlag()
    {
        //server.js:4638 - broadcast, which is a bare io.emit, so this reaches every socket on the server
        const string CHAT = @"{ ""owner"":""Trader"", ""message"":""wts firebow"", ""id"":""7"", ""p"":true }";

        var obj = TestJson.Socket<ChatLogData>(CHAT);

        obj.Should()
           .NotBeNull();

        obj.IsPlayerChat
           .Should()
           .BeTrue("'p' is the wire name for it, and nothing else distinguishes a player's line");

        obj.Owner
           .Should()
           .Be("Trader");

        obj.Message
           .Should()
           .Be("wts firebow");

        obj.Color
           .Should()
           .BeNull("player chat carries no colour");
    }

    /// <summary>
    ///     The same event carries NPC and monster chatter through xy_emit, which is local rather than server-wide and
    ///     which nobody can answer. server_functions.js:2252 is the Grinch, and it is the one that carries a colour.
    /// </summary>
    [Test]
    public void NpcChatterOmitsThePublicFlagEntirely()
    {
        const string CHAT = @"{ ""owner"":""Grinch"", ""message"":""ho ho ho"", ""id"":""m1"", ""color"":""#418343"" }";

        var obj = TestJson.Socket<ChatLogData>(CHAT);

        obj.Should()
           .NotBeNull();

        obj.IsPlayerChat
           .Should()
           .BeFalse("an absent 'p' must not read as player chat");

        obj.Color
           .Should()
           .Be("#418343");
    }

    /// <summary>
    ///     server.js:4601-4602 emits a whisper to both ends, and 'to' is populated only on the sender's copy - so it
    ///     is the only thing that identifies a message this character sent rather than received.
    /// </summary>
    [Test]
    public void OnlyTheSendersCopyOfAWhisperCarriesTheRecipient()
    {
        const string SENT = @"{ ""owner"":""Sichi"", ""to"":""Trader"", ""message"":""2m ok?"", ""id"":""1"" }";
        const string RECEIVED = @"{ ""owner"":""Trader"", ""message"":""deal"", ""id"":""7"" }";

        var sent = TestJson.Socket<PmData>(SENT);
        var received = TestJson.Socket<PmData>(RECEIVED);

        sent.Should()
            .NotBeNull();

        received.Should()
                .NotBeNull();

        sent.To
            .Should()
            .Be("Trader");

        received.To
                .Should()
                .BeNull("the recipient's copy is the same event without an addressee");
    }

    /// <summary>
    ///     server.js:4562 - a cross-server whisper reaches the recipient with an extra 'xserver' key and the sender's
    ///     owner id in place of a character id. Nothing maps either, and a throw here would escape into
    ///     ALSocketClient.OnAny and discard the frame.
    /// </summary>
    [Test]
    public void ACrossServerWhisperDeserializesDespiteItsExtraKey()
    {
        const string PM = @"{ ""owner"":""Trader"", ""message"":""deal"", ""id"":""Trader"", ""xserver"":true }";

        var obj = TestJson.Socket<PmData>(PM);

        obj.Should()
           .NotBeNull();

        obj.Owner
           .Should()
           .Be("Trader");

        obj.To
           .Should()
           .BeNull();
    }
}
