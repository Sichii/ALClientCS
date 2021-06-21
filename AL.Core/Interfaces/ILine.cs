namespace AL.Core.Interfaces
{
    public interface ILine
    {
        public float Length { get; }
        public IPoint Point1 { get; }
        public IPoint Point2 { get; }
    }
}