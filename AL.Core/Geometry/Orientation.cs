using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Core.Json.Attributes;

// ReSharper disable InvalidXmlDocComment

namespace AL.Core.Geometry
{
    /// <summary>
    ///     <inheritdoc cref="IOriented" />
    /// </summary>
    /// <param name="X">The x coordinate.</param>
    /// <param name="Y">The y coordinate.</param>
    /// <param name="Direction">The direction the object is oriented towards.</param>
    /// <seealso cref="AL.Core.Interfaces.IOriented" />
    /// <seealso cref="AL.Core.Interfaces.IPoint" />
    public record Orientation(
        [property: JsonArrayIndex(0)] float X,
        [property: JsonArrayIndex(1)] float Y,
        [property: JsonArrayIndex(2)] Direction Direction) : IOriented
    {
        /// <summary>
        ///     This represents an invalid value since the default value of a point <c>(0, 0)</c> is a used value.
        /// </summary>
        public static readonly Orientation None = new(float.MaxValue, float.MaxValue, Direction.Invalid);
    }
}