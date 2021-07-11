#nullable disable

namespace AL.Data.Games
{
    public record Slots
    {
        public string[] Glyphs { get; init; }
        public int Gold { get; init; }
    }
}