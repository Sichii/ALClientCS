namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents the data received when getting a party invite.
    /// </summary>
    public record InviteData
    {
        /// <summary>
        ///     The name of the person that invited you to their party.
        /// </summary>
        public string Name { get; init; } = null!;
    }
}