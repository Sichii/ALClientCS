#nullable disable

namespace AL.Data.Games
{
    public record Wheel
    {
        public int Gold { get; init; }
        public Slice[] Slices { get; init; }
    }
}