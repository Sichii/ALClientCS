using System;

namespace AL.Core.Interfaces
{
    /// <summary>
    ///     Represent an object that is mutable. <br />
    ///     <b>Anything that inherits from this should implement <see cref="IEquatable{T}" /></b>.
    /// </summary>
    public interface IMutable
    {
        /// <summary>
        ///     A mutable object should have an Id to override .Equals and .GetHashCode with.
        /// </summary>
        string Id { get; }
    }

    /// <summary>
    ///     An object that is mutated by <typeparamref name="TMutator"/>. <br />
    ///     This exists to create a self-documenting pattern of interactions between objects. <br />
    ///     <b>Anything that inherits from this should implement <see cref="IEquatable{T}" /></b>.
    /// </summary>
    /// <typeparam name="TMutator"></typeparam>
    public interface IMutable<in TMutator> : IMutable
    {
        /// <summary>
        ///     The interaction between a mutable object, and the context that mutates it.
        /// </summary>
        /// <param name="mutator">The object containing information used to mutate this object.</param>
        void Mutate(TMutator mutator);
    }
}