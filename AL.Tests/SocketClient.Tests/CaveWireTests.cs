#region
using AL.Core.Definitions;
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     The small wire changes the daily dungeon brought, each pinned to the exact spelling the game's client reads: a
///     locked stair refuses the transport with its own reason, the dungeon's chests carry their own chest type, and the
///     entry animation rides the ui event.
/// </summary>
public class CaveWireTests
{
    [Test]
    public void ACaveChestDropParsesItsChestType()
    {
        var data = TestJson.Socket<DropData>(
            @"{""chest"":""cavechest"",""id"":""abc"",""x"":1,""y"":2,""map"":""main"",""items"":1,""owners"":[""a""]}");

        data.Should()
            .NotBeNull();

        data.ChestType
            .Should()
            .Be(ChestType.CaveChest);
    }

    [Test]
    public void ALockedStairRefusalParsesAsSealClosed()
    {
        var data = TestJson.Socket<GameResponseData>(@"{""response"":""seal_closed"",""place"":""transport"",""failed"":true}");

        data.Should()
            .NotBeNull();

        data.ResponseType
            .Should()
            .Be(GameResponseType.SealClosed);

        data.Failed
            .Should()
            .BeTrue();
    }

    [Test]
    public void TheEntryAnimationFrameParsesItsUiType()
    {
        var data = TestJson.Socket<UIData>(@"{""type"":""cave_enter"",""key"":""k"",""names"":[""a""],""duration"":1800}");

        data.Should()
            .NotBeNull();

        data.UIDataType
            .Should()
            .Be(UIDataType.CaveEnter);
    }
}