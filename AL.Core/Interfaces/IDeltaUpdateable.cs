using Newtonsoft.Json;

namespace AL.Core.Interfaces
{
    /// <summary>
    ///     Represents an object interpolated via <see cref="Helpers.DeltaTime" />. <br />
    ///     <inheritdoc cref="IMutable" />
    /// </summary>
    /// <seealso cref="IMutable" />
    public interface IDeltaUpdateable : IMutable
    {
        /// <summary>
        ///     Historical delta information.
        /// </summary>
        [JsonIgnore]
        long Delta { get; set; }

        /// <summary>
        ///     An update method that gets called with a differential delta.
        /// </summary>
        /// <param name="delta">A differential delta value.</param>
        void Update(long delta);
    }
}