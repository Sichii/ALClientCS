using System.Collections.Generic;

namespace AL.Data.Tokens
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class TokensDatum : DatumBase<IReadOnlyDictionary<string, float>>
    {
        public IReadOnlyDictionary<string, float> FunToken { get; set; } = new Dictionary<string, float>();
        public IReadOnlyDictionary<string, float> MonsterToken { get; set; } = new Dictionary<string, float>();
        public IReadOnlyDictionary<string, float> PvPToken { get; set; } = new Dictionary<string, float>();
    }
}