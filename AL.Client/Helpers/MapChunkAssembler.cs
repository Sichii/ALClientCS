#region
using System.Text.RegularExpressions;
using AL.SocketClient.SocketModel;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Represents a run of <c>map_chunk</c> frames being joined back into the one JSON bundle the server split. A bundle
///     starts at index zero, every later chunk carries the same run and count and the next index, and anything else
///     discards the pending pieces.
/// </summary>
internal sealed partial class MapChunkAssembler
{
    private int Count;
    private List<string>? Parts;
    private string? Run;
    private int Size;

    /// <summary>Adds one chunk to the pending bundle.</summary>
    /// <param name="chunk">The chunk as the server sent it.</param>
    /// <returns>
    ///     The joined bundle text when this chunk completes it, or null while more are due.
    /// </returns>
    /// <exception cref="ArgumentNullException">chunk</exception>
    /// <exception cref="InvalidOperationException">
    ///     The chunk is malformed, or it does not follow the pending bundle - either way the pending bundle is discarded.
    /// </exception>
    public string? Add(MapChunkData chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        //the game client's own caps, so a malformed stream cannot grow the buffer without bound
        const int MAX_COUNT = 1400;
        const int MAX_TEXT_LENGTH = 12_000;
        const int MAX_BUNDLE_LENGTH = 16 * 1024 * 1024;

        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (chunk.Run is null
            || !RunPattern()
                .IsMatch(chunk.Run)
            || chunk.Count is < 1 or > MAX_COUNT
            || chunk.Text is null
            || (chunk.Text.Length > MAX_TEXT_LENGTH))
        {
            Reset();

            throw new InvalidOperationException($"Invalid map chunk {chunk.Index}/{chunk.Count} for run {chunk.Run}.");
        }

        // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

        if (chunk.Index == 0)
        {
            Run = chunk.Run;
            Count = chunk.Count;
            Parts = [];
            Size = 0;
        }

        if (Parts is null || (chunk.Run != Run) || (chunk.Count != Count) || (chunk.Index != Parts.Count))
        {
            Reset();

            throw new InvalidOperationException($"Out-of-order map chunk {chunk.Index}/{chunk.Count} for run {chunk.Run}.");
        }

        Parts.Add(chunk.Text);
        Size += chunk.Text.Length;

        if (Size > MAX_BUNDLE_LENGTH)
        {
            Reset();

            throw new InvalidOperationException($"Oversized generated map bundle for run {chunk.Run}.");
        }

        if (Parts.Count != Count)
            return null;

        var text = string.Concat(Parts);
        Reset();

        return text;
    }

    private void Reset()
    {
        Run = null;
        Count = 0;
        Parts = null;
        Size = 0;
    }

    [GeneratedRegex("^[a-f0-9]{24}$")]
    private static partial Regex RunPattern();
}