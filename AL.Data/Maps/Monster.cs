using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Maps
{
    public record Monster
    {
        [JsonProperty("boundaries", ItemConverterType = typeof(ArrayToRectangleConverter))]
        private Rectangle[] _boundaries;

        [JsonProperty("boundary"), JsonConverter(typeof(ArrayToRectangleConverter))]
        private Rectangle _boundary;

        //polygon
        public int Count { get; init; }
        public bool GateKeeper { get; init; }
        public bool Grow { get; init; }

        [JsonProperty("type")]
        public string MonsterType { get; init; }

        public int Radius { get; init; }

        [JsonProperty("rage"), JsonConverter(typeof(ArrayToRectangleConverter))]
        public Rectangle RageBoundary { get; init; }

        public bool Roam { get; init; }
        public SpawnType SpawnType { get; init; }
        public bool Special { get; init; }

        [JsonIgnore]
        public IEnumerable<Rectangle> Boundaries
        {
            get
            {
                IEnumerable<Rectangle> GetBoundaries()
                {
                    if (_boundary != null)
                        yield return _boundary;

                    if (_boundaries != null)
                        foreach (var boundary in _boundaries)
                            yield return boundary;
                }

                return GetBoundaries();
            }
        }

        //position
    }
}