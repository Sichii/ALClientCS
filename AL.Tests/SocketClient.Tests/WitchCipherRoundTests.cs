#region
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

public class WitchCipherRoundTests
{
    private static readonly string[] INGREDIENTS =
    [
        "verdantspore",
        "frostscale",
        "embercoal",
        "gloomfeather"
    ];

    private static readonly WitchCipherClue CLUE = new()
    {
        Mix =
        [
            "verdantspore",
            "verdantspore",
            "frostscale",
            "embercoal",
            "gloomfeather"
        ],
        Right = 2
    };

    //recipe n puts ingredient (n / 4^i) % 4 in slot i, as the server enumerates them
    private static string[] Recipe(int n)
        => Enumerable.Range(0, 5)
                     .Select(slot => INGREDIENTS[n / (int)Math.Pow(4, slot) % 4])
                     .ToArray();

    [Test]
    public void FitBitsAgreeWithTheBoardTest()
    {
        var fitting = Enumerable.Range(0, 1024)
                                .Where(n => Recipe(n)
                                            .Zip(CLUE.Mix)
                                            .Count(pair => pair.First == pair.Second)
                                            == CLUE.Right)
                                .ToHashSet();

        //bit n & 3 of hex digit n >> 2
        var fit = string.Concat(
            Enumerable.Range(0, 256)
                      .Select(digit => Enumerable.Range(0, 4)
                                                 .Sum(bit => fitting.Contains(digit * 4 + bit) ? 1 << bit : 0)
                                                 .ToString("x")));

        var withFit = new WitchCipherRound
        {
            Board = [CLUE],
            Mixes = 1,
            Fit = fit
        };

        var boardOnly = withFit with
        {
            Fit = null
        };

        foreach (var n in Enumerable.Range(0, 1024))
        {
            withFit.Fits(INGREDIENTS, Recipe(n))
                   .Should()
                   .Be(fitting.Contains(n));

            boardOnly.Fits(INGREDIENTS, Recipe(n))
                     .Should()
                     .Be(fitting.Contains(n));
        }
    }

    [Test]
    public void TriedAndUnknownTest()
    {
        var round = new WitchCipherRound
        {
            Board = [CLUE],
            Mixes = 1
        };

        round.Fits(INGREDIENTS, CLUE.Mix)
             .Should()
             .BeFalse();

        //the board dropped older clues and no fit bits came with it
        (round with
            {
                Mixes = 31
            }).Fits(INGREDIENTS, Recipe(0))
              .Should()
              .BeNull();
    }

    [Test]
    public void PhaseTest()
    {
        var start = new DateTime(
            2026,
            10,
            25,
            18,
            0,
            0,
            DateTimeKind.Utc);
        var ms = (DateTime time) => new DateTimeOffset(time).ToUnixTimeMilliseconds();

        var round = new WitchCipherRound
        {
            Live = true,
            GatherEnd = ms(start.AddMinutes(20)),
            End = ms(start.AddMinutes(28))
        };

        round.IsGathering(start.AddMinutes(5))
             .Should()
             .BeTrue();

        round.IsBrewing(start.AddMinutes(5))
             .Should()
             .BeFalse();

        round.IsBrewing(start.AddMinutes(25))
             .Should()
             .BeTrue();

        round.IsBrewing(start.AddMinutes(29))
             .Should()
             .BeFalse();
    }

    [Test]
    public void DeserializeTest()
    {
        const string SERVER_INFO = """
                                   {
                                     "witchcipher": {
                                       "live": true, "phase": "brewing", "id": "w1", "start": 1, "gather_end": 2, "end": 3,
                                       "board": [{"mix": ["frostscale","frostscale","frostscale","frostscale","frostscale"], "right": 1}],
                                       "possible": 405, "mixes": 1, "cracks": 0, "fit": "ff"
                                     }
                                   }
                                   """;

        var info = TestJson.Socket<EventAndBossData>(SERVER_INFO)!;

        info.WitchCipher!.Board
            .Should()
            .ContainSingle()
            .Which
            .Right
            .Should()
            .Be(1);

        info.WitchCipher
            .Fit
            .Should()
            .Be("ff");

        info.WitchCipher
            .End
            .Should()
            .Be(3);
    }
}