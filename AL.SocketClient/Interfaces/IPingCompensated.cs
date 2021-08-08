using System;
using AL.Core.Interfaces;

namespace AL.SocketClient.Interfaces
{
    /// <summary>
    ///     Provides an interface for objects that are offset to be pseudo-equivalent to what's on the server.
    /// </summary>
    public interface IPingCompensated : IMutable
    {
        /// <summary>
        ///     Whether or not this object has already been compensated.
        /// </summary>
        bool IsCompensated { get; }

        /// <summary>
        ///     Compensates the object to be pseudo-equivalent to what's on the server.
        /// </summary>
        /// <param name="minimumOffsetMS">
        ///     A minimized offset, so as to not overcompensate. <br />
        ///     We want to be closer to the truth, but not overcompensate.
        /// </param>
        /// <exception cref="InvalidOperationException">Object already compensated.</exception>
        void CompensateOnce(int minimumOffsetMS);
    }
}