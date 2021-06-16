namespace AL.SocketClient.Model
{
    public record PlayerUpdateEvent
    {
        public int Level { get; init; }
        public string Name { get; init; }
        public int Num { get; init; }
        public string Response { get; init; }
    }
}