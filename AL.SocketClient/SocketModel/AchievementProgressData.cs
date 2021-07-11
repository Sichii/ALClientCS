namespace AL.SocketClient.SocketModel
{
    public record AchievementProgressData
    {
        public int Count { get; set; }
        public string Name { get; set; }
        public int Needed { get; set; }
    }
}