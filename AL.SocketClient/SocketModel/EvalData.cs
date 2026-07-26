#region
using System.Text.Json.Serialization;
using AL.Core.Json.Attributes;
using AL.Core.Json.Interfaces;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents eval data.
///     <br />
///     The original language Adventure Land is meant for is JavaScript.
///     <br />
///     Javascript allows lazy evaluation of code with a simple eval command, but it's a lot more complicated to do in C#.
/// </summary>
/// <remarks>
///     The server sends this as a bare string on several paths, not always as an object.
/// </remarks>
[JsonStringOrObject(nameof(Code))]
public sealed record EvalData : IOptionalObject
{
    /// <summary>
    ///     The code to be eval'd and executed.
    ///     <br />
    /// </summary>
    public string? Code { get; set; } = null!;

    /// <inheritdoc />
    [JsonIgnore]
    public bool ContainsData { get; set; }
}