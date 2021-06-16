using System;
using AL.Core.Json.Converters;
using Chaos.Core.Extensions;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    [JsonConverter(typeof(AttributedObjectConverter<Monster>))]
    public record Monster : EntityBase
    {
        [JsonProperty("type")]
        public string Name { get; init; }

        public virtual bool Equals(Monster other) => Name.Equals(other?.Name) && base.Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Name.GetHashCode(), base.GetHashCode());
    }
}