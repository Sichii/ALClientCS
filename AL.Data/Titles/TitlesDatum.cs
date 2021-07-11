namespace AL.Data.Titles
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class TitlesDatum : DatumBase<Title>
    {
        public Title CritMonger { get; set; } = null!;
        public Title Fast { get; set; } = null!;
        public Title Festive { get; set; } = null!;
        public Title FireHazard { get; set; } = null!;
        public Title Glitched { get; set; } = null!;
        public Title Gooped { get; set; } = null!;
        public Title Legacy { get; set; } = null!;
        public Title Lucky { get; set; } = null!;
        public Title Shiny { get; set; } = null!;
        public Title Sniper { get; set; } = null!;
        public Title Stomped { get; set; } = null!;
        public Title SuperFast { get; set; } = null!;
    }
}