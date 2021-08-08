namespace AL.Data.Titles
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class TitlesDatum : DatumBase<GTitle>
    {
        public GTitle Critmonger { get; init; } = null!;
        public GTitle Fast { get; init; } = null!;
        public GTitle Festive { get; init; } = null!;
        public GTitle Firehazard { get; init; } = null!;
        public GTitle Glitched { get; init; } = null!;
        public GTitle Gooped { get; init; } = null!;
        public GTitle Legacy { get; init; } = null!;
        public GTitle Lucky { get; init; } = null!;
        public GTitle Shiny { get; init; } = null!;
        public GTitle Sniper { get; init; } = null!;
        public GTitle Stomped { get; init; } = null!;
        public GTitle Superfast { get; init; } = null!;
    }
}