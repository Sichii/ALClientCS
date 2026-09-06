#region
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     The tavern frames. Four different payloads share the one
///     <c>
///         tavern
///     </c>
///     event name, so the message type cannot tell them apart and <see cref="TavernData.Event" /> is the only
///     discriminator there is. A correlation that resolves on the next frame instead hands the caller a bet broadcast
///     dressed as the house numbers - plausible values, wrong meaning, nothing to notice.
/// </summary>
public class TavernDataTests
{
    /// <summary>
    ///     The bet broadcast every player in the tavern receives (node/server.js:11566). Its
    ///     <c>
    ///         gold
    ///     </c>
    ///     and
    ///     <c>
    ///         num
    ///     </c>
    ///     read fine, which is exactly why it has to be rejected on the event name rather than on shape.
    /// </summary>
    [Test]
    public void BetFrameIsNotAnInfoFrame()
    {
        const string BET = @"{ ""event"":""bet"", ""name"":""Sichi"", ""type"":""dice"", ""num"":50, ""gold"":2000000, ""dir"":""down"" }";

        var obj = TestJson.Socket<TavernData>(BET);

        obj.Should()
           .NotBeNull();

        obj.Event
           .Should()
           .Be("bet", "a bet broadcast must never resolve an info request");

        obj.Number
           .Should()
           .Be(50f, "'num' is a renamed member, so nothing else catches a broken JsonPropertyName");

        obj.Gold
           .Should()
           .Be(2_000_000);

        obj.Max
           .Should()
           .Be(0, "a bet frame carries no house numbers at all");
    }

    /// <summary>
    ///     The frame that opens betting (node/server_functions.js:1404). It carries the commit hash and nothing about the roll
    ///     - the number it commits to is only revealed 40 seconds later, on the lock frame.
    /// </summary>
    [Test]
    public void BetsFrameCarriesTheCommitHashAndNoRoll()
    {
        const string BETS = @"{ ""state"":""bets"", ""hex"":""deadbeef"", ""algorithm"":""hmac-sha256"" }";

        var obj = TestJson.Socket<DiceData>(BETS);

        obj.Should()
           .NotBeNull();

        obj.Number
           .Should()
           .BeNull("nothing on this frame says what the roll will be");

        obj.Hex
           .Should()
           .Be("deadbeef");

        obj.Algorithm
           .Should()
           .Be("hmac-sha256");
    }

    /// <summary>
    ///     The reply to
    ///     <c>
    ///         tavern {event:"info"}
    ///     </c>
    ///     (node/server.js:11591). Both numbers come from
    ///     <c>
    ///         S.gold - house_debt()
    ///     </c>
    ///     , so
    ///     <c>
    ///         max
    ///     </c>
    ///     is a reading taken at that instant rather than a constant.
    /// </summary>
    [Test]
    public void InfoFrameCarriesEdgeAndMax()
    {
        const string INFO = @"{ ""event"":""info"", ""edge"":1.5, ""max"":420000000 }";

        var obj = TestJson.Socket<TavernData>(INFO);

        obj.Should()
           .NotBeNull();

        obj.Event
           .Should()
           .Be("info");

        obj.Edge
           .Should()
           .Be(1.5f);

        obj.Max
           .Should()
           .Be(420_000_000);
    }

    /// <summary>
    ///     The frame that reveals the roll (node/server_functions.js:1314-1319). The server builds
    ///     <c>
    ///         num
    ///     </c>
    ///     by concatenating digits around a decimal point, so it goes out as a string and only reads as a number because the
    ///     shared options coerce it.
    /// </summary>
    [Test]
    public void LockFrameCarriesTheRollAndTheReveal()
    {
        const string LOCK
            = @"{ ""state"":""lock"", ""num"":""42.13"", ""text"":""Num: 42.13 Initials: S Random: aB3dE5gH7i"", ""key"":""aB3dE5gH7iJ9kL1mN3oP"" }";

        var obj = TestJson.Socket<DiceData>(LOCK);

        obj.Should()
           .NotBeNull();

        obj.State
           .Should()
           .Be("lock");

        obj.Number
           .Should()
           .Be(42.13f, "the server sends the roll as a string, so a lost coercion reads as no roll at all");

        obj.Key
           .Should()
           .Be("aB3dE5gH7iJ9kL1mN3oP");
    }

    /// <summary>
    ///     The roulette handler echoes the raw bet record straight back (node/server.js:11511), and that record has no
    ///     <c>
    ///         event
    ///     </c>
    ///     at all. So the discriminator has to survive a null, which is why the comparison puts the literal on the left -
    ///     <c>
    ///         EqualsI
    ///     </c>
    ///     throws on a null receiver.
    /// </summary>
    [Test]
    public void RouletteEchoLeavesTheEventUnset()
    {
        const string ECHO = @"{ ""id"":""aB3dE5gH7i"", ""type"":""roulette"", ""odds"":""red"", ""gold"":100000, ""state"":""bet"" }";

        var obj = TestJson.Socket<TavernData>(ECHO);

        obj.Should()
           .NotBeNull();

        obj.Event
           .Should()
           .BeNull("the record carries a state where every other tavern frame carries an event");
    }

    /// <summary>
    ///     The win broadcast (node/server_functions.js:1340-1348).
    ///     <c>
    ///         gold
    ///     </c>
    ///     is the gross win here rather than a stake, and
    ///     <c>
    ///         net
    ///     </c>
    ///     is what the player actually gained.
    /// </summary>
    [Test]
    public void WonFrameCarriesNet()
    {
        const string WON
            = @"{ ""event"":""won"", ""name"":""Sichi"", ""type"":""dice"", ""num"":50, ""gold"":2000000, ""dir"":""down"", ""net"":980000 }";

        var obj = TestJson.Socket<TavernData>(WON);

        obj.Should()
           .NotBeNull();

        obj.Event
           .Should()
           .Be("won");

        obj.Net
           .Should()
           .Be(980_000);

        obj.Direction
           .Should()
           .Be("down");
    }
}