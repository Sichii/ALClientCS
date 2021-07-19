namespace AL.Data.Titles
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class TitlesDatum : DatumBase<GTitle>
    {
        public GTitle CritMonger { get; set; } = null!;
        public GTitle Fast { get; set; } = null!;
        public GTitle Festive { get; set; } = null!;
        public GTitle FireHazard { get; set; } = null!;
        public GTitle Glitched { get; set; } = null!;
        public GTitle Gooped { get; set; } = null!;
        public GTitle Legacy { get; set; } = null!;
        public GTitle Lucky { get; set; } = null!;
        public GTitle Shiny { get; set; } = null!;
        public GTitle Sniper { get; set; } = null!;
        public GTitle Stomped { get; set; } = null!;
        public GTitle SuperFast { get; set; } = null!;
    }
}