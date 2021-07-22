using System.Collections.Generic;
using AL.Core.Geometry;
using AL.Core.Json.Converters;
using AL.Data.NPCs;
using Newtonsoft.Json;

namespace AL.Data.Maps
{
    /// <summary>
    ///     Represents an NPC's static data for a specific map.
    /// </summary>
    public record GMapNPC
    {
        [JsonProperty("name")]
        private string? _name;
        [JsonProperty("position"), JsonConverter(typeof(ArrayToObjectConverter<Orientation>))]
        private Orientation? _position;

        [JsonProperty("positions", ItemConverterType = typeof(ArrayToObjectConverter<Orientation>))]
        private IReadOnlyList<Orientation>? _positions;

        /// <summary>
        ///     <b>NULLABLE</b>. If populated, this is the area in which this NPC roams.
        /// </summary>
        [JsonConverter(typeof(BoundaryConverter))]
        public Boundary? Boundary { get; init; }

        /// <summary>
        ///     This NPC's data from <see cref="GameData.NPCs" />.
        /// </summary>
        /// <remarks>Enriched property</remarks>
        [JsonIgnore]
        public GNPC? Data { get; internal set; }

        /// <summary>
        ///     The id/accessor of this NPC.
        /// </summary>
        public string Id { get; init; } = null!;

        /// <summary>
        ///     Whether or not this NPC walks in a loop between it's possible positions.
        /// </summary>
        public bool Loop { get; init; }

        /// <summary>
        ///     The name of this NPC as displayed on the GUI. Sometimes different than the Id.
        /// </summary>
        public string Name => _name ?? Id;

        /// <summary>
        ///     Lazy enumeration of the spots this NPC can be at. <br />
        ///     If you're familiar with the original form of this data, it's _position(if present) + _positions(if present).
        /// </summary>
        /// <remarks>Aggregate property</remarks>
        [JsonIgnore]
        public IEnumerable<Orientation> Positions
        {
            get
            {
                IEnumerable<Orientation> GetPositions()
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