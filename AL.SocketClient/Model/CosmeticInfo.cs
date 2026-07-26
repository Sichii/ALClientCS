namespace AL.SocketClient.Model;

/// <summary>
///     Represents the appearance data for a <see cref="Player" />.
/// </summary>
public sealed class CosmeticInfo
{
    public string Chin { get; init; } = null!;

    public string Face { get; init; } = null!;

    public string Hair { get; init; } = null!;

    public string Hat { get; init; } = null!;

    public string Head { get; init; } = null!;

    public string Makeup { get; init; } = null!;

    public string Upper { get; init; } = null!;
}