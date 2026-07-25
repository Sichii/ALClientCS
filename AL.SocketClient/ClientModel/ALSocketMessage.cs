#region
using System.Text.Json.Nodes;
using AL.Core.Json.Attributes;
using AL.SocketClient.Definitions;
#endregion

namespace AL.SocketClient.ClientModel;

public sealed record ALSocketMessage(
    [property: JsonArrayIndex(0)]
    ALSocketMessageType MessageType,
    [property: JsonArrayIndex(1)]
    JsonNode Data);