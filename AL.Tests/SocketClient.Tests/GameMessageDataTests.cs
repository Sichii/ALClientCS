#region
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     The <c>game_log</c> and <c>game_error</c> payload. Both shapes have to bind: the server still sends a bare string
///     from the handful of sites it builds by hand, and an object from every site that goes through translation.
/// </summary>
public class GameMessageDataTests
{
    /// <summary>
    ///     A substitution can be a whole phrase reference rather than a string, which the cave's own lines are full of.
    ///     Narrowing the argument bag to strings does not lose the argument, it throws and drops the entire frame - so the
    ///     loose type here is the point of the test.
    /// </summary>
    [Test]
    public void APhraseArgumentMayBeAPhraseOfItsOwn()
    {
        const string FRAME = """
                             {
                               "color": "#D4BB88",
                               "message": "The shop opens. You can inspect the item and pay with cave gold if you want it.",
                               "phrase": "event.dreams.e31.options.e31_0.result",
                               "phrase_args": { "npc": "Ilex", "rival": { "phrase": "server.cave.rival" } }
                             }
                             """;

        var data = TestJson.Socket<GameMessageData>(FRAME)!;

        data.Phrase
            .Should()
            .Be("event.dreams.e31.options.e31_0.result");

        data.PhraseArgs!["npc"]!.GetValue<string>()
            .Should()
            .Be("Ilex");

        data.PhraseArgs["rival"]!["phrase"]!.GetValue<string>()
            .Should()
            .Be("server.cave.rival");
    }

    /// <summary>
    ///     The sites the server has not translated still send a bare string, and a matcher has only the text there.
    /// </summary>
    [Test]
    public void ABareStringStillBindsToTheMessage()
    {
        var data = TestJson.Socket<GameMessageData>("\"Wrong passphrase!\"")!;

        data.Message
            .Should()
            .Be("Wrong passphrase!");

        data.Phrase
            .Should()
            .BeNull();
    }
}