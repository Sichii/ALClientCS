#region
using AL.SocketClient.Json.Converters;
using AL.SocketClient.Model;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.SocketModel;

/// <inheritdoc cref="AL.SocketClient.Model.EventAndBossInfo" />
/// <seealso cref="AL.SocketClient.Model.EventAndBossInfo" />
[JsonConverter(typeof(EventAndBossDataConverter))]
public sealed record EventAndBossData : EventAndBossInfo;