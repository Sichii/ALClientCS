using System;

namespace AL.Core.Interfaces
{
    public interface IMutable
    {
        string Id { get; }

        void Mutate(object other);
    }

    public interface IMutable<T> : IEquatable<T>, IMutable
    {
        void Mutate(T other);
    }
}