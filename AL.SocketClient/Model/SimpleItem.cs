using AL.APIClient.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <inheritdoc cref="ISimpleItem" />
    /// <seealso cref="ISimpleItem" />
    public record SimpleItem : ISimpleItem
    {
        public string Name { get; init; } = null!;
        [JsonProperty("q")]
        public int Quantity { get; init; } = 1;
    }
}