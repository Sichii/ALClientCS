using System.Collections.Generic;
using AL.Core.Definitions;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents the data received for UI related things.
    ///     This data is generally unimportant.
    /// </summary>
    public class UIData
    {
        public Direction? Direction { get; init; }
        public string? From { get; init; }
        public string? Id { get; init; }
        public IReadOnlyList<string>? Ids { get; init; }
        public SimpleItem? Item { get; init; }
        public string? Name { get; init; }
        public string? To { get; init; }
        [JsonProperty("type")]
        public UIDataType UIDataType { get; init; }
    }
}