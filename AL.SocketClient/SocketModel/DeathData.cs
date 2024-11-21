namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when an entity dies.
/// </summary>
public sealed record DeathData
{
    /// <summary>
    ///     The id of the entity that died.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    ///     If populated, the <see cref="ActionData.Source" /> of the <see cref="ActionData" /> that caused this death.
    /// </summary>
    public string? Place { get; init; }
}