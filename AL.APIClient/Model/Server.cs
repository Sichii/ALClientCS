#region
using System.Text.Json.Serialization;
using AL.APIClient.Definitions;
#endregion

namespace AL.APIClient.Model;

/// <summary>
///     Represents a server that a character could log into.
/// </summary>
public sealed record Server
{
    /// <summary>
    ///     The host name of the server. There is no separate port; it is part of the host if non-standard.
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; init; } = null!;

    /// <summary>
    ///     The server identifier.
    /// </summary>
    [JsonPropertyName("name")]
    public ServerId Identifier { get; set; }

    /// <summary>
    ///     The server's "SR_" prefixed unique id.
    /// </summary>
    public string Key { get; init; } = null!;

    /// <summary>
    ///     The engine.io mount path this server listens on. Configured per server, so it is not always "/socket.io/".
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; init; } = null!;

    /// <summary>
    ///     The number of players currently on this server.
    /// </summary>
    public int Players { get; init; }

    /// <summary>
    ///     The region this server is for.
    /// </summary>
    public ServerRegion Region { get; set; }
}