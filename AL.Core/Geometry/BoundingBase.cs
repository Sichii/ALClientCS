// ReSharper disable InvalidXmlDocComment

using AL.Core.Interfaces;

namespace AL.Core.Geometry
{
    /// <summary>
    ///     Represents the measurements of a bounding box.
    /// </summary>
    /// <param name="HalfWidth">Half of the width of the bounding box.</param>
    /// <param name="VerticalNorth">The distance between the center and top of the bounding box.</param>
    /// <param name="VerticalNotNorth">The distance between the center and bottom of the bounding box.</param>
    public record BoundingBase(float HalfWidth, float VerticalNorth, float VerticalNotNorth) : IBounding;
}