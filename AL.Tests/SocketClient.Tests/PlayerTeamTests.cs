#region
using AL.SocketClient.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     Covers <see cref="Player.Team" />: the A/B Testing and duel "which side" field, sent as a plain string on the
///     wire and cleared server-side the moment a player leaves the instance.
/// </summary>
public class PlayerTeamTests
{
    [Test]
    public void ATeamFieldOnTheWireIsCaptured()
    {
        var player = TestJson.Socket<Player>(@"{ ""id"":""a"", ""team"":""A"" }");

        player.Should().NotBeNull();

        player!.Team
              .Should()
              .Be("A");
    }

    [Test]
    public void NoTeamFieldOnTheWireLeavesItNull()
    {
        var player = TestJson.Socket<Player>(@"{ ""id"":""a"" }");

        player.Should().NotBeNull();

        player!.Team
              .Should()
              .BeNull();
    }

    [Test]
    public void UpdateClearsTeamWhenALaterFrameOmitsIt()
    {
        var tracked = TestJson.Socket<Player>(@"{ ""id"":""a"", ""team"":""A"" }")!;
        var later = TestJson.Socket<Player>(@"{ ""id"":""a"" }")!;

        tracked.Update(later);

        tracked.Team
               .Should()
               .BeNull();
    }
}
