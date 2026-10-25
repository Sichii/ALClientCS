#region
using System.Globalization;
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents the Witch's Cipher round on this server, carried whole on every server info snapshot. Times are UTC unix
///     milliseconds.
/// </summary>
/// <remarks>
///     A round lasts 28 minutes: ingredients drop for the first 20, and mixes can be brewed until the end. When someone
///     cracks the recipe a new one starts on a clean board.
/// </remarks>
public sealed record WitchCipherRound
{
    /// <summary>
    ///     The latest clues on the shared board, oldest first. The server keeps only the last 30 here.
    /// </summary>
    [JsonPropertyName("board")]
    public IReadOnlyList<WitchCipherClue> Board { get; init; } = [];

    /// <summary>When the round ends.</summary>
    [JsonPropertyName("end")]
    public long End { get; init; }

    /// <summary>
    ///     If populated, one bit per possible recipe that fits every clue on the current recipe, including the clues the board
    ///     no longer shows: recipe <c>n</c> is bit <c>n &amp; 3</c> of hex digit <c>n &gt;&gt; 2</c>.
    /// </summary>
    [JsonPropertyName("fit")]
    public string? Fit { get; init; }

    /// <summary>When the gathering ends and the drops stop.</summary>
    [JsonPropertyName("gather_end")]
    public long GatherEnd { get; init; }

    /// <summary>Whether a round is running.</summary>
    [JsonPropertyName("live")]
    public bool Live { get; init; }

    /// <summary>
    ///     How many mixes the current recipe has been tried with, which can be more than <see cref="Board" /> shows.
    /// </summary>
    [JsonPropertyName("mixes")]
    public int Mixes { get; init; }

    /// <summary>Determines whether ingredients still drop.</summary>
    /// <param name="utcNow">The current time, in UTC.</param>
    /// <returns>
    ///     <c>true</c> if the round is live and gathering; otherwise, <c>false</c> .
    /// </returns>
    public bool IsGathering(DateTime utcNow) => Live && (ToUnixMs(utcNow) < GatherEnd) && (ToUnixMs(utcNow) < End);

    /// <summary>
    ///     Determines whether the round is past its gathering and still taking mixes.
    /// </summary>
    /// <param name="utcNow">The current time, in UTC.</param>
    /// <returns>
    ///     <c>true</c> if the round is brewing; otherwise, <c>false</c> .
    /// </returns>
    public bool IsBrewing(DateTime utcNow) => Live && (ToUnixMs(utcNow) >= GatherEnd) && (ToUnixMs(utcNow) < End);

    /// <summary>
    ///     Determines whether a mix fits every clue on the current recipe, which is what makes it pay a Witch's Parcel.
    /// </summary>
    /// <param name="ingredients">
    ///     The event's ingredient names, in <c>G.events.witchcipher.ingredients</c> order.
    /// </param>
    /// <param name="mix">The mix, one ingredient name per slot.</param>
    /// <returns>
    ///     <c>true</c> if it fits; <c>false</c> if it does not or was already tried; null when the server sent no
    ///     <see cref="Fit" /> and the board no longer holds every clue.
    /// </returns>
    /// <exception cref="ArgumentNullException">ingredients</exception>
    /// <exception cref="ArgumentNullException">mix</exception>
    public bool? Fits(IReadOnlyList<string> ingredients, IReadOnlyList<string> mix)
    {
        ArgumentNullException.ThrowIfNull(ingredients);

        ArgumentNullException.ThrowIfNull(mix);

        if (Board.Any(clue => clue.Mix.SequenceEqual(mix)))
            return false;

        if (Fit is { } fit)
        {
            var n = 0;

            for (var i = mix.Count - 1; i >= 0; i--)
            {
                var index = 0;

                while ((index < ingredients.Count) && (ingredients[index] != mix[i]))
                    index++;

                if (index == ingredients.Count)
                    return false;

                n = n * ingredients.Count + index;
            }

            if ((n >> 2) >= fit.Length)
                return false;

            var digit = int.Parse(fit.AsSpan(n >> 2, 1), NumberStyles.HexNumber);

            return ((digit >> (n & 3)) & 1) == 1;
        }

        if (Mixes > Board.Count)
            return null;

        return Board.All(clue => clue.Mix
                                     .Zip(mix)
                                     .Count(pair => pair.First == pair.Second)
                                 == clue.Right);
    }

    private static long ToUnixMs(DateTime utcNow) => new DateTimeOffset(utcNow, TimeSpan.Zero).ToUnixTimeMilliseconds();
}

/// <summary>
///     Represents one clue on the Witch's Cipher board: a mix and how many of its ingredients sit in the right place.
/// </summary>
public sealed record WitchCipherClue
{
    [JsonPropertyName("mix")]
    public IReadOnlyList<string> Mix { get; init; } = [];

    [JsonPropertyName("right")]
    public int Right { get; init; }
}