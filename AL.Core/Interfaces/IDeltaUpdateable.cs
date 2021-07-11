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
        [JsonIgnore]
        long Delta { get; set; }
        void Update(long delta);
    }
}