using AL.Core.Definitions;

namespace AL.Core.Model
{
    public readonly struct Mutation
    {
        public ALAttribute Attribute { get; }
        public float Mutator { get; }

        public Mutation(ALAttribute attribute, float mutator)
        {
            Attribute = attribute;
            Mutator = mutator;
        }
    }
}