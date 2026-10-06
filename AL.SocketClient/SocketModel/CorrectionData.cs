#region
using System.Text.Json.Serialization;
using AL.Core.Interfaces;
#endregion

namespace AL.SocketClient.SocketModel;

public record CorrectionData : IPoint
{
    /// <summary>
    ///     If populated, the generated cave run this correction belongs to. A correction inside the current run stops the
    ///     character instead of letting it carry on along its path.
    /// </summary>
    [JsonPropertyName("cave")]
    public string? Cave { get; init; }

    public float X { get; init; }
    public float Y { get; init; }
    public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);
}
