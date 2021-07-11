using AL.Core.Definitions;

namespace AL.Core.Model
{
    /// <summary>
    ///     Represents a mutation on an <see cref="Interfaces.IMutable" /> object that involves an <see cref="ALAttribute" />.
    /// </summary>
    public readonly struct Mutation
    {
        /// <summary>
        ///     The attribute being mutated.
        /// </summary>
        public ALAttribute Attribute { get; }
        /// <summary>
        ///     How much it is being mutated by.
        /// </summary>
        public float Mutator { get; }

        public Mutation(ALAttribute attribute, float mutator)
        {
            Attribute = attribute;
            Mutator = mutator;
        }
    }
}