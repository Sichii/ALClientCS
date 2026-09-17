#region
using AL.Client.Helpers;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     A generated map arrives as a run of map_chunk frames the client joins back into one JSON bundle. The game's own
///     client restates the rule: chunks land in index order under one run id, and anything out of order or from another
///     run throws the pending bundle away, so a stale half-bundle can never be glued onto a fresh one.
/// </summary>
public class MapChunkAssemblerTests
{
    private const string RUN = "6f1e2d3c4b5a69788796a5b4";

    [Test]
    public void AChunkFromAnotherRunDropsThePendingBundle()
    {
        var assembler = new MapChunkAssembler();
        assembler.Add(Chunk(0, 2, "{\"run\":"));

        var act = () => assembler.Add(Chunk(1, 2, "\"x\"}", "0000000000000000000000ab"));

        act.Should()
           .Throw<InvalidOperationException>();

        //the pending half is gone, so the run has to start again from its first chunk
        assembler.Add(Chunk(0, 2, "{\"run\":"))
                 .Should()
                 .BeNull();

        assembler.Add(Chunk(1, 2, "\"x\"}"))
                 .Should()
                 .Be("{\"run\":\"x\"}");
    }

    [Test]
    public void AChunkOutOfOrderDropsThePendingBundle()
    {
        var assembler = new MapChunkAssembler();
        assembler.Add(Chunk(0, 3, "a"));

        var act = () => assembler.Add(Chunk(2, 3, "c"));

        act.Should()
           .Throw<InvalidOperationException>();

        //the chunk that would have completed the old bundle is now an orphan
        var orphan = () => assembler.Add(Chunk(1, 3, "b"));

        orphan.Should()
              .Throw<InvalidOperationException>();
    }

    [Test]
    public void ACountPastTheCapIsRefused()
    {
        var assembler = new MapChunkAssembler();

        var act = () => assembler.Add(Chunk(0, 1401, "a"));

        act.Should()
           .Throw<InvalidOperationException>();
    }

    [Test]
    public void ChunksInOrderYieldTheBundleTextOnTheLastOne()
    {
        var assembler = new MapChunkAssembler();

        assembler.Add(Chunk(0, 3, "{\"run\":\""))
                 .Should()
                 .BeNull();

        assembler.Add(Chunk(1, 3, RUN))
                 .Should()
                 .BeNull();

        assembler.Add(Chunk(2, 3, "\"}"))
                 .Should()
                 .Be($"{{\"run\":\"{RUN}\"}}");
    }

    private static MapChunkData Chunk(
        int index,
        int count,
        string text,
        string run = RUN)
        => new()
        {
            Run = run,
            Index = index,
            Count = count,
            Text = text
        };
}
