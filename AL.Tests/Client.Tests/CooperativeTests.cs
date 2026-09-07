#region
using AL.Client.Extensions;
using AL.SocketClient.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     Whether a monster in view is cooperative. The server sends the flag only when the instance differs from its
///     definition, so a boss that is cooperative by definition arrives without it, and reading the raw field reports
///     every such boss as ordinary.
/// </summary>
public class CooperativeTests : GameDataTestBed
{
    [Test]
    public void ABossCooperativeByDefinitionIsCooperativeWithoutTheFlag()
    {
        var franky = TestJson.Socket<Monster>("""{"id":"4142818","type":"franky","x":0,"y":0}""")!;

        franky.IsCooperative()
              .Should()
              .BeTrue();
    }

    [Test]
    public void AnOrdinaryMonsterIsNotCooperativeWithoutTheFlag()
    {
        var goo = TestJson.Socket<Monster>("""{"id":"1","type":"goo","x":0,"y":0}""")!;

        goo.IsCooperative()
           .Should()
           .BeFalse();
    }

    [Test]
    public void AFlagOnTheWireWinsOverTheDefinition()
    {
        var goo = TestJson.Socket<Monster>("""{"id":"1","type":"goo","x":0,"y":0,"cooperative":true}""")!;

        goo.IsCooperative()
           .Should()
           .BeTrue();
    }
}
