using AL.Core.Json.Attributes;
using AL.SocketClient.Definitions;

namespace AL.SocketClient.ClientModel
{
    public record ALSocketEmit<T>(
        [property: JsonArrayIndex(0)] ALSocketEmitType ALSocketEmitType,
        [property: JsonArrayIndex(1)] T Data);
}