#region
using AL.Data.Events;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

public class GEventDoorsTests
{
    [Test]
    public void DoorsTest()
    {
        //the head of events.trickortreat as the live game data carries it
        const string TRICK_OR_TREAT = """
                                      {
                                        "name": "Trick or Treat",
                                        "type": "seasonal",
                                        "doors": [["halloween", 331, -210], ["spookytown", -130, -60]],
                                        "knocks": 14
                                      }
                                      """;

        var trickOrTreat = TestJson.Data<GEvent>(TRICK_OR_TREAT)!;

        trickOrTreat.Doors
                    .Should()
                    .Equal(new GTrickOrTreatDoor("halloween", 331, -210), new GTrickOrTreatDoor("spookytown", -130, -60));
    }
}