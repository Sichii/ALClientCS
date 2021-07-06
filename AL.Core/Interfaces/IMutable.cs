using System;

namespace AL.Core.Interfaces
{
    public interface IMutable
    {
        string Id { get; }

        void Mutate(object mutator);
    }

    public interface IMutable<in TMutator> : IMutable
    {
        void Mutate(TMutator mutator);
    }
}