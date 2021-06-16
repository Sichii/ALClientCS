using AL.SocketClient.Definitions;

namespace AL.SocketClient.Receive
{
    public record UpgradeData
    {
        public bool Success { get; init; }
        public UpgradeType UpgradeType { get; init; }
    }
}