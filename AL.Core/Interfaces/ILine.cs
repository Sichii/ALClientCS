namespace AL.Core.Interfaces
{
    public interface ILine
    {
        public IPoint Point1 { get; }
        public IPoint Point2 { get; }
        public float Length { get; }
    }
}