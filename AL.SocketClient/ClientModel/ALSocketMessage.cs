#region
using AL.Core.Json.Attributes;
using AL.SocketClient.Definitions;
using Newtonsoft.Json.Linq;
#endregion

namespace AL.SocketClient.ClientModel;

public sealed record ALSocketMessage(
    [property: JsonArrayIndex(0)]
    ALSocketMessageType MessageType,
    [property: JsonArrayIndex(1)]
    JToken Data);