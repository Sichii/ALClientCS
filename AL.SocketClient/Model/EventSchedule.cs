#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     When a server runs its scheduled events. Sent whole inside the server info snapshot, on login and on every
///     change (<c>node/server.js:10581</c>, <c>node/server_functions.js:2510</c>).
/// </summary>
/// <remarks>
///     Which event lands in a slot is not published: the server rotates a list it shuffled at start-up and holds in
///     module scope, so a client can only know when a slot comes round, never what will be in it.
/// </remarks>
public sealed record EventSchedule
{
    /// <summary>
    ///     Whole hours the server's own clock runs ahead of UTC — per region, +1 EU, -5 US, +7 ASIA
    ///     (<c>node/server.js:235</c>). Every hour below is on that clock.
    /// </summary>
    [JsonPropertyName("time_offset")]
    public int TimeOffset { get; init; }

    /// <summary>The hours a daily event starts at.</summary>
    public IReadOnlyList<int> Dailies { get; init; } = [];

    /// <summary>The hours a nightly event starts at.</summary>
    public IReadOnlyList<int> Nightlies { get; init; } = [];

    /// <summary>Whether the server clock currently reads night, which is what gates the nightly monsters.</summary>
    public bool Night { get; init; }
}
