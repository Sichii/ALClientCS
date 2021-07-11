namespace AL.Core.Interfaces
{
    /// <summary>
    ///     Represents a line denoted by two points.
    /// </summary>
    public interface ILine
    {
        /// <summary>
        ///     The length of the line.
        /// </summary>
        float Length { get; }
        IPoint Point1 { get; }
        IPoint Point2 { get; }
    }
}