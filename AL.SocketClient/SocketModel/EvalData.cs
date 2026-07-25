#region
using AL.Core.Json.Converters;
using AL.Core.Json.Interfaces;
using Newtonsoft.Json;
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
[JsonConverter(typeof(StringOrObjectConverter<EvalData>), nameof(Code))]
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
