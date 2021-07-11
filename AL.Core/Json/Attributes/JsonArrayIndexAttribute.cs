using System;

namespace AL.Core.Json.Attributes
{
    /// <summary>
    ///     Provides an index to a property or field for use with <see cref="Json.Converters.ArrayToObjectConverter{T}" />.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class JsonArrayIndexAttribute : Attribute
    {
        /// <summary>
        ///     The index of the property within the array.
        /// </summary>
        public int Index { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonArrayIndexAttribute" /> class.
        /// </summary>
        /// <param name="index">The index.</param>
        public JsonArrayIndexAttribute(int index) => Index = index;
    }
}