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
        long LastDeltaTime { get; }

        /// <summary>
        ///     An update method that gets called with a differential delta.
        /// </summary>
        /// <param name="deltaTime">A differential delta value.</param>
        void Update(long deltaTime);
    }
}