using System.Collections.Generic;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Maps
{
    public record NPC
    {
        [JsonProperty("position"), JsonConverter(typeof(ArrayToObjectConverter<Orientation>))]
        private Orientation _position;

        [JsonProperty("positions", ItemConverterType = typeof(ArrayToObjectConverter<Orientation>))]
        private Orientation[] _positions;

        [JsonConverter(typeof(ArrayToRectangleConverter))]
        public Rectangle Boundary { get; init; }

        public string Id { get; init; }
        public bool Loop { get; init; }
        public string Name { get; init; }

        [JsonIgnore]
        public IEnumerable<IOriented> Positions
        {
            get
            {
                IEnumerable<IOriented> GetPositions()
                {
                    if (_position != default)
                        yield return _position;

                    if (_positions != null)
                        foreach (var position in _positions)
                            yield return position;
                }

                return GetPositions();
            }
        }
    }
}