namespace AL.Data.Events
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class EventsDatum : DatumBase<GEvent>
    {
        public GEvent Abtesting { get; set; } = null!;
        public GEvent Goobrawl { get; set; } = null!;
    }
}