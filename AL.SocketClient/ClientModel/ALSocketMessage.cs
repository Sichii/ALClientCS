using AL.Core.Json.Attributes;
using AL.SocketClient.Definitions;
using Newtonsoft.Json.Linq;

namespace AL.SocketClient.ClientModel
{
    public record ALSocketMessage(
        [property: JsonArrayIndex(0)] ALSocketMessageType MessageType,
        [property: JsonArrayIndex(1)] JToken Data);
}