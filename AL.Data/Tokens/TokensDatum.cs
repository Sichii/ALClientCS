using System.Collections.Generic;

namespace AL.Data.Tokens
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class TokensDatum : DatumBase<IReadOnlyDictionary<string, float>>
    {
        public IReadOnlyDictionary<string, float> Funtoken { get; init; } = null!;
        public IReadOnlyDictionary<string, float> Monstertoken { get; init; } = null!;
        public IReadOnlyDictionary<string, float> Pvptoken { get; init; } = null!;
    }
}