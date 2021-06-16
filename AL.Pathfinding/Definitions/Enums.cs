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
        Walkable = 2,
        Inline = 4 | Walkable,
        Vertex = 8 | Inline,
        Discovered = 16
    }

    [Flags, JsonConverter(typeof(StringEnumConverter))]
    public enum ConnectorType : byte
    {
        Walk,
        Town,
        Transport,
        Leave
    }
}