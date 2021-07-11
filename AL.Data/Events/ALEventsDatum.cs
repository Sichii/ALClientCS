namespace AL.Data.Events
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class ALEventsDatum : DatumBase<ALEvent>
    {
        public ALEvent ABTesting { get; set; } = null!;
        public ALEvent GooBrawl { get; set; } = null!;
    }
}