using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AL.Pathfinding.Definitions
{
    [Flags, JsonConverter(typeof(StringEnumConverter))]
    public enum PointType : byte
    {
        None,
        Wall = 1,
        Walkable = 1 << 1,
        Inline = (1 << 2) | Walkable,
        Vertex = (1 << 3) | Inline,
        Discovered = (1 << 4) | Vertex
    }

    [Flags, JsonConverter(typeof(StringEnumConverter))]
    public enum EdgeType : byte
    {
        Walk = 1,
        Town = 1 << 1,
        Transport = 1 << 2,
        Door = 1 << 3,
        Leave = 1 << 4
    }
}