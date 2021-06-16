namespace AL.Data.Games
{
    public record Tarot
    {
        public string[] Cards { get; init; }
        public int Hours { get; init; }
        public string NPC { get; init; }
    }
}