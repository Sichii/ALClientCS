namespace AL.Data.Achievements
{
    public record Achievement
    {
        public int Count { get; init; }
        public string Name { get; init; }
        public int Shells { get; init; }
        public string Title { get; init; }
    }
}