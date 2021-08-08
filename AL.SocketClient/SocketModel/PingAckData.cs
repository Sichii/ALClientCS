namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents data received when getting a response to a ping.
    /// </summary>
    public record PingAckData
    {
        /// <summary>
        ///     The id of the ping being responded to.
        /// </summary>
        public string Id { get; init; } = null!;
    }
}