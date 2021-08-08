namespace AL.Core.Interfaces
{
    /// <summary>
    ///     Provides
    /// </summary>
    public interface IBounding
    {
        float HalfWidth { get; }
        float VerticalNorth { get; }
        float VerticalNotNorth { get; }
    }
}